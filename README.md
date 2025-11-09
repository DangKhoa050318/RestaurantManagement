# Restaurant Management System

> A comprehensive restaurant management solution built with .NET 8 WPF, implementing MVVM architecture and Entity Framework Core

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/DangKhoa050318/RestaurantManagement)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Usage](#usage)
- [Documentation](#documentation)
- [Contributing](#contributing)

---

## Overview

**Restaurant Management System** is a desktop application designed to streamline restaurant operations including table management, order processing, inventory control, and revenue tracking. Built with modern .NET technologies and following industry best practices.

### Key Highlights

- **Complete Solution** - 11 fully integrated modules covering all restaurant operations
- **Clean Architecture** - 3-layer design with MVVM pattern for maintainability
- **Modern UI** - Intuitive Windows Presentation Foundation interface
- **Real-time Operations** - Live order tracking and table status updates
- **Robust Data Management** - SQL Server with Entity Framework Core

---

## Features

### Core Modules

#### 1. Authentication & Authorization

- Secure login with credential validation
- Session management
- User role-based access (Admin)

#### 2. Dashboard

- Real-time statistics overview
- Table availability monitoring
- Today's revenue tracking
- Active orders count
- Quick navigation to main functions

#### 3. Area & Table Management

- CRUD operations for dining areas
- Dynamic table creation and management
- Multi-select table operations
- Visual status indicators (Empty/Using/Booked/Maintenance)
- Bulk table operations

#### 4. Menu Management

- **Category Management**: Organize dishes by categories
- **Dish Management**: Complete inventory control
  - Add/Edit/Delete dishes
  - Image URL support
  - Price and unit management
  - Real-time search and filtering

#### 5. Customer Management

- Customer database with contact information
- Order history tracking
- Walk-in customer support
- Phone number validation
- Search by name or phone

#### 6. Point of Sale (POS) System

**Core Feature** - Comprehensive order processing

- Dual-panel interface
  - Left: Order items with quantity controls
  - Right: Area/Table selection and menu browsing
- Real-time total calculation
- Customer assignment (registered or walk-in)
- Order status management (Scheduled/Completed/Cancelled)

#### 7. Payment Processing

- Payment dialog with order summary
- Automatic status updates (Scheduled ? Completed)
- Payment time tracking
- Table status automation (Using ? Empty)
- Transaction history

#### 8. Order Reporting

- Date range filtering
- Revenue statistics
- Order history with full details
- Status-based filtering
- Export capabilities

---

## Architecture

### 3-Layer Architecture

```
┌─────────────────────────────────────────────┐
│        Presentation Layer (WPF)             │
│  - Views (XAML)                             │
│  - ViewModels (MVVM Pattern)                │
│  - Commands & Data Binding                  │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│     Business Logic Layer (Services)         │
│  - Business Rules & Validation              │
│  - Service Interfaces & Implementations     │
│  - Domain Logic                             │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│      Data Access Layer (Repositories)       │
│  - Entity Framework Core                    │
│  - Repository Pattern (Singleton)           │
│  - Database Context & Migrations            │
└─────────────────────────────────────────────┘
```

### Design Patterns

- **MVVM** - Separation of UI and business logic
- **Repository Pattern** - Data access abstraction
- **Singleton** - Repository instance management
- **Dependency Injection** - Service lifetime management
- **Command Pattern** - User action handling

---

## Technologies

### Frontend

- **.NET 8 WPF** - Windows desktop UI framework
- **XAML** - Declarative UI markup
- **Material Design Principles** - Modern UI components

### Backend

- **C# 12** - Primary programming language
- **.NET 8** - Application framework
- **Entity Framework Core 9.0** - Object-relational mapping
- **SQL Server** - Relational database

### Development Tools

- **Visual Studio 2022** - Integrated development environment
- **SQL Server Management Studio** - Database administration
- **Git** - Version control system

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or later
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (Express or Developer Edition)
- Windows 10/11 operating system

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/DangKhoa050318/RestaurantManagement.git
cd RestaurantManagement
```

#### 2. Database Setup

Run the SQL script to create the database with sample data:

```sql
-- Execute RestaurantMiniManagementDB.sql in SQL Server Management Studio
-- This creates the database schema and populates sample data
```

The script will create:

- Database: `RestaurantMiniManagementDB`
- 7 tables: customers, areas, tables, categories, dishes, orders, orderdetails
- Sample data for testing

#### 3. Configure Connection String

Edit `RestaurantManagementWPF/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RestaurantMiniManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Authentication": {
    "Username": "admin",
    "Password": "admin123"
  }
}
```

#### 4. Build and Run

**Using Visual Studio:**

1. Open `RestaurantManagement.sln`
2. Set `RestaurantManagementWPF` as the startup project
3. Press `F5` to build and run

**Using Command Line:**

```bash
cd RestaurantManagementWPF
dotnet restore
dotnet build
dotnet run
```

#### 5. Login

Default credentials:

- **Username:** `admin`
- **Password:** `admin123`

---

## Project Structure

```
RestaurantManagement/
│
├── BusinessObjects/                    # Domain Models
│   ├── Models/
│   │   ├── Area.cs
│   │   ├── Table.cs
│   │   ├── Category.cs
│   │   ├── Dish.cs
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   └── OrderDetail.cs
│   └── Constants/
│       └── DbConstants.cs              # Status constants
│
├── DataAccessLayer/                    # EF Core Context
│   ├── RestaurantContext.cs           # DbContext
│   └── Migrations/                    # Database migrations
│
├── Repositories/                       # Data Access
│   ├── Interfaces/
│   │   ├── IAreaRepository.cs
│   │   ├── ITableRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── IDishRepository.cs
│   │   ├── ICustomerRepository.cs
│   │   ├── IOrderRepository.cs
│   │   └── IOrderDetailRepository.cs
│   └── Implementations/
│       └── [Repository implementations]
│
├── BusinessLogicLayer/                 # Business Services
│   ├── Interfaces/
│   │   └── [Service interfaces]
│   └── Implementations/
│       ├── AreaService.cs
│       ├── TableService.cs
│       ├── CategoryService.cs
│       ├── DishService.cs
│       ├── CustomerService.cs
│       ├── OrderService.cs
│       └── OrderDetailService.cs
│
├── RestaurantManagementWPF/            # WPF Application
│   ├── Views/                         # UI Views
│   │   ├── LoginWindow.xaml
│   │   ├── AdminShellWindow.xaml
│   │   ├── Pages/
│   │   │   ├── DashboardPage.xaml
│   │   │   ├── POSPage.xaml
│   │   │   ├── AreaManagementPage.xaml
│   │   │   ├── DishManagementPage.xaml
│   │   │   ├── CategoryManagementPage.xaml
│   │   │   ├── CustomerManagementPage.xaml
│   │   │   └── OrderReportPage.xaml
│   │   └── Dialogs/                   # Dialog Windows
│   │       └── [Add/Edit dialogs for each entity]
│   │
│   ├── ViewModels/                    # MVVM ViewModels
│   │   ├── LoginViewModel.cs
│   │   ├── AdminShellViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── POSViewModel.cs
│   │   ├── [Management ViewModels]
│   │   ├── Dialogs/                   # Dialog ViewModels
│   │   └── Models/                    # UI Models
│   │
│   ├── Services/                      # Application Services
│   │   ├── AuthenticationService.cs
│   │   ├── DialogService.cs
│   │   └── ConfigurationService.cs
│   │
│   ├── Helpers/                       # Utility Classes
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   │
│   ├── Converters/                    # Value Converters
│   │   ├── BooleanToVisibilityConverter.cs
│   │   ├── StatusToColorConverter.cs
│   │   └── CurrencyConverter.cs
│   │
│   ├── Resources/                     # UI Resources
│   │   ├── ResourceDictionary.xaml
│   │   └── Styles/
│   │       ├── ButtonStyles.xaml
│   │       ├── TextBoxStyles.xaml
│   │       └── DataGridStyles.xaml
│   │
│   ├── Docs/                          # Documentation
│   │   └── FEATURE_ANALYSIS.md        # Detailed feature documentation
│   │
│   └── appsettings.json               # Application configuration
│
├── BusinessLogicLayer.Tests/          # Unit Tests
│
└── RestaurantMiniManagementDB.sql     # Database setup script
```

---

## Usage

### Basic Workflow

1. **Login** with admin credentials
2. **Set Up Areas** - Create dining areas (e.g., "VIP Floor", "Garden")
3. **Add Tables** - Assign tables to each area
4. **Create Categories** - Organize menu (Appetizers, Main Course, Drinks, Desserts)
5. **Add Dishes** - Build your menu with prices and descriptions
6. **Register Customers** (optional) - Add regular customers to the database
7. **Create Orders** via POS:
   - Select area and table
   - Browse menu by category
   - Add items to order
   - Assign customer (or mark as walk-in)
   - Save order (table status ? "Using")
8. **Process Payment**:
   - Review order summary
   - Confirm payment
   - Order status ? "Completed"
   - Table status ? "Empty"
9. **View Reports** - Filter orders by date range, view revenue statistics

### Sample Data

The database script includes sample data:

- 4 Areas (3 active, 1 in maintenance)
- 6 Tables across different areas
- 4 Categories (Appetizers, Main Course, Dessert, Drinks)
- 9 Dishes with Vietnamese cuisine
- 5 Customers
- 5 Sample orders (3 completed, 1 scheduled, 1 cancelled)

---

## Documentation

Comprehensive feature documentation is available:

- **[FEATURE_ANALYSIS.md](RestaurantManagementWPF/Docs/FEATURE_ANALYSIS.md)** - Complete feature breakdown, implementation status, and technical details

### Key Documentation Sections

- Overview of all 11 modules
- Backend implementation details (Models, Repositories, Services)
- Frontend implementation (MVVM, Views, ViewModels)
- Feature status tracking (100% complete)
- Architecture diagrams
- Technology stack details

---

## Development

### Building from Source

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd RestaurantManagementWPF
dotnet run
```

### Adding New Features

1. **Model** - Add entity in `BusinessObjects/Models`
2. **Repository** - Create interface and implementation in `Repositories`
3. **Service** - Add business logic in `BusinessLogicLayer`
4. **ViewModel** - Create MVVM ViewModel in `RestaurantManagementWPF/ViewModels`
5. **View** - Design UI in `RestaurantManagementWPF/Views`

### Code Standards

- Follow C# naming conventions
- Implement MVVM pattern for UI components
- Add XML documentation for public APIs
- Write unit tests for business logic
- Use async/await for database operations

---

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Pull Request Process

- Ensure code builds without errors
- Update documentation for new features
- Add unit tests where applicable
- Follow existing code style and patterns

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Authors

- **Dang Khoa** - [DangKhoa050318](https://github.com/DangKhoa050318)

---

## Acknowledgments

- Microsoft for .NET and Entity Framework Core
- WPF Community for design inspiration
- Contributors and testers

---

## Contact & Support

- **GitHub Repository**: [RestaurantManagement](https://github.com/DangKhoa050318/RestaurantManagement)
- **Issues**: [Report a bug](https://github.com/DangKhoa050318/RestaurantManagement/issues)

---

## Project Status

```
Status: Production Ready
Version: 1.0.0
Completion: 100% (11/11 modules)
Last Updated: January 2025
```

### Modules Status

- Authentication & Authorization: COMPLETE
- Dashboard: COMPLETE
- Area Management: COMPLETE
- Table Management: COMPLETE
- Category Management: COMPLETE
- Dish Management: COMPLETE
- Customer Management: COMPLETE
- POS System: COMPLETE
- Payment Processing: COMPLETE
- Order Reporting: COMPLETE

---

_Built with passion for efficient restaurant operations_
