using Vital.Domain.Entities;

namespace Vital.Repository.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int patientId);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(int patientId);
    Task<bool> ExistsAsync(int patientId);
}
