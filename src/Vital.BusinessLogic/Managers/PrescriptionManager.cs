using Vital.Domain.Entities;
using Vital.Repository.Interfaces;

namespace Vital.BusinessLogic.Managers;

/// <summary>
/// Business logic for prescription management
/// </summary>
public class PrescriptionManager
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public PrescriptionManager(
        IPrescriptionRepository prescriptionRepository,
        IInventoryRepository inventoryRepository)
    {
        _prescriptionRepository = prescriptionRepository ?? throw new ArgumentNullException(nameof(prescriptionRepository));
        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
    }

    /// <summary>
    /// Creates a new prescription and updates inventory quantities
    /// </summary>
    /// <param name="prescription">Prescription entity</param>
    /// <returns>Created prescription with ID</returns>
    /// <exception cref="ArgumentNullException">Thrown when prescription is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
    public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
    {
        if (prescription == null)
            throw new ArgumentNullException(nameof(prescription));

        ValidatePrescription(prescription);

        if (prescription.Items == null || !prescription.Items.Any())
            throw new ArgumentException("Prescription must have at least one item", nameof(prescription.Items));

        // Validate inventory and check stock availability
        foreach (var item in prescription.Items)
        {
            if (item.InventoryItemId.HasValue)
            {
                var inventoryItem = await _inventoryRepository.GetByIdAsync(item.InventoryItemId.Value);
                
                if (inventoryItem == null)
                    throw new InvalidOperationException($"Inventory item with ID {item.InventoryItemId.Value} does not exist");

                if (inventoryItem.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {inventoryItem.ItemName}. Available: {inventoryItem.Quantity}, Required: {item.Quantity}");
            }

            if (string.IsNullOrWhiteSpace(item.MedicationName))
                throw new ArgumentException("Medication name is required for all prescription items");

            if (string.IsNullOrWhiteSpace(item.Dosage))
                throw new ArgumentException("Dosage is required for all prescription items");

            if (string.IsNullOrWhiteSpace(item.Frequency))
                throw new ArgumentException("Frequency is required for all prescription items");

            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero for all prescription items");

            if (item.DurationDays <= 0)
                throw new ArgumentException("Duration must be greater than zero for all prescription items");
        }

        prescription.PrescriptionDate = DateTime.UtcNow;

        // Create the prescription
        var createdPrescription = await _prescriptionRepository.CreateAsync(prescription);

        // Update inventory quantities
        foreach (var item in prescription.Items)
        {
            if (item.InventoryItemId.HasValue)
            {
                await _inventoryRepository.UpdateQuantityAsync(item.InventoryItemId.Value, -item.Quantity);
            }
        }

        return createdPrescription;
    }

    /// <summary>
    /// Gets all prescriptions for a specific patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Collection of prescriptions</returns>
    /// <exception cref="ArgumentException">Thrown when patientId is invalid</exception>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByPatientAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(patientId));

        return await _prescriptionRepository.GetByPatientIdAsync(patientId);
    }

    /// <summary>
    /// Gets all prescriptions created by a specific doctor
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <returns>Collection of prescriptions</returns>
    /// <exception cref="ArgumentException">Thrown when doctorId is invalid</exception>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByDoctorAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than zero", nameof(doctorId));

        return await _prescriptionRepository.GetByDoctorIdAsync(doctorId);
    }

    private void ValidatePrescription(Prescription prescription)
    {
        if (prescription.PatientId <= 0)
            throw new ArgumentException("Patient ID must be greater than zero", nameof(prescription.PatientId));

        if (prescription.DoctorId <= 0)
            throw new ArgumentException("Doctor ID must be greater than zero", nameof(prescription.DoctorId));

        if (prescription.VisitId <= 0)
            throw new ArgumentException("Visit ID must be greater than zero", nameof(prescription.VisitId));
    }
}
