# Vital - Clinic Management System

A comprehensive web-based clinic management system built with C# Blazor Server and SQL Server. This system digitizes and streamlines the daily operations of medical clinics, managing patient records, appointments, billing, and inventory.

## Features

- **Patient Management**: Maintain comprehensive patient records including demographics, medical history, allergies, and contact information
- **Appointment Scheduling**: Schedule and manage appointments with conflict detection and availability checking
- **Prescription Management**: Create and manage prescriptions linked to patient visits with automatic inventory deduction
- **Billing & Invoicing**: Generate invoices with automatic calculation of totals, taxes, and discounts
- **Inventory Management**: Track medical supplies and medications with low stock alerts and expiry date monitoring
- **Role-Based Access Control**: Separate interfaces for Administrators, Doctors, and Receptionists
- **Authentication & Security**: Secure password hashing using PBKDF2 with salt

## Technology Stack

- **Frontend**: Blazor Server (Interactive Server Components)
- **Backend**: .NET 10.0, C#
- **Database**: SQL Server / LocalDB
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Cookie Authentication

## Architecture

The application follows a layered architecture pattern:

- **Vital.Web**: User interface layer (Blazor components and pages)
- **Vital.BusinessLogic**: Business logic layer (Managers with validation and business rules)
- **Vital.Repository**: Data access layer (Repository pattern with EF Core)
- **Vital.Domain**: Domain entities (POCOs representing database models)

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code with C# extensions

### Installation

1. Clone the repository:
```bash
git clone https://github.com/alaagoalaa657-sys/Temp_Vital.git
cd Temp_Vital
```

2. Update the connection string in `src/Vital.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VitalClinicDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

3. Build the solution:
```bash
dotnet build
```

4. Run the application:
```bash
cd src/Vital.Web
dotnet run
```

5. Open your browser and navigate to `https://localhost:5001` (or the port shown in the console)

### Default Login Credentials

The application seeds the database with sample data including the following users:

- **Administrator**
  - Username: `admin`
  - Password: `admin123`

- **Doctor (Cardiologist)**
  - Username: `dr.smith`
  - Password: `doctor123`

- **Doctor (General Practitioner)**
  - Username: `dr.jones`
  - Password: `doctor123`

- **Receptionist**
  - Username: `receptionist`
  - Password: `reception123`

## User Roles & Permissions

### Administrator
- Full access to all features
- Manage inventory
- View reports and statistics
- Manage users (future enhancement)

### Doctor
- View and manage patient records
- Create prescriptions
- View appointments
- Update visit information

### Receptionist
- Manage patient records
- Schedule appointments
- Generate invoices
- Process payments

## Database Schema

The system includes the following main entities:

- **Users**: System users with authentication credentials
- **Doctors**: Doctor profiles linked to users
- **Receptionists**: Receptionist profiles linked to users
- **Patients**: Patient demographic and medical information
- **Appointments**: Scheduled appointments between patients and doctors
- **PatientVisits**: Record of patient consultations
- **Prescriptions**: Medications prescribed during visits
- **Invoices**: Billing information for services rendered
- **InventoryItems**: Medical supplies and medications

## Project Structure

```
Temp_Vital/
├── src/
│   ├── Vital.Web/                    # Blazor web application
│   │   ├── Components/
│   │   │   ├── Pages/               # Razor pages
│   │   │   └── Layout/              # Layout components
│   │   ├── Data/                    # Database initialization
│   │   └── Program.cs               # Application entry point
│   ├── Vital.BusinessLogic/         # Business logic layer
│   │   └── Managers/                # Business logic managers
│   ├── Vital.Repository/            # Data access layer
│   │   ├── Data/                    # DbContext
│   │   ├── Interfaces/              # Repository interfaces
│   │   └── Implementations/         # Repository implementations
│   └── Vital.Domain/                # Domain entities
│       └── Entities/                # Entity classes
└── Vital.sln                        # Solution file
```

## Key Features Detail

### Appointment Scheduling
- Conflict detection prevents double-booking
- Available time slot suggestions
- Support for multiple appointment statuses (Scheduled, Confirmed, In Progress, Completed, Cancelled)
- 30-minute appointment slots from 9 AM to 5 PM

### Prescription Management
- Link prescriptions to patient visits
- Automatic inventory deduction when prescriptions are created
- Support for multiple medications per prescription
- Dosage, frequency, and duration tracking

### Billing System
- Auto-generated invoice numbers (format: INV-YYYYMM-XXXX)
- Automatic calculation of subtotals, taxes, discounts
- Multiple invoice statuses (Pending, Paid, Partially Paid, Cancelled, Overdue)
- Payment method tracking

### Inventory Management
- Low stock alerts when quantity falls below minimum threshold
- Expiry date tracking with warnings for expiring items
- Support for multiple item categories (Medicine, Equipment, Supplies, Consumable)
- Stock level adjustments

## Development

### Adding New Migrations

After modifying entities, create a new migration:

```bash
cd src/Vital.Web
dotnet ef migrations add <MigrationName> --project ../Vital.Repository/Vital.Repository.csproj --startup-project Vital.Web.csproj
```

### Updating the Database

Apply pending migrations:

```bash
cd src/Vital.Web
dotnet ef database update --project ../Vital.Repository/Vital.Repository.csproj --startup-project Vital.Web.csproj
```

## Security Considerations

- Passwords are hashed using PBKDF2 with 100,000 iterations and random salt
- Cookie-based authentication with configurable expiration
- Role-based authorization on pages and components
- SQL injection prevention through parameterized queries (EF Core)
- XSS protection through Blazor's automatic encoding

## Future Enhancements

- [ ] User management interface for admins
- [ ] Advanced reporting and analytics
- [ ] Email notifications for appointments
- [ ] SMS reminders
- [ ] Patient portal for self-service
- [ ] Electronic Medical Records (EMR) integration
- [ ] Backup and restore functionality
- [ ] Audit logging
- [ ] Multi-language support
- [ ] Mobile app

## Contributing

This is an educational/demo project. Feel free to fork and modify as needed.

## License

This project is open source and available for educational purposes.

## Support

For issues or questions, please open an issue on the GitHub repository.
