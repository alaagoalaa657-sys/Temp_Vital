using Vital.Domain.Entities;

namespace Vital.Repository.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int doctorId);
    Task<Doctor?> GetByUserIdAsync(int userId);
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
    Task<Doctor> CreateAsync(Doctor doctor);
    Task<Doctor> UpdateAsync(Doctor doctor);
    Task<bool> DeleteAsync(int doctorId);
}
