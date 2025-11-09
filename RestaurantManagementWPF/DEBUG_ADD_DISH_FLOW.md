# ?? DEBUG GUIDE - KI?M TRA LU?NG ADD DISH

## ?? CÁCH TEST

### B??c 1: M? Output Window
1. Trong Visual Studio, nh?n **Ctrl + Alt + O**
2. Ho?c **View ? Output**
3. Ch?n "Show output from: **Debug**"

### B??c 2: Ch?y Application
1. Nh?n **F5** ?? run
2. Login vào h? th?ng
3. Click menu **"Dish Management"**

### B??c 3: Test Add Dish
1. Click button **"+ Add Dish"**
2. Nh?p data:
   - **Dish Name:** `Test Món ?n`
   - **Category:** Ch?n b?t k?
   - **Price:** `50000`
   - **Unit:** `portion`
3. Click **CREATE**

### B??c 4: Xem Debug Log

## ? LU?NG ?ÚNG - LOG M?U

```
=== ExecuteAddDish called ===
Dialog created
Starting to load categories...
Loaded 5 categories from service
Added category: Khai v?
...
Selected default category: Khai v?
ViewModel created. Categories count: 5
DataContext set
Calling dialog.ShowDialog()...
Dialog closed. Result: True, ViewModel.DialogResult: True
? User clicked CREATE, processing dish...
Data: Name='Test Món ?n', Price='50000', Category=Khai v?, Unit='portion'
? Price parsed successfully: 50000
? Dish object created: ID=0, Name='Test Món ?n', CategoryId=1, Price=50000
?? Calling _dishService.AddDish()...
?? DishService.AddDish called with: Name='Test Món ?n', Price=50000, CategoryId=1, Unit='portion'
? Validation passed, calling repository...
?? DishRepository.AddDish called
   Dish data: Name='Test Món ?n', CategoryId=1, Price=50000, Unit='portion'
   DbContext created
   Dish added to context
   ? SaveChanges completed. Rows affected: 1
   New DishId: 15
? Repository.AddDish completed
? Dish saved to database successfully!
?? Reloading dishes...
=== ExecuteAddDish completed ===
```

## ? CÁC L?I CÓ TH? X?Y RA

### L?i 1: Dialog không m?
```
=== ExecuteAddDish called ===
Dialog created
ERROR loading categories: ...
```
**Nguyên nhân:** L?i k?t n?i database khi load categories
**Gi?i pháp:** Ki?m tra connection string trong appsettings.json

### L?i 2: DialogResult = False
```
Dialog closed. Result: False, ViewModel.DialogResult: True
? User cancelled the dialog or DialogResult = false
```
**Nguyên nhân:** Code-behind không set DialogResult ?úng
**Gi?i pháp:** ?ã fix trong AddDishDialog.xaml.cs

### L?i 3: Validation failed
```
? User clicked CREATE, processing dish...
? ERROR: Price parsing failed
```
**Nguyên nhân:** Price format không h?p l?
**Gi?i pháp:** Nh?p s? nguyên, không có ký t? ??c bi?t

### L?i 4: Database error
```
?? DishRepository.AddDish called
   ? ERROR in AddDish: Cannot insert explicit value for identity column...
```
**Nguyên nhân:** DishId ???c set manual thay vì auto-increment
**Gi?i pháp:** Không set DishId khi t?o Dish m?i

### L?i 5: CategoryId null
```
? Dish object created: ID=0, Name='Test', CategoryId=, Price=50000
?? DishService.AddDish called with: Name='Test', CategoryId=, ...
```
**Nguyên nhân:** Không ch?n category
**Gi?i pháp:** ??m b?o SelectedCategory không null

## ?? CHECKLIST DEBUG

### A. Ki?m tra Dialog M?
- [ ] Log "Dialog created" xu?t hi?n
- [ ] Log "ViewModel created. Categories count: X" xu?t hi?n (X > 0)
- [ ] Log "DataContext set" xu?t hi?n

### B. Ki?m tra User Input
- [ ] Log "Dialog closed. Result: True" (PH?I là True)
- [ ] Log "ViewModel.DialogResult: True"
- [ ] Log "User clicked CREATE, processing dish..."

### C. Ki?m tra Data
- [ ] DishName không empty: `Name='...'`
- [ ] Price > 0: `Price=50000`
- [ ] CategoryId có giá tr?: `CategoryId=1` (không null)
- [ ] Unit có giá tr?: `Unit='portion'`

### D. Ki?m tra Validation
- [ ] Log "Price parsed successfully: X"
- [ ] Log "Dish object created: ..."
- [ ] Log "Validation passed, calling repository..."

### E. Ki?m tra Database
- [ ] Log "DbContext created"
- [ ] Log "Dish added to context"
- [ ] Log "SaveChanges completed. Rows affected: 1"
- [ ] Log "New DishId: X" (X > 0)

### F. Ki?m tra Success
- [ ] Log "Dish saved to database successfully!"
- [ ] Success dialog hi?n ra
- [ ] DataGrid reload (món m?i xu?t hi?n)

## ?? ?I?M D?NG BREAKPOINT

N?u log không rõ, ??t breakpoint t?i:

1. **DishManagementViewModel.cs**
   - Line: `var dialog = new AddDishDialog();`
   - Line: `var dialogResult = dialog.ShowDialog();`
   - Line: `if (dialogResult == true && viewModel.DialogResult)`
   - Line: `_dishService.AddDish(newDish);`

2. **AddDishDialog.xaml.cs**
   - Line: `this.DialogResult = viewModel.DialogResult;`

3. **DishService.cs**
   - Line: `_dishRepository.AddDish(dish);`

4. **DishRepository.cs**
   - Line: `context.Dishes.Add(dish);`
   - Line: `var saveResult = context.SaveChanges();`

## ?? PHÂN TÍCH LOG

### Tr??ng h?p 1: Không th?y log "=== ExecuteAddDish called ==="
? Command binding sai ho?c button không click ???c
? Ki?m tra: `Command="{Binding AddDishCommand}"` trong XAML

### Tr??ng h?p 2: Log d?ng t?i "Calling dialog.ShowDialog()..."
? Dialog b? crash khi m?
? Check exception trong Output window

### Tr??ng h?p 3: Log d?ng t?i "User cancelled the dialog"
? DialogResult = false
? Ki?m tra AddDishDialog.xaml.cs ? OKButton_Click

### Tr??ng h?p 4: Log d?ng t?i "Price parsing failed"
? Nh?p price không ?úng format
? Nh?p s? nguyên (VD: 50000), không dùng d?u ph?y

### Tr??ng h?p 5: Log có "SaveChanges completed" nh?ng món không xu?t hi?n
? Reload dishes b? l?i
? Check log "Reloading dishes..."

## ? EXPECTED RESULT

Sau khi test thành công:
1. ? All logs t? start ? end xu?t hi?n
2. ? "Rows affected: 1" ? Database insert thành công
3. ? "New DishId: X" ? Auto-increment ho?t ??ng
4. ? Success message hi?n ra
5. ? DataGrid reload ? món m?i xu?t hi?n
6. ? Check database: `SELECT * FROM dishes` ? có row m?i

---

**Ch?y test và g?i cho tôi TOÀN B? output log t?:**
```
=== ExecuteAddDish called ===
```
**??n:**
```
=== ExecuteAddDish completed ===
```

Tôi s? phân tích chính xác l?i ? ?âu!
