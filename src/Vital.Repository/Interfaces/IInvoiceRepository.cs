using Vital.Domain.Entities;

namespace Vital.Repository.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int invoiceId);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<Invoice> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(int invoiceId);
    Task<string> GenerateInvoiceNumberAsync();
}
