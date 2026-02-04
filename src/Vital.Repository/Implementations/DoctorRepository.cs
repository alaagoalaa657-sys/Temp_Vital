using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class DoctorRepository : IDoctorRepository
{
    private readonly VitalDbContext _context;

    public DoctorRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdAsync(int doctorId)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Appointments)
            .Include(d => d.Prescriptions)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
    }

    public async Task<Doctor?> GetByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .OrderBy(d => d.User.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Where(d => d.IsAvailable)
            .OrderBy(d => d.User.FullName)
            .ToListAsync();
    }

    public async Task<Doctor> CreateAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task<Doctor> UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task<bool> DeleteAsync(int doctorId)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null) return false;

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();
        return true;
    }
}
