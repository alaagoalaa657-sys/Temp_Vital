using Vital.Domain.Entities;
using Vital.Repository.Interfaces;

namespace Vital.BusinessLogic.Managers;

/// <summary>
/// Business logic for billing and invoice management
/// </summary>
public class InvoiceManager
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPatientRepository _patientRepository;

    public InvoiceManager(
        IInvoiceRepository invoiceRepository,
        IPatientRepository patientRepository)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    /// <summary>
    /// Creates a new invoice with auto-generated invoice number and calculated totals
    /// </summary>
    /// <param name="invoice">Invoice entity</param>
    /// <returns>Created invoice with ID and generated invoice number</returns>
    /// <exception cref="ArgumentNullException">Thrown when invoice is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        ValidateInvoice(invoice);

        // Check if patient exists
        var patientExists = await _patientRepository.ExistsAsync(invoice.PatientId);
        if (!patientExists)
            throw new InvalidOperationException($"Patient with ID {invoice.PatientId} does not exist");

        // Validate invoice items
        if (invoice.Items == null || !invoice.Items.Any())
            throw new ArgumentException("Invoice must have at least one item", nameof(invoice.Items));

        foreach (var item in invoice.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Description))
                throw new ArgumentException("Item description is required for all invoice items");

            if (item.Quantity <= 0)
                throw new ArgumentException("Item quantity must be greater than zero");

            if (item.UnitPrice < 0)
                throw new ArgumentException("Item unit price cannot be negative");

            item.Total = item.Quantity * item.UnitPrice;
        }

        // Generate invoice number
        invoice.InvoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync();

        // Calculate totals
        invoice.SubTotal = invoice.Items.Sum(i => i.Total);
        
        // Ensure discount is not greater than subtotal
        if (invoice.Discount > invoice.SubTotal)
            throw new ArgumentException("Discount cannot be greater than subtotal", nameof(invoice.Discount));

        invoice.TotalAmount = invoice.SubTotal + invoice.Tax - invoice.Discount;

        invoice.InvoiceDate = DateTime.UtcNow;
        invoice.Status = InvoiceStatus.Pending;
        invoice.PaidAt = null;

        return await _invoiceRepository.CreateAsync(invoice);
    }

    /// <summary>
    /// Updates an existing invoice
    /// </summary>
    /// <param name="invoice">Invoice entity with updated data</param>
    /// <returns>Updated invoice</returns>
    /// <exception cref="ArgumentNullException">Thrown when invoice is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        if (invoice.InvoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(invoice.InvoiceId));

        var existingInvoice = await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);
        if (existingInvoice == null)
            throw new InvalidOperationException($"Invoice with ID {invoice.InvoiceId} does not exist");

        if (existingInvoice.Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot update a paid invoice");

        if (existingInvoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled invoice");

        ValidateInvoice(invoice);

        // Recalculate totals
        if (invoice.Items != null && invoice.Items.Any())
        {
            foreach (var item in invoice.Items)
            {
                item.Total = item.Quantity * item.UnitPrice;
            }

            invoice.SubTotal = invoice.Items.Sum(i => i.Total);
        }

        if (invoice.Discount > invoice.SubTotal)
            throw new ArgumentException("Discount cannot be greater than subtotal", nameof(invoice.Discount));

        invoice.TotalAmount = invoice.SubTotal + invoice.Tax - invoice.Discount;

        return await _invoiceRepository.UpdateAsync(invoice);
    }

    /// <summary>
    /// Marks an invoice as paid
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="paymentMethod">Payment method used</param>
    /// <returns>Updated invoice</returns>
    /// <exception cref="ArgumentException">Thrown when invoiceId is invalid or paymentMethod is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when invoice does not exist or is already paid</exception>
    public async Task<Invoice> MarkAsPaidAsync(int invoiceId, string paymentMethod)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(invoiceId));

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method is required", nameof(paymentMethod));

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {invoiceId} does not exist");

        if (invoice.Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already marked as paid");

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark a cancelled invoice as paid");

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.PaymentMethod = paymentMethod;

        return await _invoiceRepository.UpdateAsync(invoice);
    }

    /// <summary>
    /// Gets all invoices
    /// </summary>
    /// <returns>Collection of all invoices</returns>
    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _invoiceRepository.GetAllAsync();
    }

    /// <summary>
    /// Gets all invoices for a specific patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Collection of invoices</returns>
    /// <exception cref="ArgumentException">Thrown when patientId is invalid</exception>
    public async Task<IEnumerable<Invoice>> GetInvoicesByPatientAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(patientId));

        return await _invoiceRepository.GetByPatientIdAsync(patientId);
    }

    /// <summary>
    /// Gets all pending invoices
    /// </summary>
    /// <returns>Collection of pending invoices</returns>
    public async Task<IEnumerable<Invoice>> GetPendingInvoicesAsync()
    {
        return await _invoiceRepository.GetByStatusAsync(InvoiceStatus.Pending);
    }

    private void ValidateInvoice(Invoice invoice)
    {
        if (invoice.PatientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(invoice.PatientId));

        if (invoice.SubTotal < 0)
            throw new ArgumentException("Subtotal cannot be negative", nameof(invoice.SubTotal));

        if (invoice.Tax < 0)
            throw new ArgumentException("Tax cannot be negative", nameof(invoice.Tax));

        if (invoice.Discount < 0)
            throw new ArgumentException("Discount cannot be negative", nameof(invoice.Discount));

        if (invoice.TotalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(invoice.TotalAmount));
    }
}
