namespace Vital.Domain.Entities;

public class Prescription
{
    public int PrescriptionId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int VisitId { get; set; }
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public PatientVisit Visit { get; set; } = null!;
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    public int PrescriptionId { get; set; }
    public int? InventoryItemId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int Quantity { get; set; }
    public string Instructions { get; set; } = string.Empty;
    
    // Navigation properties
    public Prescription Prescription { get; set; } = null!;
    public InventoryItem? InventoryItem { get; set; }
}
