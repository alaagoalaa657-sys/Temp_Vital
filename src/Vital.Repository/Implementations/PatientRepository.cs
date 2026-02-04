using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class PatientRepository : IPatientRepository
{
    private readonly VitalDbContext _context;

    public PatientRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.Visits)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _context.Patients.OrderBy(p => p.FullName).ToListAsync();
    }

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.FullName.Contains(searchTerm) || 
                       p.PhoneNumber.Contains(searchTerm) ||
                       p.Email.Contains(searchTerm))
            .OrderBy(p => p.FullName)
            .ToListAsync();
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        patient.UpdatedAt = DateTime.UtcNow;
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<bool> DeleteAsync(int patientId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null) return false;

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int patientId)
    {
        return await _context.Patients.AnyAsync(p => p.PatientId == patientId);
    }
}
