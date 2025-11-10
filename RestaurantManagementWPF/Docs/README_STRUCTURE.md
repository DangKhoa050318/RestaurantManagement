# Restaurant Management WPF - C?u trúc th? m?c

## ?? C?u trúc th? m?c MVVM

```
RestaurantManagementWPF/
?
??? ?? Views/                          # XAML Views (Windows, Pages, UserControls)
?   ??? AdminShellWindow.xaml          # Main admin interface
?
??? ?? ViewModels/                     # ViewModels (MVVM Pattern)
?   ??? BaseViewModel.cs               # Base class cho t?t c? ViewModels
?   ??? MainWindowViewModel.cs         # ViewModel cho MainWindow
?   ??? AdminShellViewModel.cs         # ViewModel cho AdminShell
?
??? ?? Models/                         # View-specific models
?   ??? ViewModels/                    # DTOs for UI display
?       ??? AreaViewModel.cs           # Area display model
?       ??? TableViewModel.cs          # Table display model
?       ??? OrderViewModel.cs          # Order display model
?
??? ?? Services/                       # Business services & utilities
?   ??? ConfigurationService.cs        # Read appsettings.json
?   ??? DialogService.cs               # MessageBox & dialogs
?
??? ?? Helpers/                        # Helper classes & utilities
?   ??? RelayCommand.cs                # ICommand implementation
?   ??? NavigationService.cs           # Navigation between views
?
??? ?? Converters/                     # Value converters for XAML binding
?   ??? BooleanToVisibilityConverter.cs    # Bool ? Visibility
?   ??? StatusToColorConverter.cs          # Status ? Color
?   ??? CurrencyConverter.cs               # Decimal ? Currency format
?
??? ?? Resources/                      # Resources (styles, images, etc.)
?   ??? ResourceDictionary.xaml        # Main resource dictionary
?   ??? Styles/                        # XAML styles
?       ??? ButtonStyles.xaml          # Button styles (Primary, Secondary, etc.)
?       ??? TextBoxStyles.xaml         # TextBox styles
?       ??? DataGridStyles.xaml        # DataGrid styles
?
??? App.xaml                           # Application definition & global resources
??? App.xaml.cs                        # Application code-behind
??? MainWindow.xaml                    # Main entry window
??? appsettings.json                   # Configuration file
```

## ?? Naming Conventions

### Views
- **Windows**: `*Window.xaml` (e.g., `MainWindow.xaml`, `AdminShellWindow.xaml`)
- **Pages**: `*Page.xaml` (e.g., `DashboardPage.xaml`, `OrderPage.xaml`)
- **UserControls**: `*Control.xaml` or `*View.xaml` (e.g., `TableCardControl.xaml`)

### ViewModels
- Format: `{ViewName}ViewModel` (e.g., `MainWindowViewModel`, `DashboardPageViewModel`)
- T?t c? inherit t? `BaseViewModel`

### Services
- Format: `{Purpose}Service` (e.g., `DialogService`, `NavigationService`)
- Singleton pattern cho global services

### Models
- **View Models**: Trong folder `Models/ViewModels/`
- Format: `{Entity}ViewModel` (e.g., `AreaViewModel`, `TableViewModel`)

## ?? Dependencies c?n cài

```xml
<!-- Trong RestaurantManagementWPF.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" />
  
  <ProjectReference Include="..\BusinessLogicLayer\Services.csproj" />
  <ProjectReference Include="..\Repositories\Repositories.csproj" />
  <ProjectReference Include="..\BusinessObjects\BusinessObjects.csproj" />
  <ProjectReference Include="..\DataAccessLayer\DataAccessLayer.csproj" />
</ItemGroup>
```

## ?? Next Steps

### 1. T?o các Views chính
- [ ] DashboardPage.xaml (T?ng quan)
- [ ] TableManagementPage.xaml (Qu?n lý bàn)
- [ ] OrderManagementPage.xaml (Qu?n lý ??n hàng)
- [ ] DishManagementPage.xaml (Qu?n lý món ?n)
- [ ] AreaManagementPage.xaml (Qu?n lý khu v?c)
- [ ] CustomerManagementPage.xaml (Qu?n lý khách hàng)

### 2. T?o ViewModels t??ng ?ng
- [ ] DashboardPageViewModel
- [ ] TableManagementViewModel
- [ ] OrderManagementViewModel
- [ ] DishManagementViewModel
- [ ] AreaManagementViewModel
- [ ] CustomerManagementViewModel

### 3. Implement Navigation
- [ ] Setup NavigationService trong AdminShellWindow
- [ ] T?o Menu navigation
- [ ] Binding commands cho navigation

### 4. Connect to Business Layer
- [ ] Inject Services vào ViewModels
- [ ] Implement CRUD operations
- [ ] Handle errors & validation

## ?? MVVM Pattern Flow

```
View (XAML)
    ? (Data Binding)
ViewModel
    ? (Business Logic)
Service Layer (BusinessLogicLayer)
    ? (Data Access)
Repository Layer
    ? (EF Core)
Database
```

## ?? Styles Available

### Buttons
- `PrimaryButton` - Blue button
- `SecondaryButton` - Gray button
- `SuccessButton` - Green button
- `DangerButton` - Red button

### TextBox
- `ModernTextBox` - Rounded modern style

### DataGrid
- `ModernDataGrid` - Clean modern grid
- `ColumnHeaderStyle` - Header styling
- `CellStyle` - Cell styling

## ?? Usage Example

```xaml
<!-- Using styles -->
<Button Style="{StaticResource PrimaryButton}" Content="L?u"/>
<TextBox Style="{StaticResource ModernTextBox}"/>
<DataGrid Style="{StaticResource ModernDataGrid}"/>

<!-- Using converters -->
<TextBlock Text="{Binding Price, Converter={StaticResource CurrencyConverter}}"/>
<Ellipse Fill="{Binding Status, Converter={StaticResource StatusToColorConverter}}"/>
```
