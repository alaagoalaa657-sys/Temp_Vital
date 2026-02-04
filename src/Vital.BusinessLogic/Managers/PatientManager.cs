using Vital.Domain.Entities;
using Vital.Repository.Interfaces;

namespace Vital.BusinessLogic.Managers;

/// <summary>
/// Business logic for patient management
/// </summary>
public class PatientManager
{
    private readonly IPatientRepository _patientRepository;

    public PatientManager(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    /// <summary>
    /// Gets a patient by their ID
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Patient entity or null if not found</returns>
    /// <exception cref="ArgumentException">Thrown when patientId is invalid</exception>
    public async Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(patientId));

        return await _patientRepository.GetByIdAsync(patientId);
    }

    /// <summary>
    /// Gets all patients
    /// </summary>
    /// <returns>Collection of all patients</returns>
    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return await _patientRepository.GetAllAsync();
    }

    /// <summary>
    /// Searches patients by name, email, or phone number
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Collection of matching patients</returns>
    /// <exception cref="ArgumentException">Thrown when searchTerm is empty</exception>
    public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

        return await _patientRepository.SearchAsync(searchTerm);
    }

    /// <summary>
    /// Creates a new patient with validation
    /// </summary>
    /// <param name="patient">Patient entity</param>
    /// <returns>Created patient with ID</returns>
    /// <exception cref="ArgumentNullException">Thrown when patient is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        ValidatePatient(patient);

        patient.CreatedAt = DateTime.UtcNow;
        patient.UpdatedAt = null;

        return await _patientRepository.CreateAsync(patient);
    }

    /// <summary>
    /// Updates an existing patient
    /// </summary>
    /// <param name="patient">Patient entity with updated data</param>
    /// <returns>Updated patient</returns>
    /// <exception cref="ArgumentNullException">Thrown when patient is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when patient does not exist</exception>
    public async Task<Patient> UpdatePatientAsync(Patient patient)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        if (patient.PatientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(patient.PatientId));

        ValidatePatient(patient);

        var existingPatient = await _patientRepository.GetByIdAsync(patient.PatientId);
        if (existingPatient == null)
            throw new InvalidOperationException($"Patient with ID {patient.PatientId} does not exist");

        patient.UpdatedAt = DateTime.UtcNow;

        return await _patientRepository.UpdateAsync(patient);
    }

    /// <summary>
    /// Deletes a patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when patientId is invalid</exception>
    public async Task<bool> DeletePatientAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(patientId));

        return await _patientRepository.DeleteAsync(patientId);
    }

    private void ValidatePatient(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
            throw new ArgumentException("Patient full name is required", nameof(patient.FullName));

        if (patient.DateOfBirth == default || patient.DateOfBirth > DateTime.Today)
            throw new ArgumentException("Invalid date of birth", nameof(patient.DateOfBirth));

        if (string.IsNullOrWhiteSpace(patient.Gender))
            throw new ArgumentException("Gender is required", nameof(patient.Gender));

        if (string.IsNullOrWhiteSpace(patient.PhoneNumber))
            throw new ArgumentException("Phone number is required", nameof(patient.PhoneNumber));

        if (string.IsNullOrWhiteSpace(patient.Email))
            throw new ArgumentException("Email is required", nameof(patient.Email));

        if (!IsValidEmail(patient.Email))
            throw new ArgumentException("Invalid email format", nameof(patient.Email));
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
