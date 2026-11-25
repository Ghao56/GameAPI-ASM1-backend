# GameApi – ASM 1 (GAM106)

Game API theo hướng dẫn ASM 1: đăng ký, đăng nhập bằng JWT, tạo và sửa nhân vật.
Kiến trúc chia lớp ngang: `Models`, `Data`, `DTOs`, `Services`, `Controllers`.

## 1. Chức năng

| # | Chức năng | Phương thức + đường dẫn | Token | Kết quả đúng |
|---|-----------|-------------------------|-------|--------------|
| 1 | Đăng ký | `POST /api/auth/register` | Không | 201, trả id + username |
| 2 | Đăng nhập | `POST /api/auth/login` | Không | 200, trả token |
| 3 | Tạo nhân vật | `POST /api/characters` | Có | 201, level 1, hp 100 |
| 4 | Sửa nhân vật | `PUT /api/characters/{id}` | Có | 200, nhân vật sau khi sửa |

## 2. Công nghệ

| Thành phần | Phiên bản |
|-----------|-----------|
| .NET / ASP.NET Core | 10.0 (net10.0) |
| Entity Framework Core | 10.0.14 |
| Microting.EntityFrameworkCore.MySql | 10.0.14 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.14 |
| Swashbuckle.AspNetCore | 11.0.1 |
| MySQL Server | 8.x / 9.x, cổng 3306 |

> Hướng dẫn dùng .NET 8/9 + Pomelo. Máy này chỉ có SDK 10, và Pomelo chưa có bản EF Core 10,
> nên project dùng **Microting.EntityFrameworkCore.MySql 10.0.14** (bản fork của Pomelo, API
> `UseMySql` / `MySqlServerVersion` giống hệt). Code nghiệp vụ không khác gì bản .NET 9.

## 3. Cấu trúc

```
Models/       Account, Character
Data/         AppDbContext, AppDbContextFactory (cho dotnet ef)
DTOs/         Register/Login/CreateCharacter/UpdateCharacter request + response
Services/     AuthService (băm mật khẩu, tạo JWT), CharacterService (kiểm chủ sở hữu)
Controllers/  AuthController, CharactersController (chọn mã HTTP)
Migrations/   InitialCreate
```

Controller không truy cập `DbContext`; service không trả mã HTTP.

## 4. Chạy project

1. Tạo database (MySQL Workbench, tài khoản root):

```sql
CREATE DATABASE IF NOT EXISTS game_asm1_demo
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
```

2. Kiểm tra chuỗi kết nối và khóa JWT trong `appsettings.json` (khóa phải ≥ 32 ký tự):

```json
"ConnectionStrings": { "Default": "Server=localhost;Port=3306;Database=game_asm1_demo;User ID=root;Password=123456;" },
"Jwt": { "Key": "GameApi-ASM1-Secret-Key-At-Least-32-Characters-2026" }
```

3. Tạo bảng và chạy:

```powershell
dotnet restore
dotnet ef database update   # hoặc để app tự Database.Migrate() khi khởi động
dotnet run
```

Mở `http://localhost:5292/swagger` (trang `/` tự chuyển sang Swagger).

Nếu model thay đổi: `dotnet ef migrations add <TenMigration>` rồi `dotnet ef database update`.

## 5. 12 ca kiểm thử trên Swagger

| Mã | API | Dữ liệu vào | Mong đợi |
|----|-----|-------------|----------|
| TC01 | register | sinhvien01 / 123456 | 201, id + username |
| TC02 | register | sinhvien01 lần nữa | 409 "Tên đăng nhập đã tồn tại" |
| TC03 | register | mật khẩu "123" | 400, lỗi trường Password |
| TC04 | login | sinhvien01 / sai mật khẩu | 401 "Sai tên đăng nhập hoặc mật khẩu" |
| TC05 | login | sinhvien01 / 123456 | 200, có token |
| TC06 | POST characters | chưa Authorize | 401 |
| TC07 | Authorize | dán token TC05 (không thêm chữ Bearer) | Authorized |
| TC08 | POST characters | Hiep Si Rong / Warrior | 201, level 1, hp 100 |
| TC09 | POST characters | name rỗng | 400, lỗi trường Name |
| TC10 | PUT characters/{id} | id TC08, level 5, hp 250 | 200, level 5, hp 250 |
| TC11 | PUT characters/{id} | level 0 | 400, lỗi trường Level |
| TC12 | PUT characters/{id} | id nhân vật của sinhvien02 | 404 "Không tìm thấy nhân vật của bạn" |

TC12 cần tài khoản thứ hai: đăng ký `sinhvien02`, đăng nhập, Authorize bằng token đó, tạo một
nhân vật và ghi lại id; sau đó Authorize lại bằng token `sinhvien01` rồi sửa id đó.

Đã kiểm chứng tự động toàn bộ 12 ca trên máy này: mã HTTP và body khớp bảng trên; `PasswordHash`
trong MySQL bắt đầu bằng `AQAAAA...`; PUT thiếu `hp` cũng trả 400 (nhờ `int?` + `[Required]`).

## 6. Lỗi thường gặp

| Triệu chứng | Cách sửa |
|-------------|----------|
| `Access denied for user 'root'@'localhost'` | Sửa user/mật khẩu trong `appsettings.json` |
| `Unable to connect to any of the specified MySQL hosts` | Bật dịch vụ MySQL (`Get-Service *mysql*`), kiểm tra cổng 3306 |
| `Unknown database 'game_asm1_demo'` | Chạy `CREATE DATABASE` ở mục 4 |
| `Table '...accounts' doesn't exist` | `dotnet ef database update`, hoặc build lại rồi chạy |
| Đã Authorize vẫn 401 | Dán đúng token (không thêm "Bearer"), `UseAuthentication()` phải đứng trước `UseAuthorization()` |
| `IDX10720` khi đăng nhập | `Jwt:Key` ngắn hơn 32 ký tự |
| PUT thiếu `hp` vẫn 200 | Phải khai báo `int?` + `[Required]` (đã làm trong `UpdateCharacterRequest`) |

Trước khi nộp: xóa `bin` và `obj`.
