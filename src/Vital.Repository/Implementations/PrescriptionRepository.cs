using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly VitalDbContext _context;

    public PrescriptionRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<Prescription?> GetByIdAsync(int prescriptionId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Include(p => p.Visit)
            .Include(p => p.Items)
            .ThenInclude(i => i.InventoryItem)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);
    }

    public async Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Include(p => p.Visit)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescriptionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prescription>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Include(p => p.Visit)
            .Include(p => p.Items)
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.PrescriptionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prescription>> GetByVisitIdAsync(int visitId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Include(p => p.Visit)
            .Include(p => p.Items)
            .ThenInclude(i => i.InventoryItem)
            .Where(p => p.VisitId == visitId)
            .OrderByDescending(p => p.PrescriptionDate)
            .ToListAsync();
    }

    public async Task<Prescription> CreateAsync(Prescription prescription)
    {
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Prescription> UpdateAsync(Prescription prescription)
    {
        _context.Prescriptions.Update(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<bool> DeleteAsync(int prescriptionId)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null) return false;

        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();
        return true;
    }
}
