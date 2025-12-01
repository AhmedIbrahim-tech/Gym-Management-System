# 🏋️ VitaGym Portal Web

A comprehensive web application built with **ASP.NET Core MVC** following clean architecture principles to efficiently manage gym operations, including member subscriptions, trainer scheduling, session booking, attendance tracking, payment processing, and financial reporting.

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Development Guidelines](#development-guidelines)
- [License](#license)

---

## Features

### Member Management
- **Complete CRUD Operations:** Create, Read, Update, and Delete member records with comprehensive validation
- **Health Record Tracking:** Detailed health records for each member, enabling personalized training programs
- **Member Search:** Fast and efficient search functionality by phone ID or name
- **Photo Management:** Upload and manage member profile photos

### Trainer Management
- **Full CRUD Functionality:** Complete trainer lifecycle management
- **Specialty Management:** Assign and track trainer specialties and certifications
- **Scheduling:** Efficient scheduling system for trainer availability
- **Performance Tracking:** Monitor trainer performance and session assignments

### Session Management
- **Session CRUD Operations:** Create and manage training sessions with flexible scheduling
- **Booking System:** Members can book sessions with real-time availability checking
- **Category-Based Organization:** Organize sessions by workout categories
- **Capacity Management:** Track available slots and manage session capacity
- **Attendance Marking:** Mark member attendance for booked sessions

### Membership & Plans
- **Plan Management:** Create and manage membership plans with flexible duration and pricing
- **Plan Activation/Deactivation:** Dynamic control over available membership options
- **Member Membership Tracking:** Comprehensive tracking of member subscriptions and renewals
- **Pricing Flexibility:** Support for various pricing models and promotional plans
- **Membership Lifecycle:** Create, view, and cancel memberships with automatic date calculations

### Attendance Management
- **Check-In System:** Members can check in when visiting the gym
- **Membership Validation:** Automatic validation of active membership before check-in
- **Attendance History:** Track and view member attendance records
- **Member-Specific Reports:** View attendance history for individual members

### Payment Management
- **Payment Processing:** Record and track payments from members
- **Multiple Payment Methods:** Support for Cash, Card, Online, and Bank Transfer
- **Payment History:** Comprehensive payment tracking and reporting
- **Member Payment Reports:** View payment history for individual members
- **Total Payment Calculation:** Calculate total payments by member

### User Management
- **User CRUD Operations:** Create, Read, Update, and Delete system users
- **User Status Management:** Enable/disable user accounts
- **Role Assignment:** Assign roles to users (SuperAdmin, Admin, Trainer, Member)
- **Profile Management:** Users can update their own profiles with photo uploads

### Roles & Permissions
- **Role Management:** Create, edit, and delete roles with user count tracking
- **Permission-Based Authorization:** Granular permission system for fine-grained access control
- **Permission Assignment:** Assign specific permissions to roles
- **Protected Roles:** SuperAdmin role protection with automatic full permissions
- **Permission Modules:** Organized permissions by feature modules (Users, Members, Trainers, Sessions, etc.)

### Analytics & Reporting
- **Dashboard Analytics:** Real-time insights into gym operations, member statistics, and revenue
- **Session Analytics:** Track session attendance, booking rates, and trainer utilization
- **Financial Reports:** Generate comprehensive financial reports for business intelligence
- **Performance Metrics:** Monitor key performance indicators across all operations

### Security & Access Control
- **Role-Based Authorization:** Secure access control with SuperAdmin, Admin, Trainer, and Member roles
- **Permission-Based Authorization:** Fine-grained access control using custom permission attributes
- **Microsoft Identity Integration:** Enterprise-grade authentication and authorization
- **Session Security:** Protected endpoints with proper authorization attributes
- **Global Exception Handling:** Centralized error handling middleware for consistent error responses

---

## Technology Stack

### Backend & Framework

| Technology | Version | Purpose |
|:-----------|:--------|:--------|
| ASP.NET Core MVC | 10.0 | Web application framework and MVC pattern implementation |
| .NET | 10.0 | Runtime and framework for building modern applications |
| C# | Latest | Primary programming language |

### Database & Data Access

| Technology | Version | Purpose |
|:-----------|:--------|:--------|
| SQL Server | Latest | Primary relational database management system |
| Entity Framework Core | 10.0.0 | Object-Relational Mapping (ORM) for database operations |
| Code First Migrations | 10.0.0 | Database schema management and versioning |

### Architecture & Design Patterns

| Pattern/Technique | Implementation | Purpose |
|:------------------|:----------------|:--------|
| Clean Architecture | Layered separation (Infrastructure, Core, Web) | Separation of concerns and maintainability |
| Module-Based Architecture | Feature modules in Core layer | Organized business logic by domain features |
| Dependency Injection | Built-in .NET DI container with extension methods | Inversion of Control for loose coupling |
| Extension Methods Pattern | EntityMappers, configuration extensions | Extending types without modification |
| Middleware Pattern | GlobalExceptionHandlingMiddleware | Cross-cutting concerns and request pipeline processing |
| Authorization Handler Pattern | PermissionAuthorizationHandler | Custom permission-based authorization logic |
| View Component Pattern | Reusable view components (UserInfo) | Encapsulated UI components with logic |
| Fluent Validation Pattern | FluentValidation validators per module | Declarative input validation |
| Configuration Extensions Pattern | ServiceCollectionExtensions, ApplicationBuilderExtensions | Organized service and middleware registration |
| Repository Pattern | Generic and specific repositories | Data access abstraction and testability |
| Unit of Work Pattern | UnitOfWork implementation | Transaction management and data consistency |

### Mapping & Utilities

| Technology | Version | Purpose |
|:-----------|:--------|:--------|
| EntityMappers | Custom | Extension methods for object-to-object mapping between ViewModels and Entities |
| Global Usings | .NET 10 | Centralized namespace imports per layer |
| FluentValidation | 11.3.1 | Fluent validation library for input validation |
| Bogus | 35.6.5 | Fake data generator for testing and seeding |

### Identity & Security

| Technology | Version | Purpose |
|:-----------|:--------|:--------|
| Microsoft Identity | 10.0.0 | Authentication and authorization framework |
| Role-Based Access Control | Built-in | Secure role management and authorization |
| Permission-Based Authorization | Custom | Fine-grained permission system with custom authorization handlers |

### Notifications

| Technology | Version | Purpose |
|:-----------|:--------|:--------|
| NToastNotify | 8.0.0 | Toast notification library for user feedback |

### Third-Party Services

| Technology | Purpose |
|:-----------|:--------|
| Email Service | Email notification service for system communications |
| Attachment Service | File upload and management service for photos and documents |

---

## Architecture

The system follows **Clean Architecture** principles with a clear separation of concerns across three main layers:

### Layer Overview

```
┌─────────────────────────────────────────────────────────┐
│                      Web Layer                          │
│  (Controllers, Views, Middleware, Authorization)       │
└──────────────────────┬──────────────────────────────────┘
                       │ depends on
┌──────────────────────▼──────────────────────────────────┐
│                     Core Layer                           │
│  (Services, ViewModels, Validators, Business Logic)      │
└──────────────────────┬──────────────────────────────────┘
                       │ depends on
┌──────────────────────▼──────────────────────────────────┐
│                Infrastructure Layer                      │
│  (DbContext, Entities, Repositories, Data Access)      │
└─────────────────────────────────────────────────────────┘
```

## 📁 Project Structure

```
VitaGymPortalWeb/
├── Infrastructure/                    # Data access and persistence layer
│   ├── Data/                         # Database context and configurations
│   │   ├── ApplicationDbContext.cs  # Main database context
│   │   ├── Configurations/          # EF Core entity configurations
│   │   ├── DataSeed/                # Initial data seeding
│   │   └── Migrations/              # Database migrations
│   ├── Entities/                     # Domain entities
│   │   ├── Users/                   # User-related entities (Member, Trainer, ApplicationUser)
│   │   ├── Sessions/                # Session and Category entities
│   │   ├── Membership/              # Membership, Plan, Payment entities
│   │   ├── Attendances/             # Attendance entity
│   │   ├── HumanResources/         # HR entities (TrainerPayroll, LeaveType, LeaveRequest)
│   │   ├── Shared/                  # BaseEntity, Address
│   │   └── Enums/                   # Enumeration types
│   ├── Repositories/                # Repository pattern implementation
│   │   ├── GenericRepository.cs     # Base repository
│   │   ├── UnitOfWork.cs            # Unit of Work pattern
│   │   └── Specific repositories    # MemberRepository, TrainerRepository, etc.
│   ├── Constants/                   # Roles and Permissions constants
│   └── GlobalUsings.cs              # Global namespace imports
│
├── Core/                             # Business logic layer
│   ├── Modules/                     # Feature-based modules
│   │   ├── Accounts/                # Authentication services
│   │   ├── Analyticals/             # Dashboard analytics
│   │   ├── Attendances/             # Attendance services
│   │   ├── Bookings/                # Booking services
│   │   ├── Members/                 # Member services and validators
│   │   ├── Memberships/             # Membership services
│   │   ├── Payments/                # Payment services
│   │   ├── Plans/                   # Plan services and validators
│   │   ├── Sessions/                # Session services and validators
│   │   ├── Trainers/                # Trainer services and validators
│   │   └── UserManagement/          # User and role management services
│   ├── Mappers/                     # Entity mapping extensions
│   │   └── EntityMappers.cs        # Extension methods for entity-to-viewmodel mapping
│   ├── ThirdParty/                  # Third-party service integrations
│   │   ├── AttachmentService/       # File upload service
│   │   └── Email/                   # Email service
│   └── GlobalUsings.cs              # Global namespace imports
│
└── Web/                              # Presentation layer
    ├── Controllers/                  # MVC controllers
    ├── Views/                        # Razor views organized by feature
    ├── Models/                       # Error handling models
    ├── Attributes/                   # Custom attributes (RequirePermissionAttribute)
    ├── Authorization/                # Custom authorization handlers
    ├── Configurations/               # Service registration extensions
    ├── Helpers/                      # Helper classes (PermissionHelper)
    ├── Middleware/                   # Global exception handling middleware
    ├── ViewComponents/               # Reusable view components
    ├── wwwroot/                     # Static files (CSS, JS, images)
    ├── Program.cs                    # Application startup
    └── GlobalUsings.cs               # Global namespace imports
```


**Built with ❤️ using ASP.NET Core MVC and Clean Architecture principles**
