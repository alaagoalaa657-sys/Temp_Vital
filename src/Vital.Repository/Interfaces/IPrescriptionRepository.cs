using Vital.Domain.Entities;

namespace Vital.Repository.Interfaces;

public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdAsync(int prescriptionId);
    Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Prescription>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Prescription>> GetByVisitIdAsync(int visitId);
    Task<Prescription> CreateAsync(Prescription prescription);
    Task<Prescription> UpdateAsync(Prescription prescription);
    Task<bool> DeleteAsync(int prescriptionId);
}
