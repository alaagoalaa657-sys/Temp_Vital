using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly VitalDbContext _context;

    public AppointmentRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(int appointmentId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan time, int? excludeAppointmentId = null)
    {
        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId && 
                       a.AppointmentDate.Date == date.Date &&
                       a.Status != AppointmentStatus.Cancelled &&
                       a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
        }

        var appointments = await query.ToListAsync();

        const int defaultDuration = 30;
        foreach (var appointment in appointments)
        {
            var appointmentStart = appointment.AppointmentTime;
            var appointmentEnd = appointmentStart.Add(TimeSpan.FromMinutes(appointment.DurationMinutes));
            var newEnd = time.Add(TimeSpan.FromMinutes(defaultDuration));

            if ((time >= appointmentStart && time < appointmentEnd) ||
                (newEnd > appointmentStart && newEnd <= appointmentEnd) ||
                (time <= appointmentStart && newEnd >= appointmentEnd))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.UtcNow;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<bool> DeleteAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null) return false;

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }
}
