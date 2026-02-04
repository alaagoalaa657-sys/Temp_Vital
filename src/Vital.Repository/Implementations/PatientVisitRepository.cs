using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class PatientVisitRepository : IPatientVisitRepository
{
    private readonly VitalDbContext _context;

    public PatientVisitRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<PatientVisit?> GetByIdAsync(int visitId)
    {
        return await _context.PatientVisits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .Include(v => v.Appointment)
            .Include(v => v.Prescriptions)
            .ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);
    }

    public async Task<IEnumerable<PatientVisit>> GetByPatientIdAsync(int patientId)
    {
        return await _context.PatientVisits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .Include(v => v.Appointment)
            .Include(v => v.Prescriptions)
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PatientVisit>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.PatientVisits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .ThenInclude(d => d.User)
            .Include(v => v.Appointment)
            .Include(v => v.Prescriptions)
            .Where(v => v.DoctorId == doctorId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<PatientVisit> CreateAsync(PatientVisit visit)
    {
        _context.PatientVisits.Add(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task<PatientVisit> UpdateAsync(PatientVisit visit)
    {
        _context.PatientVisits.Update(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task<bool> DeleteAsync(int visitId)
    {
        var visit = await _context.PatientVisits.FindAsync(visitId);
        if (visit == null) return false;

        _context.PatientVisits.Remove(visit);
        await _context.SaveChangesAsync();
        return true;
    }
}
