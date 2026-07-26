# CareerForge Geliştirme Takibi

Bu belge projenin kalıcı geliştirme hafızası ve sıralı backlog'udur. Sohbet
geçmişinden bağımsızdır. Durum değişiklikleri geliştirmeyle aynı anda bu dosyaya
işlenir.

## Şu anki durum

- Son güncelleme: 2026-07-26
- Aktif faz: Faz 1 — Çalışan dikey dilimi güvenceye alma
- Devam eden iş: Yok
- Sıradaki iş: `CF-103 — Kimlik ve yetkilendirme integration testleri`
- Son tamamlanan iş: `CF-102 — API integration test altyapısı`
- Genel hedef: Kayıt, hazırlık profili, tanılama, mülakat ve sonuç akışını
  güvenilir bir MVP dikey dilimi hâline getirmek.

## Durum özeti

| Faz | Tamamlandı | Toplam | Durum |
| --- | ---: | ---: | --- |
| 0. Temel iskelet | 6 | 6 | Tamamlandı |
| 1. Dikey dilimi güvenceye alma | 3 | 7 | Devam ediyor |
| 2. Öğrenme ve içerik MVP'si | 0 | 8 | Bekliyor |
| 3. İlerleme ve tekrar | 0 | 6 | Bekliyor |
| 4. Yönetim ve içerik yaşam döngüsü | 0 | 6 | Bekliyor |
| 5. Üretim dayanıklılığı | 0 | 8 | Bekliyor |
| 6. Gelişmiş deneyim | 0 | 6 | Bekliyor |

## Sıralı geliştirme planı

### Faz 0 — Temel iskelet

| ID | Durum | İş | Kabul özeti |
| --- | --- | --- | --- |
| CF-001 | Tamamlandı | React + TypeScript + Vite iskeleti | Web projesi mevcut |
| CF-002 | Tamamlandı | ASP.NET Core API iskeleti | API, OpenAPI ve health endpoint mevcut |
| CF-003 | Tamamlandı | PostgreSQL + EF Core modeli | DbContext ve migration'lar mevcut |
| CF-004 | Tamamlandı | Kimlik doğrulama temeli | Kayıt, giriş ve JWT mevcut |
| CF-005 | Tamamlandı | Hazırlık profili ve öğrenme yolu temeli | Profil ve plan endpoint'leri mevcut |
| CF-006 | Tamamlandı | Tanılama/mülakat oturumu temeli | Oturum, cevap ve sonuç akışı mevcut |

### Faz 1 — Çalışan dikey dilimi güvenceye alma

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-100 | Tamamlandı | Docker build context ve port düzeltmesi | CF-001–006 | Compose tanımları düzeltildi |
| CF-101 | Tamamlandı | Docker Compose uçtan uca doğrulama | CF-100 | Tüm servisler build olur; health ve web erişilir; temel kullanıcı akışı smoke test edilir |
| CF-102 | Tamamlandı | API integration test altyapısı | CF-101 | WebApplicationFactory/test DB ile kritik akışlar otomatik çalışır |
| CF-103 | Sıradaki | Kimlik ve yetkilendirme integration testleri | CF-102 | Kayıt/giriş, 401 ve kullanıcı veri izolasyonu test edilir |
| CF-104 | Bekliyor | Oturum akışı integration testleri | CF-102 | Başlat, cevapla, tamamla ve sonuç akışı test edilir |
| CF-105 | Bekliyor | Frontend test altyapısı | CF-101 | Vitest + React Testing Library yapılandırılır |
| CF-106 | Bekliyor | Frontend kritik akış testleri | CF-105 | Onboarding ve soru çözme davranışı test edilir |

### Faz 2 — Öğrenme ve içerik MVP'si

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-201 | Bekliyor | Versiyonlanabilir içerik şeması | CF-104 | Ders, bölüm, soru, rubric ve pattern modeli tanımlanır |
| CF-202 | Bekliyor | Git tabanlı içerik yükleme | CF-201 | İçerik koddan ayrılmış dosyalardan doğrulanarak yüklenir |
| CF-203 | Bekliyor | Öğrenme rehberi API'si | CF-202 | Teknoloji, ders listesi ve ders detayı endpoint'leri hazırdır |
| CF-204 | Bekliyor | Öğrenme rehberi arayüzü | CF-203 | Liste ve ders okuma akışı responsive ve erişilebilirdir |
| CF-205 | Bekliyor | İlk üç örnek ders | CF-202 | Middleware dahil üç tam ders içerir |
| CF-206 | Bekliyor | Pattern rehberi | CF-202 | Strategy ve Outbox dahil ilk pattern sayfaları hazırdır |
| CF-207 | Bekliyor | Rubric tabanlı değerlendirme | CF-201 | Boyut bazlı puan ve açıklanabilir geri bildirim üretir |
| CF-208 | Bekliyor | En az 10 soruluk doğrulanmış oturum | CF-207 | Güçlü sinyal, kırmızı bayrak ve model cevapları içerir |

### Faz 3 — İlerleme ve tekrar

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-301 | Bekliyor | Ders ilerleme kaydı | CF-204 | Kullanıcı kaldığı yerden farklı cihazda devam eder |
| CF-302 | Bekliyor | Beceri bazlı gelişim hesabı | CF-207 | Ölçülen seviye ve güven skoru geçmişi korunur |
| CF-303 | Bekliyor | Tekrar listesi | CF-302 | Soru ekleme, çıkarma ve filtreleme yapılır |
| CF-304 | Bekliyor | Spaced repetition planı | CF-303 | Sonraki tekrar tarihi hesaplanır |
| CF-305 | Bekliyor | Kullanıcı dashboard'u | CF-301–304 | Sıradaki çalışma, zayıf alan ve son sonuç görünür |
| CF-306 | Bekliyor | Erişilebilirlik ve responsive kabul turu | CF-305 | Temel akışlarda klavye, mobil ve taşma sorunları giderilir |

### Faz 4 — Yönetim ve içerik yaşam döngüsü

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-401 | Bekliyor | Rol ve policy tabanlı yetkilendirme | CF-103 | Öğrenci ve yönetici yetkileri API seviyesinde ayrılır |
| CF-402 | Bekliyor | İçerik yönetimi API'si | CF-401, CF-202 | İçerik CRUD ve doğrulama akışları hazırdır |
| CF-403 | Bekliyor | Admin içerik arayüzü | CF-402 | Ders, soru, rubric ve pattern yönetilir |
| CF-404 | Bekliyor | Taslak–inceleme–yayın akışı | CF-403 | Yayın durumu ve yetki kontrolleri vardır |
| CF-405 | Bekliyor | İçerik versiyonlama | CF-404 | Eski cevap doğru içerik/rubric sürümüyle eşleşir |
| CF-406 | Bekliyor | İçerik kalite kontrolleri | CF-405 | Şema, bağlantı ve zorunlu alan kontrolleri CI'da çalışır |

### Faz 5 — Üretim dayanıklılığı

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-501 | Bekliyor | OpenTelemetry uçtan uca doğrulama | CF-101 | Log, metric ve trace Grafana'da ilişkilendirilebilir |
| CF-502 | Bekliyor | Health/readiness kontrolleri | CF-101 | DB ve kritik bağımlılık durumu raporlanır |
| CF-503 | Bekliyor | Güvenlik sertleştirmesi | CF-401 | Token, parola, rate limit, CORS ve veri maskeleme gözden geçirilir |
| CF-504 | Bekliyor | Redis cache | CF-305 | Cache-aside, TTL, fallback ve ölçümler vardır |
| CF-505 | Bekliyor | RabbitMQ ve mesaj sözleşmesi | CF-207 | Güvenilir asenkron değerlendirme temeli vardır |
| CF-506 | Bekliyor | Outbox ve idempotent consumer | CF-505 | Commit/publish tutarlılığı ve duplicate koruması test edilir |
| CF-507 | Bekliyor | Performans ve yük testleri | CF-502–506 | Kritik akışların eşik değerleri ve raporu vardır |
| CF-508 | Bekliyor | CI/CD kalite hattı | CF-406, CF-507 | Build, test, lint, güvenlik ve image kontrolleri otomatik çalışır |

### Faz 6 — Gelişmiş deneyim

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-601 | Bekliyor | Uygulama laboratuvarları | Faz 3 | En az üç yönlendirmeli laboratuvar vardır |
| CF-602 | Bekliyor | Gelişmiş beceri raporu | CF-305 | Zaman ve beceri boyutunda gelişim görünür |
| CF-603 | Bekliyor | Açıklanabilir yapay zekâ geri bildirimi | CF-405 | Rubric'e bağlı, güvenli ve denetlenebilir geri bildirim vardır |
| CF-604 | Bekliyor | Sesli mülakat provası | CF-603 | Sesli oturum ve metin geri bildirimi vardır |
| CF-605 | Bekliyor | Kod/SQL çalışma alanı | CF-601 | İzole ve güvenli uygulama ortamı vardır |
| CF-606 | Bekliyor | Mentor/takım görünümü | CF-602 | Yetkili kullanıcı gelişim özetini görebilir |

## Tamamlama ölçütü

Bir iş ancak aşağıdakilerin tamamı sağlandığında `Tamamlandı` olur:

1. Kabul kriterleri karşılandı.
2. İlgili otomatik testler eklendi veya güncellendi.
3. Backend build/test ve frontend build/lint/test kontrollerinden ilgili olanlar
   geçti.
4. Güvenlik, veri migration'ı ve geriye uyumluluk etkileri değerlendirildi.
5. Bu dosyada durum, tarih ve çalışma günlüğü güncellendi.

## Çalışma günlüğü

### 2026-07-26 — Takip sistemi kuruldu

- Proje köküne kalıcı Codex başlangıç ve “devam” protokolü eklendi.
- Mevcut kaynak kod ve yol haritası esas alınarak sıralı backlog oluşturuldu.
- Mevcut kod tabanı Faz 0 tamamlanmış kabul edildi.
- Son committeki Docker düzeltmesinden sonra `CF-101` sıradaki iş olarak seçildi.
- Yerel doğrulamada frontend bağımlılıkları kurulu olmadığı için `build` ve `lint`
  çalışmadı.
- API testi, çalışan `CareerForge.Api.exe` dosyayı kilitlediği için bu kontrolde
  tamamlanamadı. Bu durum `CF-101` sırasında yeniden doğrulanmalıdır.

### 2026-07-26 — CF-101 başlatıldı

- Docker Compose build, servis health ve temel kullanıcı akışı doğrulaması
  başlatıldı.

### 2026-07-26 — CF-101 tamamlandı

- API ve web image'ları güncel kaynaktan başarıyla üretildi; sekiz Compose
  servisi çalışır durumda ve PostgreSQL health kontrolü başarılıdır.
- API `/health` ve web kök adresi `200` döndürdü.
- Benzersiz yerel test kullanıcısıyla kayıt, hazırlık profili, üç soruluk
  tanılama, cevap kaydı, tamamlama ve sonuç akışı uçtan uca doğrulandı.
- Model cevapların aktif oturumda gizli, tamamlanan oturumda görünür olduğu
  doğrulandı.
- Smoke test, oturum tamamlama yanıtında `Question.Skill` yüklenmediği için oluşan
  `NullReferenceException` hatasını ortaya çıkardı. `SessionService.CompleteAsync`
  sorgusuna skill ilişkisi eklenerek hata giderildi ve smoke test tekrar geçti.
- Release yapılandırmasında backend testlerinin 7/7'si geçti.
- Docker web build'i TypeScript kontrolü ve Vite production build'iyle başarılı
  oldu.

### 2026-07-26 — CF-102 başlatıldı

- `WebApplicationFactory` ve izole test veritabanı tabanlı API integration test
  altyapısının kurulmasına başlandı.

### 2026-07-26 — CF-102 tamamlandı

- `WebApplicationFactory<Program>` tabanlı test hostu ve her fixture için bellekte
  yaşayan izole SQLite veritabanı eklendi.
- Test veritabanı otomatik oluşturulup gerçek `SeedData` ile dolduruluyor.
- Health endpoint'i ve seed edilmiş teknoloji/yetkinlik kataloglarını doğrulayan
  iki altyapı integration testi eklendi.
- Test ortamında OTLP exporter'ları devre dışı bırakılarak dış servise bağımlı
  beklemeler azaltıldı.
- SQLite transitif paketindeki yüksek önem dereceli güvenlik uyarısı, güncel
  `SQLitePCLRaw.bundle_e_sqlite3` sürümü sabitlenerek giderildi; vulnerability
  taraması temiz geçti.
- Release test paketinin 9/9 testi başarılıdır.

## Karar günlüğü

| Tarih | Karar | Gerekçe |
| --- | --- | --- |
| 2026-07-26 | Takip sohbet geçmişi yerine depoda tutulacak | Yeni görevlerde bağlam kaybını önlemek |
| 2026-07-26 | Tek aktif ana iş kullanılacak | “Devam” komutunu deterministik yapmak |
| 2026-07-26 | Önce dikey dilim ve test güvenliği tamamlanacak | Yeni özelliklerden önce mevcut akışı güvenilir kılmak |
| 2026-07-26 | Her tamamlanan geliştirme tek cümleyle raporlanıp commit edilecek | Değişiklik geçmişini küçük, anlaşılır ve geri alınabilir tutmak |
