# 📋 PHÂN TÍCH TÍNH NĂNG - RESTAURANT MANAGEMENT WPF

## ✅ ĐÁNH GIÁ BACKEND (3-Layer Architecture)

### 🟢 ĐÃ HOÀN THÀNH - Backend Layer

#### 1. **BusinessObjects (Models)** ✅
- ✅ Customer (CustomerId, Fullname, Phone)
- ✅ Area (AreaId, AreaName, AreaStatus)
- ✅ Table (TableId, TableName, Status, AreaId)
- ✅ Category (CategoryId, CategoryName, CategoryDescription)
- ✅ Dish (DishId, Name, Price, Description, UnitOfCalculation, ImgUrl, CategoryId)
- ✅ Order (OrderId, CustomerId, TableId, OrderTime, TotalAmount, Status)
- ✅ OrderDetail (OrderDetailId, OrderId, DishId, Quantity, UnitPrice, Description)

#### 2. **Repositories** ✅
- ✅ IAreaRepository + AreaRepository (CRUD Areas)
- ✅ ITableRepository + TableRepository (CRUD Tables)
- ✅ IDishRepository + DishRepository (CRUD Dishes)
- ✅ ICategoryRepository + CategoryRepository (CRUD Categories)
- ✅ ICustomerRepository + CustomerRepository (CRUD Customers)
- ✅ IOrderRepository + OrderRepository (CRUD Orders)
- ✅ IOrderDetailRepository + OrderDetailRepository (CRUD OrderDetails)

#### 3. **Services (Business Logic)** ✅
- ✅ IAreaService + AreaService
- ✅ ITableService + TableService
- ✅ IDishService + DishService
- ✅ ICategoryService + CategoryService (cần verify)
- ✅ ICustomerService + CustomerService
- ✅ IOrderService + OrderService
- ✅ IOrderDetailService + OrderDetailService

#### 4. **Database Context** ✅
- ✅ RestaurantMiniManagementDbContext
- ✅ SQL Server connection
- ✅ Entity Framework Core

---

## 📱 ĐÁNH GIÁ FRONTEND (WPF)

### 🟡 CẦN IMPLEMENT - Frontend Features

## I. AUTHENTICATION & AUTHORIZATION

### ❌ 1. Màn hình đăng nhập
**Yêu cầu:**
- Username và Password (lưu trong appsettings.json)
- Bỏ phần mã cửa hàng
- Vai trò: Admin

**Trạng thái:** ❌ CHƯA CÓ
**Cần tạo:**
- [ ] Views/LoginWindow.xaml
- [ ] ViewModels/LoginViewModel.cs
- [ ] Services/AuthenticationService.cs
- [ ] Cập nhật appsettings.json với Admin credentials

---

## II. MÀN HÌNH CHÍNH (Main POS Screen)

### ❌ 2. Màn hình Order chính - Layout 2 bảng

**Yêu cầu:**
- **Bảng bên TRÁI:** Danh sách món đã chọn
  - STT
  - Tên món
  - Số lượng
  - Giá bán
  - Tổng tiền
  - Bỏ phần user

- **Bảng bên PHẢI:** Chọn khu vực, bàn, món ăn
  - Dropdown chọn khu vực
  - Hiển thị sơ đồ bàn theo khu vực
  - Menu món ăn (theo category)

**Trạng thái:** ❌ CHƯA CÓ
**Cần tạo:**
- [ ] Views/Pages/POSPage.xaml (Point of Sale Page)
- [ ] ViewModels/POSViewModel.cs
- [ ] UserControls/OrderItemsListControl.xaml (Bảng trái)
- [ ] UserControls/TableSelectionControl.xaml (Sơ đồ bàn)
- [ ] UserControls/MenuSelectionControl.xaml (Menu món ăn)

---

## III. QUẢN LÝ KHU VỰC & BÀN

### ❌ 3. Quản lý khu vực (Area Management)

**Yêu cầu:**
- ✅ CRUD khu vực
- ✅ Xem trạng thái khu vực
- ✅ Dấu cộng màu xanh để thêm khu vực
- ✅ Dropdown chọn khu vực
- ✅ Tự động tạo bàn cho khu vực

**Chức năng:**
- [x] Backend: AreaService hoàn chỉnh
- [ ] Frontend:
  - [ ] Views/Pages/AreaManagementPage.xaml
  - [ ] ViewModels/AreaManagementViewModel.cs
  - [ ] Dialog: AddAreaDialog.xaml
  - [ ] Dialog: EditAreaDialog.xaml
  - [ ] Dialog: DeleteAreaConfirmDialog.xaml
  - [ ] Auto create tables feature

---

### ❌ 4. Quản lý bàn (Table Management)

**Yêu cầu:**
- ✅ Tạo bàn (manual + auto)
- ✅ Xóa bàn
- ✅ Cập nhật trạng thái bàn
- ✅ Hiển thị sơ đồ bàn theo khu vực

**Chức năng:**
- [x] Backend: TableService hoàn chỉnh
- [ ] Frontend:
  - [ ] UserControls/TableCardControl.xaml (Card hiển thị 1 bàn)
  - [ ] ViewModels/TableCardViewModel.cs
  - [ ] Dialog: AddTableDialog.xaml
  - [ ] Dialog: EditTableDialog.xaml

---

## IV. QUẢN LÝ MÓN ĂN

### ❌ 5. Quản lý món ăn (Dish Management)

**Yêu cầu:**
- ✅ CRUD món ăn
- ✅ Xem thông tin món ăn
- ✅ Dialog để thêm/sửa món ăn
- ✅ Upload/hiển thị hình ảnh món
- ✅ Search món ăn

**Chức năng:**
- [x] Backend: DishService hoàn chỉnh (có SearchDishesByName)
- [ ] Frontend:
  - [ ] Views/Pages/DishManagementPage.xaml
  - [ ] ViewModels/DishManagementViewModel.cs
  - [ ] Dialog: AddDishDialog.xaml
  - [ ] Dialog: EditDishDialog.xaml
  - [ ] Dialog: DeleteDishConfirmDialog.xaml
  - [ ] Image upload/display control

**Fields:**
- DishId, Name, Price, Description
- UnitOfCalculation (đơn vị tính)
- ImgUrl (hình ảnh)
- CategoryId (danh mục)

---

### ❌ 6. Quản lý danh mục món ăn (Category Management)

**Yêu cầu:**
- ✅ CRUD danh mục
- ✅ Hiển thị danh sách danh mục
- ✅ Bỏ phần "máy in chế biến"

**Chức năng:**
- [x] Backend: CategoryService (cần verify)
- [ ] Frontend:
  - [ ] Views/Pages/CategoryManagementPage.xaml
  - [ ] ViewModels/CategoryManagementViewModel.cs
  - [ ] Dialog: AddCategoryDialog.xaml
  - [ ] Dialog: EditCategoryDialog.xaml
  - [ ] Dialog: DeleteCategoryConfirmDialog.xaml

---

## V. QUẢN LÝ ĐỐN HÀNG

### ❌ 7. Tạo và quản lý Order

**Yêu cầu:**
- ✅ Chọn bàn
- ✅ Chọn món ăn (add OrderDetail)
- ✅ Tính tổng tiền tự động
- ✅ Cập nhật trạng thái đơn
- ✅ Xóa/chỉnh sửa món trong order

**Chức năng:**
- [x] Backend: OrderService + OrderDetailService hoàn chỉnh
- [ ] Frontend:
  - [ ] Đã có trong POSPage (Item 2)
  - [ ] ViewModels/OrderViewModel.cs
  - [ ] Logic tính tổng tiền realtime

---

### ❌ 8. Thanh toán (Payment)

**Yêu cầu:**
- ✅ Hiển thị tổng tiền
- ✅ Bỏ chọn máy in
- ✅ Bỏ mẫu in hóa đơn (hoặc đơn giản hóa)
- ✅ Cập nhật trạng thái Order = "Completed"
- ✅ Cập nhật trạng thái Table = "Available"

**Chức năng:**
- [x] Backend: Có sẵn UpdateOrderStatus
- [ ] Frontend:
  - [ ] Dialog: PaymentDialog.xaml
  - [ ] ViewModels/PaymentViewModel.cs
  - [ ] Print preview (optional)

---

## VI. BÁO CÁO

### ❌ 9. Báo cáo đơn hàng (Order Report)

**Yêu cầu:**
- ✅ Search theo Start Date và End Date
- ✅ Hiển thị danh sách orders
- ✅ Bỏ phần phí dịch vụ
- ✅ Bỏ phần tiền thuế

**Chức năng:**
- [x] Backend: GetOrders() có sẵn (cần thêm filter by date)
- [ ] Frontend:
  - [ ] Views/Pages/OrderReportPage.xaml
  - [ ] ViewModels/OrderReportViewModel.cs
  - [ ] DatePicker for Start/End date
  - [ ] Export to Excel/PDF (optional)

**Cần bổ sung Backend:**
- [ ] IOrderService.GetOrdersByDateRange(DateTime start, DateTime end)
- [ ] OrderRepository.GetOrdersByDateRange()

---

## VII. QUẢN LÝ KHÁCH HÀNG (OPTIONAL)

### ❌ 10. Quản lý khách hàng (Customer Management)

**Yêu cầu:**
- ✅ CRUD khách hàng
- ✅ Search theo tên/SĐT
- ✅ Lịch sử đơn hàng

**Chức năng:**
- [x] Backend: CustomerService hoàn chỉnh
- [ ] Frontend:
  - [ ] Views/Pages/CustomerManagementPage.xaml
  - [ ] ViewModels/CustomerManagementViewModel.cs
  - [ ] Dialog: AddCustomerDialog.xaml
  - [ ] Dialog: EditCustomerDialog.xaml

---

## 📊 TỔNG KẾT TÍNH NĂNG

### Backend Status: 🟢 95% Complete
- ✅ All Models
- ✅ All Repositories (Singleton pattern)
- ✅ All Services with business logic
- ✅ EF Core + SQL Server
- ⚠️ Thiếu: OrderRepository.GetOrdersByDateRange()

### Frontend Status: 🔴 5% Complete
- ✅ Project structure (MVVM folders)
- ✅ Base classes (BaseViewModel, RelayCommand)
- ✅ Services (Configuration, Dialog, Navigation)
- ✅ Styles & Resources
- ❌ Tất cả Views/Pages chưa có
- ❌ Tất cả ViewModels nghiệp vụ chưa có
- ❌ Login chưa có

---

## 🎯 KẾ HOẠCH THỰC HIỆN (ROADMAP)

### PHASE 1: AUTHENTICATION (Priority: HIGHEST) 🔥
**Estimate: 1-2 hours**
1. [ ] LoginWindow.xaml + ViewModel
2. [ ] AuthenticationService
3. [ ] Update appsettings.json
4. [ ] Set StartupUri = LoginWindow

### PHASE 2: MAIN SHELL & NAVIGATION 🚀
**Estimate: 2-3 hours**
1. [ ] Update AdminShellWindow (Menu + Navigation)
2. [ ] AdminShellViewModel
3. [ ] NavigationService integration
4. [ ] Dashboard/Home page (optional)

### PHASE 3: AREA & TABLE MANAGEMENT 🏢
**Estimate: 4-6 hours**
1. [ ] AreaManagementPage + ViewModel
2. [ ] TableCardControl + ViewModel
3. [ ] Add/Edit/Delete dialogs
4. [ ] Auto create tables feature
5. [ ] Table status visualization

### PHASE 4: DISH & CATEGORY MANAGEMENT 🍜
**Estimate: 4-6 hours**
1. [ ] CategoryManagementPage + ViewModel
2. [ ] DishManagementPage + ViewModel
3. [ ] Image upload control
4. [ ] Add/Edit/Delete dialogs
5. [ ] Search functionality

### PHASE 5: POS SCREEN (MAIN FEATURE) 💰
**Estimate: 8-12 hours**
1. [ ] POSPage layout (2 panels)
2. [ ] Left panel: Order items list
3. [ ] Right panel: Area/Table selection
4. [ ] Right panel: Menu selection
5. [ ] Add/Remove items logic
6. [ ] Auto calculate total
7. [ ] PaymentDialog

### PHASE 6: ORDER MANAGEMENT & REPORTS 📊
**Estimate: 3-4 hours**
1. [ ] OrderReportPage + ViewModel
2. [ ] Date range filter
3. [ ] Export functionality (optional)
4. [ ] Order history view

### PHASE 7: CUSTOMER MANAGEMENT (OPTIONAL) 👥
**Estimate: 2-3 hours**
1. [ ] CustomerManagementPage + ViewModel
2. [ ] Add/Edit/Delete dialogs
3. [ ] Search by name/phone
4. [ ] Order history per customer

---

## 📝 DANH SÁCH FILES CẦN TẠO

### Views (Windows + Pages)
```
Views/
├── LoginWindow.xaml ❌
├── AdminShellWindow.xaml ⚠️ (Đã có nhưng cần update)
├── Pages/
│   ├── DashboardPage.xaml ❌
│   ├── POSPage.xaml ❌
│   ├── AreaManagementPage.xaml ❌
│   ├── DishManagementPage.xaml ❌
│   ├── CategoryManagementPage.xaml ❌
│   ├── OrderReportPage.xaml ❌
│   └── CustomerManagementPage.xaml ❌
└── Dialogs/
    ├── AddAreaDialog.xaml ❌
    ├── EditAreaDialog.xaml ❌
    ├── AddTableDialog.xaml ❌
    ├── AddDishDialog.xaml ❌
    ├── EditDishDialog.xaml ❌
    ├── AddCategoryDialog.xaml ❌
    ├── EditCategoryDialog.xaml ❌
    ├── PaymentDialog.xaml ❌
    └── ConfirmDeleteDialog.xaml ❌
```

### ViewModels
```
ViewModels/
├── LoginViewModel.cs ❌
├── AdminShellViewModel.cs ⚠️ (Đã có nhưng cần update)
├── DashboardViewModel.cs ❌
├── POSViewModel.cs ❌
├── AreaManagementViewModel.cs ❌
├── DishManagementViewModel.cs ❌
├── CategoryManagementViewModel.cs ❌
├── OrderReportViewModel.cs ❌
├── CustomerManagementViewModel.cs ❌
└── Dialogs/
    ├── AddAreaDialogViewModel.cs ❌
    ├── AddDishDialogViewModel.cs ❌
    ├── PaymentDialogViewModel.cs ❌
    └── ...
```

### UserControls
```
UserControls/
├── TableCardControl.xaml ❌
├── DishCardControl.xaml ❌
├── OrderItemControl.xaml ❌
├── MenuCategoryControl.xaml ❌
└── StatisticCardControl.xaml ❌ (for Dashboard)
```

### Services
```
Services/
├── AuthenticationService.cs ❌
├── OrderProcessingService.cs ❌ (Business logic for POS)
└── ReportService.cs ❌ (for reports)
```

---

## 🎨 UI/UX CONSIDERATIONS

### Đã có sẵn:
- ✅ ButtonStyles (Primary, Secondary, Success, Danger)
- ✅ TextBoxStyles (Modern)
- ✅ DataGridStyles (Modern)
- ✅ Converters (Currency, Status to Color, Boolean to Visibility)

### Cần thêm:
- [ ] ComboBoxStyles (cho dropdown khu vực, category)
- [ ] CardStyles (cho TableCard, DishCard)
- [ ] MenuItemStyles (cho navigation menu)
- [ ] StatusBadgeStyle (cho trạng thái bàn, order)

---

## ⚠️ LƯU Ý QUAN TRỌNG

### 1. Authentication
- Admin credentials trong appsettings.json (plaintext hoặc hash)
- Không cần role-based access control (chỉ có Admin)

### 2. Table Status Flow
```
Available → Reserved → Occupied → Available
           ↓
      Maintenance
```

### 3. Order Status Flow
```
Pending → In Progress → Completed
          ↓
      Cancelled
```

### 4. Tính tổng tiền
- Realtime khi add/remove món
- TotalAmount = SUM(OrderDetail.Quantity * OrderDetail.UnitPrice)

### 5. Xóa dữ liệu
- Area: Không xóa nếu còn Table
- Table: Không xóa nếu có Order
- Dish: Không xóa nếu có OrderDetail
- Category: Cần check trước khi xóa

---

## 🚀 BẮT ĐẦU TỪ ĐÂU?

### Đề xuất: Bắt đầu với **PHASE 1 - LOGIN**
Lý do:
1. Đơn giản nhất
2. Cần thiết cho tất cả features khác
3. Test được AuthenticationService
4. Làm quen với Dialog pattern

### Sau đó: **PHASE 2 - SHELL & NAVIGATION**
Lý do:
1. Tạo khung sườn cho toàn bộ app
2. Test được NavigationService
3. Sau này chỉ cần thêm Pages vào

### Tiếp theo: **PHASE 3 - AREA/TABLE** (Module đơn giản nhất)
Lý do:
1. Logic đơn giản (CRUD cơ bản)
2. Không phụ thuộc module khác
3. Học pattern: Page → ViewModel → Service → Repository
4. Test được Dialog pattern

---

## ✅ KẾT LUẬN

**Backend:** ✅ Hoàn chỉnh 95%
- Chỉ cần bổ sung: GetOrdersByDateRange() cho Report

**Frontend:** ❌ Gần như chưa có
- Cần implement tất cả Views, ViewModels, Dialogs
- Estimate: **30-45 hours** work

**Ưu tiên:**
1. 🔥 Login (HIGHEST)
2. 🚀 Navigation Shell
3. 🏢 Area/Table Management
4. 💰 POS Screen (CORE FEATURE)
5. 📊 Reports
6. 🍜 Dish/Category Management
7. 👥 Customer Management

---

**Bạn muốn bắt đầu từ đâu?**
