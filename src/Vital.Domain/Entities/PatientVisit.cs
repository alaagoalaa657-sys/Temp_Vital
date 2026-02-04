namespace Vital.Domain.Entities;

public class PatientVisit
{
    public int VisitId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string VitalSigns { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
