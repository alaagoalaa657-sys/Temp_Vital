using Vital.Domain.Entities;
using Vital.Repository.Interfaces;

namespace Vital.BusinessLogic.Managers;

/// <summary>
/// Business logic for appointment scheduling and management
/// </summary>
public class AppointmentManager
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentManager(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
        _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
    }

    /// <summary>
    /// Schedules a new appointment with validation and conflict checking
    /// </summary>
    /// <param name="appointment">Appointment entity</param>
    /// <returns>Created appointment with ID</returns>
    /// <exception cref="ArgumentNullException">Thrown when appointment is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public async Task<Appointment> ScheduleAppointmentAsync(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment));

        await ValidateAppointmentAsync(appointment);

        // Check if patient exists
        var patientExists = await _patientRepository.ExistsAsync(appointment.PatientId);
        if (!patientExists)
            throw new InvalidOperationException($"Patient with ID {appointment.PatientId} does not exist");

        // Check if doctor exists
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {appointment.DoctorId} does not exist");

        // Check for time conflicts
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            appointment.DoctorId,
            appointment.AppointmentDate,
            appointment.AppointmentTime,
            null);

        if (hasConflict)
            throw new InvalidOperationException(
                $"Doctor already has an appointment at {appointment.AppointmentDate:yyyy-MM-dd} {appointment.AppointmentTime}");

        appointment.Status = AppointmentStatus.Scheduled;
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.UpdatedAt = null;

        return await _appointmentRepository.CreateAsync(appointment);
    }

    /// <summary>
    /// Updates an existing appointment
    /// </summary>
    /// <param name="appointment">Appointment entity with updated data</param>
    /// <returns>Updated appointment</returns>
    /// <exception cref="ArgumentNullException">Thrown when appointment is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment));

        if (appointment.AppointmentId <= 0)
            throw new ArgumentException("Appointment ID must be greater than zero", nameof(appointment.AppointmentId));

        var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.AppointmentId);
        if (existingAppointment == null)
            throw new InvalidOperationException($"Appointment with ID {appointment.AppointmentId} does not exist");

        if (existingAppointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled appointment");

        await ValidateAppointmentAsync(appointment);

        // Check for time conflicts (excluding current appointment)
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            appointment.DoctorId,
            appointment.AppointmentDate,
            appointment.AppointmentTime,
            appointment.AppointmentId);

        if (hasConflict)
            throw new InvalidOperationException(
                $"Doctor already has an appointment at {appointment.AppointmentDate:yyyy-MM-dd} {appointment.AppointmentTime}");

        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    /// <summary>
    /// Cancels an appointment
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Updated appointment</returns>
    /// <exception cref="ArgumentException">Thrown when appointmentId is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when appointment does not exist or is already cancelled</exception>
    public async Task<Appointment> CancelAppointmentAsync(int appointmentId)
    {
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment ID must be greater than zero", nameof(appointmentId));

        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment with ID {appointmentId} does not exist");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed appointment");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    /// <summary>
    /// Gets all appointments for a specific patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Collection of appointments</returns>
    /// <exception cref="ArgumentException">Thrown when patientId is invalid</exception>
    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(patientId));

        return await _appointmentRepository.GetByPatientIdAsync(patientId);
    }

    /// <summary>
    /// Gets all appointments for a specific doctor
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <returns>Collection of appointments</returns>
    /// <exception cref="ArgumentException">Thrown when doctorId is invalid</exception>
    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than zero", nameof(doctorId));

        return await _appointmentRepository.GetByDoctorIdAsync(doctorId);
    }

    /// <summary>
    /// Gets available time slots for a doctor on a specific date
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <param name="date">Date to check availability</param>
    /// <returns>Collection of available time slots</returns>
    /// <exception cref="ArgumentException">Thrown when doctorId is invalid or date is in the past</exception>
    public async Task<IEnumerable<TimeSpan>> GetAvailableSlotsAsync(int doctorId, DateTime date)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than zero", nameof(doctorId));

        if (date.Date < DateTime.Today)
            throw new ArgumentException("Cannot get available slots for past dates", nameof(date));

        var existingAppointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, date);
        var bookedSlots = existingAppointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentTime)
            .ToHashSet();

        var availableSlots = new List<TimeSpan>();
        var startHour = 9; // 9 AM
        var endHour = 17; // 5 PM
        var slotDuration = 30; // 30 minutes

        for (int hour = startHour; hour < endHour; hour++)
        {
            for (int minute = 0; minute < 60; minute += slotDuration)
            {
                var slot = new TimeSpan(hour, minute, 0);
                if (!bookedSlots.Contains(slot))
                {
                    availableSlots.Add(slot);
                }
            }
        }

        return availableSlots;
    }

    private async Task ValidateAppointmentAsync(Appointment appointment)
    {
        if (appointment.PatientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(appointment.PatientId));

        if (appointment.DoctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than zero", nameof(appointment.DoctorId));

        if (appointment.AppointmentDate == default)
            throw new ArgumentException("Appointment date is required", nameof(appointment.AppointmentDate));

        if (appointment.AppointmentDate.Date < DateTime.Today)
            throw new ArgumentException("Cannot schedule appointments in the past", nameof(appointment.AppointmentDate));

        if (appointment.AppointmentTime < TimeSpan.Zero || appointment.AppointmentTime >= TimeSpan.FromHours(24))
            throw new ArgumentException("Invalid appointment time", nameof(appointment.AppointmentTime));

        if (appointment.DurationMinutes <= 0 || appointment.DurationMinutes > 480)
            throw new ArgumentException("Appointment duration must be between 1 and 480 minutes", nameof(appointment.DurationMinutes));

        if (string.IsNullOrWhiteSpace(appointment.Reason))
            throw new ArgumentException("Appointment reason is required", nameof(appointment.Reason));
    }
}
