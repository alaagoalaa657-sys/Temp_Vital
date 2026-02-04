using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly VitalDbContext _context;

    public InvoiceRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int invoiceId)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Visit)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Visit)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Visit)
            .Include(i => i.Items)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Visit)
            .Include(i => i.Items)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Visit)
            .Include(i => i.Items)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(int invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null) return false;

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var now = DateTime.UtcNow;
        var yearMonth = $"{now.Year}{now.Month:D2}";
        var prefix = $"INV-{yearMonth}-";

        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int sequential = 1;
        if (lastInvoice != null)
        {
            var lastNumber = lastInvoice.InvoiceNumber.Replace(prefix, "");
            if (int.TryParse(lastNumber, out int lastSequential))
            {
                sequential = lastSequential + 1;
            }
        }

        return $"{prefix}{sequential:D4}";
    }
}
