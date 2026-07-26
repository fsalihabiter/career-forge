# CareerForge Geliştirme Takibi

Bu belge projenin kalıcı geliştirme hafızası ve sıralı backlog'udur. Sohbet
geçmişinden bağımsızdır. Durum değişiklikleri geliştirmeyle aynı anda bu dosyaya
işlenir.

## Şu anki durum

- Son güncelleme: 2026-07-26
- Aktif faz: Faz 2 — Öğrenme ve içerik MVP'si
- Devam eden iş: Yok
- Sıradaki iş: `CF-205 — İlk üç örnek ders`
- Son tamamlanan iş: `CF-204 — Öğrenme rehberi arayüzü`
- Genel hedef: Kayıt, hazırlık profili, tanılama, mülakat ve sonuç akışını
  güvenilir bir MVP dikey dilimi hâline getirmek.

## Durum özeti

| Faz | Tamamlandı | Toplam | Durum |
| --- | ---: | ---: | --- |
| 0. Temel iskelet | 6 | 6 | Tamamlandı |
| 1. Dikey dilimi güvenceye alma | 7 | 7 | Tamamlandı |
| 2. Öğrenme ve içerik MVP'si | 4 | 8 | Devam ediyor |
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
| CF-103 | Tamamlandı | Kimlik ve yetkilendirme integration testleri | CF-102 | Kayıt/giriş, 401 ve kullanıcı veri izolasyonu test edilir |
| CF-104 | Tamamlandı | Oturum akışı integration testleri | CF-102 | Başlat, cevapla, tamamla ve sonuç akışı test edilir |
| CF-105 | Tamamlandı | Frontend test altyapısı | CF-101 | Vitest + React Testing Library yapılandırılır |
| CF-106 | Tamamlandı | Frontend kritik akış testleri | CF-105 | Onboarding ve soru çözme davranışı test edilir |

### Faz 2 — Öğrenme ve içerik MVP'si

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-201 | Tamamlandı | Versiyonlanabilir içerik şeması | CF-104 | Ders, bölüm, soru, rubric ve pattern modeli tanımlanır |
| CF-202 | Tamamlandı | Git tabanlı içerik yükleme | CF-201 | İçerik koddan ayrılmış dosyalardan doğrulanarak yüklenir |
| CF-203 | Tamamlandı | Öğrenme rehberi API'si | CF-202 | Teknoloji, ders listesi ve ders detayı endpoint'leri hazırdır |
| CF-204 | Tamamlandı | Öğrenme rehberi arayüzü | CF-203 | Liste ve ders okuma akışı responsive ve erişilebilirdir |
| CF-205 | Sıradaki | İlk üç örnek ders | CF-202 | Middleware dahil üç tam ders içerir |
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

### 2026-07-26 — CF-103 başlatıldı

- Kayıt/giriş, korunan endpoint ve kullanıcılar arası veri izolasyonu için HTTP
  seviyesinde integration testlerinin eklenmesine başlandı.

### 2026-07-26 — CF-103 tamamlandı

- Kayıt sonrası token üretimi ve aynı kimlik bilgileriyle giriş akışı otomatik
  test edildi.
- Korunan profil endpoint'inin anonim isteklere `401 Unauthorized` döndürdüğü
  doğrulandı.
- Bir kullanıcının başka kullanıcıya ait tanılama oturumunu okuyamadığı ve API'nin
  kaynak varlığını sızdırmadan `404 Not Found` döndürdüğü doğrulandı.
- SQLite'ın `DateTimeOffset` karşılaştırma kısıtı için yalnızca SQLite test
  sağlayıcısında yakın-geçmiş filtresi bellekte uygulanırken PostgreSQL üretim
  sorgusu sunucu tarafında bırakıldı.
- Release test paketinin 12/12 testi ve güncel container üzerinde PostgreSQL
  oturum oluşturma smoke testi başarılıdır.

### 2026-07-26 — CF-104 başlatıldı

- Tanılama oturumunun başlatma, cevaplama, tamamlama ve sonuç adımlarını kapsayan
  uçtan uca integration testlerinin eklenmesine başlandı.

### 2026-07-26 — CF-104 tamamlandı

- Üç soruluk tanılama oturumunun oluşturulması, bütün cevapların kaydedilmesi,
  oturumun tamamlanması ve sonuç ekranı verisinin alınması otomatik test edildi.
- Model cevap, güçlü sinyal ve kırmızı bayrakların aktif oturumda gizli, tamamlanan
  oturumda görünür olduğu doğrulandı.
- Tamamlanan oturumdaki cevabın değiştirilemediği ve API'nin `409 Conflict`
  döndürdüğü doğrulandı.
- Boş cevap ve geçersiz öz değerlendirme puanının `400 Bad Request` ile
  reddedildiği doğrulandı.
- Release test paketinin 14/14 testi başarılıdır.

### 2026-07-26 — CF-105 başlatıldı

- Vitest, jsdom ve React Testing Library tabanlı frontend test altyapısının
  kurulmasına başlandı.

### 2026-07-26 — CF-105 tamamlandı

- Vitest, jsdom, React Testing Library, jest-dom ve user-event geliştirme
  bağımlılıkları eklendi; npm güvenlik taraması açık bulmadı.
- Vite test ortamı, ortak test setup dosyası, DOM temizliği ve yerel depolama
  izolasyonu yapılandırıldı.
- Tek seferlik ve izleme modlu npm test komutları eklendi.
- Uygulamanın hesap giriş ekranını render edip üç herkese açık katalog isteğini
  doğrulayan ilk component smoke testi eklendi ve geçti.
- TypeScript/Vite production build ve Oxlint başarılıdır; lint mevcut
  `useEffect` bağımlılığı için engelleyici olmayan bir uyarı üretmektedir.

### 2026-07-26 — CF-106 başlatıldı

- Kayıttan profil tamamlamaya uzanan onboarding ve tanılamadan sonuç ekranına
  uzanan soru çözme akışlarının component testlerine başlandı.

### 2026-07-26 — CF-106 tamamlandı

- Kayıt formunun token kaydedip onboarding ekranına geçmesi, dört onboarding
  adımının tamamlanması ve kişisel rota ekranının açılması kullanıcı etkileşimiyle
  test edildi.
- Mevcut oturumla dashboard'un yüklenmesi, tanılamanın başlatılması, cevabın
  yazılması, oturumun tamamlanması ve model cevaplı sonuç ekranının açılması test
  edildi.
- API istekleri URL ve HTTP metoduna göre kontrollü yanıtlarla izole edildi;
  testler arasında fetch, DOM ve localStorage temizliği sağlandı.
- Frontend component testlerinin 3/3'ü, TypeScript/Vite production build ve
  Oxlint başarılıdır; mevcut `useEffect` uyarısı engelleyici değildir.
- Faz 1'in yedi işi tamamlandı ve geliştirme sırası Faz 2 içerik şemasına geçti.

### 2026-07-26 — CF-201 başlatıldı

- Ders, pattern, sıralı bölüm, soru yayın durumu ve ağırlıklı rubric için
  versiyonlanabilir EF Core içerik şemasının tasarımına başlandı.

### 2026-07-26 — CF-201 tamamlandı

- Ders ve pattern içerikleri ortak versiyon, slug, teknoloji, seviye, yayın durumu
  ve öğrenme metadata'sı taşıyan TPH içerik modeliyle tanımlandı.
- Markdown ve isteğe bağlı kod örneği taşıyan, içerik başına benzersiz anahtar ve
  sıra kısıtlarına sahip bölüm modeli eklendi.
- Rubric ve ağırlıklı rubric boyutları ayrı, versiyonlanabilir modeller hâline
  getirildi; sorular yayın durumu ve rubric ilişkisi kazandı.
- Stable ID + version, slug + version, bölüm anahtarı/sırası ve rubric boyutu
  kısıtları için benzersiz indeksler eklendi.
- PostgreSQL migration'ı üretildi; mevcut sorular yayınlanmış kabul edilerek
  ağırlıkları toplam 100 olan varsayılan rubric'e geriye uyumlu biçimde bağlandı.
- Migration mevcut Docker PostgreSQL veritabanında başarıyla uygulandı ve EF model
  snapshot'ında bekleyen değişiklik kalmadığı doğrulandı.
- Seed verisi ilişkisel rubric modelini kullanacak şekilde güncellendi; backend
  testlerinin 16/16'sı geçti.

### 2026-07-26 — CF-202 başlatıldı

- Rubric, ders, pattern ve soru içeriklerini sürümlü Git dosyalarından doğrulayarak
  veritabanına aktaran idempotent yükleme altyapısının geliştirilmesine başlandı.

### 2026-07-26 — CF-202 tamamlandı

- Rubric, ders, pattern ve soru sözleşmeleri koddan ayrı, sürümlü JSON dosyaları
  olarak tanımlandı ve publish çıktısına dahil edildi.
- Dosyalar veritabanı değişikliğinden önce zorunlu alan, sürüm, benzersizlik,
  bölüm sırası, rubric ağırlığı ve referans bütünlüğü açısından doğrulanıyor.
- Doğrulanmış içerik transaction içinde stable ID + version anahtarıyla idempotent
  olarak ekleniyor veya güncelleniyor; eksik katalog referansları açık hatayla
  reddediliyor.
- Uygulama başlangıcında migration ve temel seed işleminden sonra Git içerik
  yükleyicisi otomatik çalışacak şekilde yapılandırıldı.
- Geçerli içeriğin iki kez yüklenmesi ve geçersiz içeriğin veritabanını
  değiştirmeden reddedilmesi integration testleriyle doğrulandı.
- Release build hatasız, backend testleri 18/18 başarılı ve güncel API containerı
  PostgreSQL üzerinde içeriği yükledikten sonra health kontrolünden geçti.

### 2026-07-26 — CF-203 başlatıldı

- Yalnızca yayınlanmış güncel sürümleri sunan teknoloji, ders listesi ve ders
  detayı endpoint'lerinin geliştirilmesine başlandı.

### 2026-07-26 — CF-203 tamamlandı

- `/api/learning/technologies` endpoint'i yalnızca yayınlanmış güncel dersleri
  bulunan teknolojileri ders sayılarıyla döndürecek şekilde eklendi.
- `/api/learning/lessons` endpoint'i stable ID başına son yayınlanmış sürümü
  teknoloji ve seviye filtreleriyle listeliyor; geçersiz seviyeler `400` dönüyor.
- `/api/learning/lessons/{slug}` endpoint'i ders metadata'sını, hedeflerini,
  ön koşullarını ve sıralı Markdown/kod bölümlerini döndürüyor; taslak ve bulunamayan
  içerikler yayınlanmıyor.
- Güncel sürüm seçimi, filtreleme, teknoloji ders sayısı, bölüm sırası ve taslak
  görünmezliği HTTP seviyesinde integration testiyle doğrulandı.
- Release build hatasız, backend testleri 19/19 başarılı ve endpoint sorguları
  güncel API containerında PostgreSQL üzerinde çalıştı.

### 2026-07-26 — CF-204 başlatıldı

- Mevcut CareerForge tasarım dili içinde teknoloji filtreli ders listesi ve odaklı
  ders okuma deneyiminin geliştirilmesine başlandı.

### 2026-07-26 — CF-204 tamamlandı

- Öğrenme rehberi oturum açmadan da erişilebilen ana navigasyon öğesi olarak
  eklendi; oturumlu kullanıcıların rota ve mülakat akışları korundu.
- Teknoloji başına ders sayısını gösteren filtre rayı, boş/yükleniyor durumları ve
  seviye, süre, teknoloji metadata'sı taşıyan responsive ders kataloğu geliştirildi.
- Ders okuyucu hedefleri, ön koşulları, sıralı bölüm navigasyonunu, okunabilir içerik
  kolonunu ve yatay kaydırılabilir kod örneklerini erişilebilir HTML ile sunuyor.
- Liste, teknoloji filtresi ve ders detayı geçişi component testiyle doğrulandı;
  frontend testlerinin 4/4'ü ve production build başarılıdır.
- Container build sırasında bulunan platform bağımlılığı lock uyumsuzluğu Linux
  Node 24 ortamında düzeltilerek `npm ci` ve Docker web build'i çalışır hâle getirildi.
- Çalışan container masaüstü ve 390 px mobil viewport'ta görsel olarak incelendi;
  mobil sayfa genişliğinde yatay taşma bulunmadı.

## Karar günlüğü

| Tarih | Karar | Gerekçe |
| --- | --- | --- |
| 2026-07-26 | Takip sohbet geçmişi yerine depoda tutulacak | Yeni görevlerde bağlam kaybını önlemek |
| 2026-07-26 | Tek aktif ana iş kullanılacak | “Devam” komutunu deterministik yapmak |
| 2026-07-26 | Önce dikey dilim ve test güvenliği tamamlanacak | Yeni özelliklerden önce mevcut akışı güvenilir kılmak |
| 2026-07-26 | Her tamamlanan geliştirme tek cümleyle raporlanıp commit edilecek | Değişiklik geçmişini küçük, anlaşılır ve geri alınabilir tutmak |
