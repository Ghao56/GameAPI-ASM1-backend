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
| 5 | Danh sách nhân vật của tôi | `GET /api/characters` | Có | 200, mảng nhân vật của chính mình, sắp theo id |
| 6 | Xóa nhân vật của tôi | `DELETE /api/characters/{id}` | Có | 204, không body |

## 2. Công nghệ

| Thành phần | Phiên bản |
|-----------|-----------|
| .NET / ASP.NET Core | 10.0 (net10.0) |
| Entity Framework Core | 9.0.0 |
| Pomelo.EntityFrameworkCore.MySql | 9.0.0 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 |
| Swashbuckle.AspNetCore | 6.6.2 |
| MySQL Server | 8.x / 9.x, cổng 3306 |

## 3. Cấu trúc

```
Models/       Account, Character
Data/         AppDbContext, AppDbContextFactory (cho dotnet ef)
DTOs/         Register/Login/CreateCharacter/UpdateCharacter request + response
Services/     AuthService (băm mật khẩu, tạo JWT), CharacterService (kiểm chủ sở hữu)
Controllers/  AuthController, CharactersController (chọn mã HTTP)
Migrations/   InitialCreate, AddCharacters
```

Controller không truy cập `DbContext`; service không trả mã HTTP.

## 4. Chạy project

1. Tạo database (MySQL Workbench, tài khoản root):

```sql
CREATE DATABASE IF NOT EXISTS game_asm1
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
```

2. `appsettings.json` đã có sẵn chuỗi kết nối mẫu (`root` / `123456`, database `game_asm1`) và `Jwt:Key` ≥ 32 ký tự. Nếu MySQL của bạn khác thì sửa lại:

```json
"ConnectionStrings": { "Default": "Server=127.0.0.1;Port=3306;Database=game_asm1;User=root;Password=123456;" },
"Jwt": { "Key": "GAM106-ASM1-Dev-Key-2026-0123456789abcdef-0123456789abcdef" }
```

Muốn không ghi mật khẩu vào file thì chạy `.\run-local.ps1` để truyền mật khẩu MySQL và khóa JWT qua biến môi trường (biến môi trường sẽ ghi đè `appsettings.json`).

3. Tạo bảng và chạy:

```powershell
dotnet tool restore
dotnet restore
dotnet ef database update
dotnet run
```

Mở `http://localhost:5254/swagger`.

Lab 5: API có CORS cho FE Next.js chạy ở `http://localhost:3000` (policy `fe`, chỉ đúng origin này; `http://127.0.0.1:3000` bị chặn).

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

### Lab 5 – 7 ca kiểm thử

| Mã | Thao tác | Mong đợi |
|----|----------|----------|
| TC-A01 | GET /api/characters, không token | 401 |
| TC-A02 | GET bằng token A (A có 2 nhân vật) | 200, đúng 2 phần tử, id tăng dần |
| TC-A03 | GET bằng token B | 200, không có nhân vật của A (`[]` nếu B chưa có) |
| TC-A04 | DELETE nhân vật của A bằng token B | 404 `"Không tìm thấy nhân vật của bạn"`, nhân vật của A vẫn còn |
| TC-A05 | DELETE nhân vật của A bằng token A | 204, không body |
| TC-A06 | DELETE lại cùng id | 404 |
| TC-A07 | GET bằng token A sau khi xóa | không còn id vừa xóa |

Thêm: DELETE không token → 401.

CORS: `OPTIONS /api/characters` với `Origin: http://localhost:3000` phải trả
`Access-Control-Allow-Origin: http://localhost:3000`; với `Origin: http://127.0.0.1:3000` thì không có header đó.

Đã kiểm chứng tự động toàn bộ 12 ca trên máy này: mã HTTP và body khớp bảng trên; `PasswordHash`
trong MySQL bắt đầu bằng `AQAAAA...`; PUT thiếu `hp` cũng trả 400 (nhờ `int?` + `[Required]`).
Lab 5: đã chạy lại 12 ca trên database mới cùng TC-A01…TC-A07, DELETE không token và preflight CORS – 25/25 pass.

## 6. Lỗi thường gặp

| Triệu chứng | Cách sửa |
|-------------|----------|
| `Access denied for user 'root'@'localhost'` | Sửa user/mật khẩu trong `appsettings.json` hoặc dùng `run-local.ps1` |
| `Unable to connect to any of the specified MySQL hosts` | Bật dịch vụ MySQL (`Get-Service *mysql*`), kiểm tra cổng 3306 |
| `Unknown database 'game_asm1'` | Chạy `CREATE DATABASE` ở mục 4 |
| `Table '...accounts' doesn't exist` | `dotnet ef database update`, hoặc build lại rồi chạy |
| Đã Authorize vẫn 401 | Dán đúng token (không thêm "Bearer"), `UseAuthentication()` phải đứng trước `UseAuthorization()` |
| `IDX10720` khi đăng nhập | `Jwt:Key` ngắn hơn 32 ký tự |
| PUT thiếu `hp` vẫn 200 | Phải khai báo `int?` + `[Required]` (đã làm trong `UpdateCharacterRequest`) |

Trước khi nộp: xóa `bin` và `obj`.
