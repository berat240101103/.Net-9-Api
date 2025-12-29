PROJE HAKKINDA






Bu proje, .NET 9 kullanılarak geliştirilmiş, Minimal API yaklaşımına sahip JSON tabanlı bir REST API uygulaması örneğidir.


Controller kullanılmadan endpoint tanımları yapılmıştır.


Uygulama katmanlı mimari (API, Application, Domain, Infrastructure) prensiplerine uygun olarak tasarlanmıştır.


User, Product, Category ve Review olmak üzere en az dört entity bulunmaktadır ve entity’ler arasında ilişkiler kurulmuştur.


Tüm CRUD işlemleri DTO (Create, Update, Response) yapıları kullanılarak gerçekleştirilmiş olup API doğrudan entity döndürmemektedir.


Veri erişimi için Entity Framework Core kullanılmış, Code First + Migration yaklaşımıyla veritabanı oluşturulmuştur.


Uygulama REST prensiplerine uygun olarak geliştirilmiş, HTTP method’ları (GET, POST, PUT, DELETE) amacına uygun biçimde kullanılmıştır.


HTTP status code’lar doğru senaryolarda tepki vermektedir.


Global Exception Middleware ile uygulama genelinde oluşabilecek beklenmeyen hatalar yakalanır ve kullanıcıya standart hata mesajları döndürür.


Swagger / OpenAPI entegrasyonu sayesinde tüm endpoint’ler dokümante edilmiş ve test edilebilir durumdadır.


JWT Authentication kullanılarak kullanıcı doğrulaması sağlanmış, rol bazlı yetkilendirme (Admin / User) uygulanmıştır.


Logging altyapısı entegre edilerek uygulama içerisindeki kritik işlemler ve hatalar loglanır.


Ayrıca soft delete, seed data ve asenkron veri erişimi gibi ek özellikler projeye dahil edilmiştir.











KULLANILAN TEKNOLOJILER





.NET 9 – REST API geliştirme


ASP.NET Core Minimal API – Endpoint tanımları


Entity Framework Core (EF Core) – ORM ve veritabanı işlemleri


SQL Server – Veritabanı


JWT (JSON Web Token) – Kimlik doğrulama ve yetkilendirme


Swagger / OpenAPI – API dokümantasyonu ve test arayüzü


Katmanlı Mimari (Clean Architecture yaklaşımı)


DTO (Data Transfer Objects) – Veri transferi ve response standardizasyonu


Global Exception Handling Middleware – Merkezi hata yönetimi


Serilog Logging











PROJE MIMARISI





---API Katmanı---


Uygulamanın dış dünyaya açılan katmanıdır.


Minimal API endpoint’leri burada tanımlanır, JWT authentication, middleware, logging ve Swagger yapılandırmaları bu katmanda bulunur.


---Application Katmanı---


İş kurallarının yer aldığı katmandır.


Service interface’leri ve DTO’lar burada tanımlanır. API katmanı doğrudan Infrastructure veya Domain’e bağımlı değildir.


---Domain Katmanı---


Uygulamanın çekirdek katmanıdır.


Entity’ler, ilişkiler ve ortak alanlar (CreatedAt, UpdatedAt, IsDeleted) burada yer alır. Hiçbir dış katmana bağımlılığı yoktur.


---Infrastructure Katmanı---


Veri erişiminden sorumludur.


Entity Framework Core, DbContext, migration’lar ve somut service implementasyonları bu katmanda bulunur.











ENDPOINT LİSTESİ





---AUTHENTICATION---





-POST /login-


Açıklama: Kullanıcı girişi yapar ve JWT token döndürür.


Yetki: Herkes erişebilir





---CATEGORIES---





-GET /categories-


Açıklama: Tüm kategorileri listeler.


Yetki: Herkes erişebilir


-POST /categories-


Açıklama: Yeni kategori ekler.


Yetki: Admin





---PRODUCTS---





-GET /products-


Açıklama: Tüm ürünleri listeler.


Yetki: Herkes erişebilir


-GET /products/{id}-


Açıklama: Belirli bir ürünü getirir.


Yetki: Herkes erişebilir


-POST /products-


Açıklama: Yeni ürün ekler.


Yetki: Admin


-PUT /products/{id}-


Açıklama: Ürün bilgilerini günceller.


Yetki: Admin


-DELETE /products/{id}-


Açıklama: Ürünü soft delete (mantıksal silme) yapar.


Yetki: Admin


-GET /products/all-


Açıklama: Test amaçlı oluşturulmuş, yetkili ve giriş yapmış kullanıcılar için ürün listesi.


Yetki: Sadece giriş yapan kullanıcılar (User / Admin)





---REVIEWS---





-GET /products/{id}/reviews-


Açıklama: Belirli bir ürüne ait yorumları listeler.


Yetki: Herkes erişebilir


-POST /products/{id}/reviews-


Açıklama: Ürüne yorum ekler.


Yetki: Admin





---USERS---





-GET /users-


Açıklama: Tüm kullanıcıları listeler.


Yetki: Herkes erişebilir


-GET /users/{id}-


Açıklama: Belirli bir kullanıcıyı getirir.


Yetki: Herkes erişebilir


-POST /users-


Açıklama: Yeni kullanıcı oluşturur.


Yetki: Herkes erişebilir


-PUT /users/{id}-


Açıklama: Kullanıcı bilgilerini günceller.


Yetki: Admin


-DELETE /users/{id}-


Açıklama: Kullanıcıyı soft delete yapar.


Yetki: Admin


-PUT /users/{id}/role-


Açıklama: Kullanıcının rolünü günceller.


Yetki: Admin





---NOTLAR---





-Admin yetkisi gerektiren endpoint’lerde JWT token zorunludur.


-(Username = "admin", Password = "123456") Seed data ile giriş yapılarak Admin yetkisi barındıran JWT token üretilebilir. 


-JWT token, Authorization header’ında gönderilmelidir.


-Silme işlemleri veritabanından fiziksel silme yapmaz.


-Tüm response’lar standart ApiResponse formatı ile döner.











API RESPONSE ORNEKLERİ





-GET /products-


{"success": true,"message": "Products listed","data": [{ "id": 2, " name": "Laptop", price: 70000, "categoryId": 1, "categoryName": "Electronics", "createdAt": "...", "updadateAt": "..."}]}


-POST/products- (Mouse, 1000, 1)


{ "success": true, "message": "Product created", "data": { "id": 3, "name": "Mouse", "price": 1000, "categoryId": 1, "categoryName": null, "createdAt": "...", "updatedAt": "..." } }


-GET/products/{id}- (3)


{ "success": true, "message": "Product fetched", "data": { "id": 3, "name": "Mouse", "price": 1000, "categoryId": 1, "categoryName": "Electronics", "createdAt": "...", "updatedAt": "..." } }


-PUT/products/{id}- (3,Keyboard,2000)


{ "success": true, "message": "Product updated", "data": null }


-DELETE/products/{id}- (3)


{ "success": true, "message": "Product soft-deleted", "data": null }


-Kayıt Bulunamadı- (404 Not Found)


{ "success": false, "message": "Product not found", "data": null }


-Yetkisiz Erişim- (401 Unauthorized)


{ "success": false, "message": "Unauthorized", "data": null }


-Yasaklanmış- (403 Forbidden)


{ "success": false, "message": "Forbidden", "data": null }


-Anlaşmazlık- (409 Conflict)


{ "success": false, "message": "Conflict", "data": null }


-Sunucu Hatası- (500 Internal Server Error)


{ "success": false, "message": "Internal server error", "data": null }


-POST /login-


{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."}











KURULUM TALİMATLARI





Projeyi çalıştırabilmek için aşağıdaki yazılımların sisteminizde kurulu olması gerekmektedir:


-.NET 9 SDK


-SQL Server (LocalDB veya SQL Server Express)


-Visual Studio 2022 veya VS Code


-Git





Projenin klonlanması için aşağıdaki komutları çalıştırın:


git clone https://github.com/berat240101103/.Net-9-Api.git


cd .\ApiProject\





'appsettings.json' dosyasında bağlantı cümlesini kendi ortamınıza göre düzenleyin:


"ConnectionStrings": { "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ApiProjectDb;Trusted_Connection=True;" }


Aynı dosyada JWT ayarlarının bulunduğundan emin olun:


"Jwt": { "Key": "SuperSecretKey_AtLeast_32_Chars_Long", "Issuer": "ApiProject", "Audience": "ApiProjectUsers", "DurationInMinutes": 60 }


Not: Key değeri en az 32 karakter olmalıdır.





Infrastructure projesi dizininde aşağıdaki komutları çalıştırın:


dotnet ef database update --project Infrastructure --startup-project Api





API projesini çalıştırmak için:


dotnet run --project Api


Alternatif olarak: 


cd .\Api\


dotnet run





Uygulama çalıştıktan sonra konsolda oluşan lokalhost adresini tarayıcıda açıp 'swagger' sayfa yolunu açın:


https://localhost:{port}/swagger


Not: Swagger arayüzü üzerinden tüm endpoint’ler test edilebilir.





'/login' endpoint’inden Seed data ile gelen varsayılan admin kullanıcısı verileriyle giriş yapın:


Username: "admin"


Password: "123456"


Daha sonra dönen JWT token’ı Swagger’daki Authorize butonuna ekleyin.


Yetkili endpoint’leri test edin.





Loglar hem konsola hem de Logs/ klasörü altına yazılır.


Serilog kullanılarak yapılandırılmıştır.
