using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.BusinessLogic.Managers;

namespace Vital.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VitalDbContext>();
        var authManager = scope.ServiceProvider.GetRequiredService<AuthenticationManager>();

        // Ensure database is created
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Create Users
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = authManager.HashPassword("admin123"),
            FullName = "System Administrator",
            Email = "admin@vitalclinic.com",
            PhoneNumber = "1234567890",
            Role = UserRole.Admin,
            IsActive = true
        };
        context.Users.Add(adminUser);

        var doctorUser1 = new User
        {
            Username = "dr.smith",
            PasswordHash = authManager.HashPassword("doctor123"),
            FullName = "Dr. John Smith",
            Email = "dr.smith@vitalclinic.com",
            PhoneNumber = "1234567891",
            Role = UserRole.Doctor,
            IsActive = true
        };
        context.Users.Add(doctorUser1);

        var doctorUser2 = new User
        {
            Username = "dr.jones",
            PasswordHash = authManager.HashPassword("doctor123"),
            FullName = "Dr. Sarah Jones",
            Email = "dr.jones@vitalclinic.com",
            PhoneNumber = "1234567892",
            Role = UserRole.Doctor,
            IsActive = true
        };
        context.Users.Add(doctorUser2);

        var receptionistUser = new User
        {
            Username = "receptionist",
            PasswordHash = authManager.HashPassword("reception123"),
            FullName = "Emily Brown",
            Email = "emily@vitalclinic.com",
            PhoneNumber = "1234567893",
            Role = UserRole.Receptionist,
            IsActive = true
        };
        context.Users.Add(receptionistUser);

        await context.SaveChangesAsync();

        // Create Doctors
        var doctor1 = new Doctor
        {
            UserId = doctorUser1.UserId,
            Specialization = "Cardiologist",
            LicenseNumber = "MED-12345",
            ConsultationFee = 150.00m,
            IsAvailable = true
        };
        context.Doctors.Add(doctor1);

        var doctor2 = new Doctor
        {
            UserId = doctorUser2.UserId,
            Specialization = "General Practitioner",
            LicenseNumber = "MED-12346",
            ConsultationFee = 100.00m,
            IsAvailable = true
        };
        context.Doctors.Add(doctor2);

        // Create Receptionist
        var receptionist = new Receptionist
        {
            UserId = receptionistUser.UserId,
            Department = "Front Desk"
        };
        context.Receptionists.Add(receptionist);

        await context.SaveChangesAsync();

        // Create Sample Patients
        var patients = new[]
        {
            new Patient
            {
                FullName = "Michael Johnson",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Male",
                PhoneNumber = "5551234567",
                Email = "michael.j@email.com",
                Address = "123 Main St, City, State 12345",
                EmergencyContact = "Jane Johnson - 5559876543",
                BloodType = "O+",
                Allergies = "Penicillin",
                MedicalHistory = "Hypertension, controlled with medication"
            },
            new Patient
            {
                FullName = "Lisa Anderson",
                DateOfBirth = new DateTime(1990, 8, 22),
                Gender = "Female",
                PhoneNumber = "5552345678",
                Email = "lisa.a@email.com",
                Address = "456 Oak Ave, City, State 12345",
                EmergencyContact = "Mark Anderson - 5558765432",
                BloodType = "A+",
                Allergies = "None",
                MedicalHistory = "No significant medical history"
            },
            new Patient
            {
                FullName = "Robert Wilson",
                DateOfBirth = new DateTime(1975, 3, 10),
                Gender = "Male",
                PhoneNumber = "5553456789",
                Email = "robert.w@email.com",
                Address = "789 Pine Rd, City, State 12345",
                EmergencyContact = "Susan Wilson - 5557654321",
                BloodType = "B+",
                Allergies = "Latex",
                MedicalHistory = "Type 2 Diabetes, managed with insulin"
            },
            new Patient
            {
                FullName = "Jennifer Martinez",
                DateOfBirth = new DateTime(1995, 11, 30),
                Gender = "Female",
                PhoneNumber = "5554567890",
                Email = "jennifer.m@email.com",
                Address = "321 Elm St, City, State 12345",
                EmergencyContact = "Carlos Martinez - 5556543210",
                BloodType = "AB+",
                Allergies = "Peanuts",
                MedicalHistory = "Asthma, uses inhaler as needed"
            }
        };

        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();

        // Create Sample Appointments
        var today = DateTime.Today;
        var appointments = new[]
        {
            new Appointment
            {
                PatientId = patients[0].PatientId,
                DoctorId = doctor1.DoctorId,
                AppointmentDate = today,
                AppointmentTime = new TimeSpan(9, 0, 0),
                DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Regular checkup",
                Notes = "Blood pressure monitoring"
            },
            new Appointment
            {
                PatientId = patients[1].PatientId,
                DoctorId = doctor2.DoctorId,
                AppointmentDate = today,
                AppointmentTime = new TimeSpan(10, 0, 0),
                DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Follow-up consultation",
                Notes = ""
            },
            new Appointment
            {
                PatientId = patients[2].PatientId,
                DoctorId = doctor1.DoctorId,
                AppointmentDate = today.AddDays(1),
                AppointmentTime = new TimeSpan(14, 0, 0),
                DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Diabetes management",
                Notes = "Review insulin dosage"
            }
        };

        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();

        // Create Sample Inventory Items
        var inventoryItems = new[]
        {
            new InventoryItem
            {
                ItemName = "Paracetamol 500mg",
                ItemCode = "MED-001",
                Category = ItemCategory.Medicine,
                Description = "Pain reliever and fever reducer",
                Quantity = 500,
                MinimumQuantity = 100,
                UnitPrice = 0.50m,
                Unit = "Tablet",
                Supplier = "PharmaCorp",
                ExpiryDate = DateTime.Today.AddYears(2)
            },
            new InventoryItem
            {
                ItemName = "Ibuprofen 400mg",
                ItemCode = "MED-002",
                Category = ItemCategory.Medicine,
                Description = "Anti-inflammatory medication",
                Quantity = 300,
                MinimumQuantity = 100,
                UnitPrice = 0.75m,
                Unit = "Tablet",
                Supplier = "PharmaCorp",
                ExpiryDate = DateTime.Today.AddYears(2)
            },
            new InventoryItem
            {
                ItemName = "Bandages (Sterile)",
                ItemCode = "SUP-001",
                Category = ItemCategory.Supplies,
                Description = "Sterile adhesive bandages",
                Quantity = 50,
                MinimumQuantity = 20,
                UnitPrice = 2.00m,
                Unit = "Box",
                Supplier = "MedSupply Co",
                ExpiryDate = null
            },
            new InventoryItem
            {
                ItemName = "Syringes 5ml",
                ItemCode = "SUP-002",
                Category = ItemCategory.Supplies,
                Description = "Disposable syringes",
                Quantity = 15,
                MinimumQuantity = 50,
                UnitPrice = 0.30m,
                Unit = "Piece",
                Supplier = "MedSupply Co",
                ExpiryDate = DateTime.Today.AddYears(3)
            },
            new InventoryItem
            {
                ItemName = "Surgical Gloves",
                ItemCode = "SUP-003",
                Category = ItemCategory.Supplies,
                Description = "Latex-free surgical gloves",
                Quantity = 200,
                MinimumQuantity = 100,
                UnitPrice = 0.25m,
                Unit = "Pair",
                Supplier = "MedSupply Co",
                ExpiryDate = null
            }
        };

        context.InventoryItems.AddRange(inventoryItems);
        await context.SaveChangesAsync();
    }
}
