using Vital.Domain.Entities;

namespace Vital.Repository.Interfaces;

public interface IPatientVisitRepository
{
    Task<PatientVisit?> GetByIdAsync(int visitId);
    Task<IEnumerable<PatientVisit>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<PatientVisit>> GetByDoctorIdAsync(int doctorId);
    Task<PatientVisit> CreateAsync(PatientVisit visit);
    Task<PatientVisit> UpdateAsync(PatientVisit visit);
    Task<bool> DeleteAsync(int visitId);
}
