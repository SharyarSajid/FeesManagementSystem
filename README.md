# Fees Management System

Fees Management System is an ASP.NET Core MVC application designed to manage student fee records, track payments, and handle automated notifications. It provides a comprehensive solution for educational institutions to streamline their fee collection processes with role-based access control.

## Key Features

- **Student Management**: Register, update, and track student records.
- **Fee Management**: Define fee heads (types of fees), manage student fees, and track payment statuses.
- **Role-Based Authentication (RBAC)**: Supports roles such as `Supervisor` and `Data Entry Operator` to restrict access to sensitive operations.
- **Identity & Security**: Built using ASP.NET Core Identity with secure login, password reset functionality, and an OTP verification flow.
- **Automated Notifications**: Includes a background hosted service (`FeeNotificationJob`) to automatically send email notifications for fee updates or reminders.
- **Audit Logging**: Keeps track of changes to student records (`StudentAuditLog`) and notification histories (`NotificationLog`).
- **Automation Scripts**: Comes with PowerShell scripts (e.g., `Setup-Local-Environment.ps1`, `Publish-And-Run.ps1`, `Fix-IIS-Permissions.ps1`) for local database setup, easy publishing, and configuring IIS environments.

## Technology Stack

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Database**: SQL Server accessed via Entity Framework Core 10.0.1
- **Authentication**: ASP.NET Core Identity
- **Logging**: Log4Net integration for robust application logging
- **Background Jobs**: ASP.NET Core Hosted Services (`IHostedService`)
- **Frontend**: Razor Pages/Views (`.cshtml`)

## Project Structure

- `Controllers/`: Contains the core logic routing requests (`AccountController`, `FeesController`, `HomeController`, `StudentsController`).
- `Models/`: Entity definitions including `Student`, `StudentFee`, `FeeHead`, and Identity ViewModels.
- `Data/`: Contains `ApplicationDbContext` and Entity Framework Code-First Migrations.
- `Services/`: Interfaces and implementations for core business logic, like `FeeService`, `NotificationService`, and `EmailSender`.
- `Background/`: Contains the `FeeNotificationJob` for automated asynchronous tasks.
- `Views/`: Razor views for the web interface.

## Setup & Deployment

1. **Database Configuration**: Ensure the connection string under `DefaultConnection` in `appsettings.json` points to your SQL Server instance.
2. **Email Configuration**: Configure the SMTP settings in `appsettings.json` under the `EmailSettings` node for system notifications.
3. **Environment Setup**: You can run `Setup-Local-Environment.ps1` or `Test-LocalDb.ps1` to prepare your development environment.
4. **Running Locally**: Run the project via Visual Studio or `dotnet run`. The application will automatically apply EF migrations on startup and seed default roles.
5. **IIS Deployment**: Scripts like `Publish-And-Run.ps1` and `Fix-IIS-Permissions.ps1` are provided to streamline publishing to IIS.
