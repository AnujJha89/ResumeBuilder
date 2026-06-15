# ResumeBuilder

A professional, feature-rich ASP.NET Core Web application designed to help users build, customize, and download high-quality resumes. Built with clean architecture principles and modern styling refinements.

## Contributors
- **Anuj Kumar Jha** - [@AnujJha89](https://github.com/AnujJha89)
- **Khushdeep** - [@Khushdeep00978](https://github.com/Khushdeep00978)

---

## Prerequisites & Setup Guide

To run this project locally, you need to have the following tools and packages installed.

### 1. Software & SDKs
- **.NET 8.0 SDK**: [Download & Install .NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- **SQL Server LocalDB**: Included with Visual Studio's ".NET desktop development" / "ASP.NET and web development" workloads. Or you can download [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) locally.
- **EF Core CLI Tools**: Install globally by running this command in your terminal:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2. NuGet Packages Used
All packages are automatically restored upon building the application. Below are the key packages referenced in the solution:

* **Web Project (ResumeBuilder.Web)**:
  - `Microsoft.AspNetCore.Authentication.Google` (v8.0.11) - Google OAuth integrations
  - `Microsoft.EntityFrameworkCore.Design` (v8.0.11) - EF Core design-time tools

* **Infrastructure Project (ResumeBuilder.Infrastructure)**:
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (v8.0.11) - Identity & user management
  - `Microsoft.EntityFrameworkCore` (v8.0.11) - ORM framework
  - `Microsoft.EntityFrameworkCore.SqlServer` (v8.0.11) - SQL Server database provider
  - `Microsoft.EntityFrameworkCore.Tools` (v8.0.11) - EF Core package manager commands
  - `PuppeteerSharp` (v20.0.2) - Headless Chrome API wrapper for converting HTML resumes to PDF

* **Core Project (ResumeBuilder.Core)**:
  - `Microsoft.Extensions.Identity.Stores` (v8.0.11) - Identity store abstractions

---

## How to Run the Project Locally

Follow these steps to set up and start the application on your local machine:

### Step 1: Clone the Repository
```bash
git clone https://github.com/AnujJha89/ResumeBuilder.git
cd ResumeBuilder
```

### Step 2: Restore Dependencies
Restore all required NuGet packages:
```bash
dotnet restore
```

### Step 3: Run Database Migrations
Apply the EF Core migrations to create your local SQL database (ResumeBuilderDb):
```bash
dotnet ef database update --project ResumeBuilder.Infrastructure --startup-project ResumeBuilder.Web
```

### Step 4: Run the Application
Start the web application:
```bash
dotnet run --project ResumeBuilder.Web
```

The app will compile and start. Open your browser and navigate to the local HTTPS URL shown in your console (usually https://localhost:7196 or similar).

---

*Made to simplify professional resume creation.*
