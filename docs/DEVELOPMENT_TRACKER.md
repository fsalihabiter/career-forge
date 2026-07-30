# CareerForge Geliştirme Takibi

Bu belge projenin kalıcı geliştirme hafızası ve sıralı backlog'udur. Sohbet
geçmişinden bağımsızdır. Durum değişiklikleri geliştirmeyle aynı anda bu dosyaya
işlenir.

## Şu anki durum

- Son güncelleme: 2026-07-31
- Aktif faz: Faz 4 — Yönetim ve içerik yaşam döngüsü
- Devam eden iş: Yok
- Sıradaki iş: `CF-405 — İçerik versiyonlama`
- Son tamamlanan iş: `CF-404 — Taslak–inceleme–yayın akışı`
- Genel hedef: Kayıt, hazırlık profili, tanılama, mülakat ve sonuç akışını
  güvenilir bir MVP dikey dilimi hâline getirmek.

## Durum özeti

| Faz | Tamamlandı | Toplam | Durum |
| --- | ---: | ---: | --- |
| 0. Temel iskelet | 6 | 6 | Tamamlandı |
| 1. Dikey dilimi güvenceye alma | 7 | 7 | Tamamlandı |
| 2. Öğrenme ve içerik MVP'si | 8 | 8 | Tamamlandı |
| 3. İlerleme ve tekrar | 6 | 6 | Tamamlandı |
| 4. Yönetim ve içerik yaşam döngüsü | 4 | 6 | Devam ediyor |
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
| CF-205 | Tamamlandı | İlk üç örnek ders | CF-202 | Middleware dahil üç tam ders içerir |
| CF-206 | Tamamlandı | Pattern rehberi | CF-202 | Strategy ve Outbox dahil ilk pattern sayfaları hazırdır |
| CF-207 | Tamamlandı | Rubric tabanlı değerlendirme | CF-201 | Boyut bazlı puan ve açıklanabilir geri bildirim üretir |
| CF-208 | Tamamlandı | En az 10 soruluk doğrulanmış oturum | CF-207 | Güçlü sinyal, kırmızı bayrak ve model cevapları içerir |

### Faz 3 — İlerleme ve tekrar

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-301 | Tamamlandı | Ders ilerleme kaydı | CF-204 | Kullanıcı kaldığı yerden farklı cihazda devam eder |
| CF-302 | Tamamlandı | Beceri bazlı gelişim hesabı | CF-207 | Ölçülen seviye ve güven skoru geçmişi korunur |
| CF-303 | Tamamlandı | Tekrar listesi | CF-302 | Soru ekleme, çıkarma ve filtreleme yapılır |
| CF-304 | Tamamlandı | Spaced repetition planı | CF-303 | Sonraki tekrar tarihi hesaplanır |
| CF-305 | Tamamlandı | Kullanıcı dashboard'u | CF-301–304 | Sıradaki çalışma, zayıf alan ve son sonuç görünür |
| CF-306 | Tamamlandı | Erişilebilirlik ve responsive kabul turu | CF-305 | Temel akışlarda klavye, mobil ve taşma sorunları giderilir |

### Faz 4 — Yönetim ve içerik yaşam döngüsü

| ID | Durum | İş | Bağımlılık | Kabul özeti |
| --- | --- | --- | --- | --- |
| CF-401 | Tamamlandı | Rol ve policy tabanlı yetkilendirme | CF-103 | Öğrenci ve yönetici yetkileri API seviyesinde ayrılır |
| CF-402 | Tamamlandı | İçerik yönetimi API'si | CF-401, CF-202 | İçerik CRUD ve doğrulama akışları hazırdır |
| CF-403 | Tamamlandı | Admin içerik arayüzü | CF-402 | Ders, soru, rubric ve pattern yönetilir |
| CF-404 | Tamamlandı | Taslak–inceleme–yayın akışı | CF-403 | Yayın durumu ve yetki kontrolleri vardır |
| CF-405 | Sıradaki | İçerik versiyonlama | CF-404 | Eski cevap doğru içerik/rubric sürümüyle eşleşir |
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

### 2026-07-31 — CF-404 başlatıldı

- İçerik editörü ile yönetici sorumluluklarını ayıran taslak, inceleme, yayın ve
  arşiv durum geçişlerinin geliştirilmesine başlandı.

### 2026-07-31 — CF-404 tamamlandı

- İçerik editörü rolü ve editör/yönetici ortak içerik yönetimi policy'si eklendi;
  öğrenci erişim sınırı korunurken yayın yetkisi yöneticide bırakıldı.
- Taslak → inceleme → yayın → arşiv ana akışı ile incelemeden taslağa ve arşivden
  yeni taslağa dönüşler sunucu tarafında açık bir durum matrisiyle sınırlandı.
- Yeni kayıtların yalnızca taslak oluşturulması sağlandı ve CRUD gövdesiyle yayın
  durumunu atlama girişimleri reddedildi.
- İncelemedeki/yayındaki içeriklerin doğrudan silinmesi engellendi; yayınlama
  öncesinde sorunun bağlı rubric'inin yayında olması zorunlu kılındı.
- Admin çalışma alanına mevcut durumu ve role göre izin verilen incelemeye gönder,
  taslağa döndür, yayınla ve arşivle eylemleri eklendi.
- Editörün incelemeye gönderebilmesi fakat yayınlayamaması, yöneticinin
  yayınlama/arşivleme yetkisi, geçersiz geçiş ve doğrudan durum değiştirme
  integration testiyle doğrulandı.
- Backend testleri 33/33 ve frontend testleri 8/8 geçti; Release çözüm build'i,
  frontend production build'i ve lint uyarısız tamamlandı.
- Mevcut durum ve rol tabloları yeterli olduğu için migration gerekmedi;
  geliştirme sırası CF-405 içerik versiyonlamaya taşındı.

### 2026-07-31 — CF-403 başlatıldı

- Ders, pattern, rubric ve soruları tek yönetici çalışma alanında yöneten
  responsive ve erişilebilir içerik arayüzünün geliştirilmesine başlandı.

### 2026-07-31 — CF-403 tamamlandı

- JWT rol claim'i güvenli biçimde okunarak içerik yönetimi navigasyonu yalnızca
  yönetici rolündeki kullanıcılara gösterildi.
- Ders, pattern, rubric ve sorular kodlu editoryal sekmeler, aranabilir sürüm
  dizini ve ortak içerik sözleşmesi editörüyle tek çalışma alanında birleştirildi.
- Yeni kayıt, detay yükleme, güncelleme ve onaylı silme işlemleri CF-402 API'sine
  bağlandı; API doğrulama mesajları arayüzde doğrudan gösteriliyor.
- Masaüstünde çift kolonlu, tablette tek kolonlu ve mobilde yatay tür raylı
  responsive düzen; görünür odaklar ve semantik tab yapısı tamamlandı.
- Yönetici görünürlüğü, dört içerik türü, ders oluşturma ve API çağrısı component
  testiyle doğrulandı; frontend testleri 8/8 ve backend testleri 32/32 geçti.
- Release çözüm build'i, frontend production build'i ve lint uyarısız tamamlandı;
  375 px tarayıcı kabul kontrolünde yatay sayfa taşması görülmedi.
- Şema veya API değişikliği gerekmedi ve geliştirme sırası CF-404
  taslak–inceleme–yayın akışına taşındı.

### 2026-07-31 — CF-402 başlatıldı

- Ders, pattern, soru ve rubric içerikleri için yönetici yetkili CRUD ve doğrulama
  akışlarının geliştirilmesine başlandı.

### 2026-07-31 — CF-402 tamamlandı

- Ders, pattern, rubric ve soru içerikleri için yönetici policy'si altında
  listeleme, detay, oluşturma, güncelleme ve silme endpoint'leri eklendi.
- Stable ID/sürüm ve slug çakışmaları; zorunlu alanlar, bölüm/boyut
  benzersizliği, rubric ağırlığı ve katalog/rubric referansları doğrulanıyor.
- Kullanıcı verilerinin referans verdiği sorular ile soruların kullandığı
  rubric'ler güvenli biçimde silinmeye karşı korunuyor.
- Öğrenci erişim reddi, dört içerik türünün CRUD akışı, güncelleme, doğrulama ve
  referans bütünlüğü HTTP seviyesinde integration testleriyle doğrulandı.
- Backend testleri 32/32, frontend testleri 7/7 geçti; Release çözüm build'i,
  frontend production build'i ve lint uyarısız tamamlandı.
- Şema değişikliği gerekmedi; mevcut sürümlü içerik tabloları kullanıldı ve
  geliştirme sırası CF-403 admin içerik arayüzüne taşındı.

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

### 2026-07-26 — CF-205 başlatıldı

- Middleware sırası, PostgreSQL sorgu planı ve React istek yarışları konularında
  üç uygulamalı, yayınlanabilir ders içeriğinin hazırlanmasına başlandı.

### 2026-07-26 — CF-205 tamamlandı

- ASP.NET Core middleware sırası dersi; iki yönlü pipeline, kimlik/yetki sırası,
  hata sınırı ve davranış tabanlı test bölümleriyle tamamlanıp yayınlandı.
- PostgreSQL sorgu planı dersi; güvenli ölçüm, cardinality sapması, bileşik/partial
  indeks seçimi ve ölçüm sonrası doğrulama bölümleriyle tamamlanıp yayınlandı.
- React istek yarışları dersi; yarış zaman çizelgesi, AbortController, istek kimliği
  ve ters tamamlanma sırası testi bölümleriyle tamamlanıp yayınlandı.
- Her ders üç öğrenme hedefi, ön koşullar, dört sıralı bölüm ve çalıştırılabilir
  C#, SQL veya TypeScript örnekleri taşıyor.
- Gerçek PostgreSQL doğrulamasında ortaya çıkan bölüm anahtarı güncelleme hatası,
  ana kaydı koruyup alt bölümleri transaction içinde deterministik yeniden kuran
  importer akışıyla giderildi ve regresyon testi eklendi.
- Release build hatasız, backend testleri 20/20 başarılı; container importer'ı üç
  dersi yükledi ve API middleware detayında dört bölümü doğru sırada döndürdü.

### 2026-07-26 — CF-206 başlatıldı

- Strategy ve Transactional Outbox pattern içeriklerini API ve mevcut öğrenme
  rehberi deneyimi içinde yayınlama çalışmasına başlandı.

### 2026-07-26 — CF-206 tamamlandı

- Strategy ve Transactional Outbox patternleri üç öğrenme hedefi, dört sıralı bölüm
  ve uygulanabilir C#, SQL veya yapılandırma örnekleriyle tamamlanıp yayınlandı.
- Pattern liste ve detay endpoint'leri yalnızca stable ID başına son yayınlanmış
  sürümü döndürecek şekilde eklendi.
- Öğrenme rehberi, dersler ve patternler arasında erişilebilir sekmelerle geçiş
  yapan ve ortak odaklı okuyucuyu kullanan tek çalışma alanına dönüştürüldü.
- Release build hatasız, backend testleri 20/20, frontend component testleri 4/4
  başarılıdır; lint yalnızca önceden bilinen engelleyici olmayan `useEffect` uyarısını
  üretmektedir.
- Güncel container'lar üzerinde PostgreSQL'e iki pattern yüklendiği ve Outbox
  detayının dört bölümü doğru biçimde sunduğu gerçek API isteğiyle doğrulandı.

### 2026-07-26 — CF-207 başlatıldı

- Öz değerlendirmeyi sistem ölçümünden ayıran, rubric boyutlarına göre puan ve
  açıklanabilir geri bildirim üreten değerlendirme akışına başlandı.

### 2026-07-26 — CF-207 tamamlandı

- Cevaplar beklenen sinyal, riskli yaklaşım, gerekçelendirme, alternatif ve anlatım
  kanıtlarıyla rubric'in dört ağırlıklı boyutunda deterministik olarak puanlanıyor.
- Her boyut puanı, ağırlığı ve insan tarafından okunabilir gerekçesi ile rubric
  sürümü oturum sorusuna JSON snapshot olarak kaydediliyor.
- Kullanıcı beceri seviyesi öz puan yerine ağırlıklı sistem ölçümünden güncelleniyor;
  öz değerlendirme karşılaştırma için ayrı tutuluyor.
- Sonuç ekranına toplam sistem ölçümü, öz puan ve her rubric boyutu için erişilebilir
  ilerleme göstergesi ile açıklayıcı geri bildirim eklendi.
- Kalıcılık için PostgreSQL migration'ı eklendi; backend testleri 22/22, frontend
  testleri 4/4 ve production build başarılıdır, lint yalnızca önceden bilinen
  engelleyici olmayan `useEffect` uyarısını üretmektedir.
- Güncel container'larda migration uygulandı; gerçek PostgreSQL oturumunda üç cevap
  değerlendirilip sonuç API'sinden dört boyut, toplam sistem puanı ve ayrı öz puan
  döndüğü doğrulandı.

### 2026-07-26 — CF-208 başlatıldı

- On soruluk yayınlanmış soru bankasını Git içeriklerine taşıma ve profil
  filtrelerinden bağımsız olarak istenen oturum boyutunu güvenle tamamlama çalışmasına
  başlandı.

### 2026-07-26 — CF-208 tamamlandı

- Dokuz yeni soru eklenerek Git tabanlı yayınlanmış soru bankası 10 soruya çıkarıldı;
  her soru kapsamlı model cevap, en az üç güçlü sinyal ve birden fazla kırmızı bayrak
  taşıyor.
- Soru tanımları kod içindeki seed verisinden kaldırıldı; test ve üretim başlangıcı
  katalog seed'inden sonra aynı sürümlü içerik importunu kullanacak biçimde hizalandı.
- İçerik doğrulaması model cevabı, güçlü sinyal ve kırmızı bayrak alanlarını zorunlu
  kılarak eksik değerlendirme içeriğinin yayınlanmasını engelliyor.
- Oturum seçimi yalnızca yayınlanmış soruları kullanıyor; profil tercihleri önce
  uygulanıyor, banka yeterliyse istenen sayı farklı teknolojilerden güvenle
  tamamlanıyor.
- On soruluk mülakatın benzersiz ve dengeli seçimi, aktifken cevap anahtarlarının
  gizlenmesi, tamamlanması ve tüm soruların rubric değerlendirmesi integration
  testiyle doğrulandı.
- Backend testleri 23/23, frontend testleri 4/4 ve production build başarılıdır;
  lint yalnızca önceden bilinen engelleyici olmayan `useEffect` uyarısını üretmektedir.
- Gerçek PostgreSQL smoke testinde profilsiz kullanıcıya altı soru türüne yayılan
  10 soru verildi; model cevapların aktifken gizli, tamamlandıktan sonra 10/10
  görünür ve dört boyutta değerlendirilmiş olduğu doğrulandı.

### 2026-07-26 — CF-301 başlatıldı

- Ders bölümlerinin kullanıcı hesabına kaydedilmesi, kalınan bölümün farklı cihazda
  geri yüklenmesi ve okuyucuda ilerleme durumunun görünür kılınması çalışmasına
  başlandı.

### 2026-07-26 — CF-301 tamamlandı

- Ders stable ID ve sürümü başına kullanıcıya özel son bölüm, tamamlanan bölüm
  anahtarları, başlangıç, güncelleme ve tamamlanma zamanlarını saklayan ilerleme
  modeli ile PostgreSQL migration'ı eklendi.
- Yetkilendirilmiş ilerleme okuma ve güncelleme endpoint'leri yalnızca güncel
  yayınlanmış dersleri kabul ediyor; yabancı bölüm anahtarlarını reddediyor ve
  hesaplar arasında veri izolasyonu sağlıyor.
- Okuyucunun editoryal yapısı korunarak sol bölüm rayı tamamlanan, kalınan ve sıradaki
  adımları gösteren bir ilerleme omurgasına dönüştürüldü; bölüm ve ders tamamlama
  eylemleri ile toplam ilerleme çubuğu eklendi.
- Kayıtlı kullanıcı ders açtığında hesap ilerlemesi yükleniyor ve kaldığı bölüme
  yönlendiriliyor; anonim okuyucuya ilerlemeyi korumak için giriş yönlendirmesi
  gösteriliyor.
- Anonim erişim, geçersiz bölüm, hesap izolasyonu ve ikinci oturumda geri yükleme
  integration testiyle; bölüm tamamlama davranışı component testiyle doğrulandı.
- Backend testleri 24/24, frontend testleri 4/4 ve production/container build
  başarılıdır; lint yalnızca önceden bilinen engelleyici olmayan `useEffect`
  uyarısını üretmektedir.
- Gerçek PostgreSQL smoke testinde ilerleme sıfırdan başlatıldı, bir bölüm kaydedildi
  ve aynı hesapla yeni giriş sonrasında son bölüm ile tamamlanan bölüm aynen geri
  yüklendi.

### 2026-07-26 — CF-302 başlatıldı

- Her tamamlanan oturumun beceri puanı, ölçülen seviyesi ve kanıt güvenini tarihsel
  bir ölçüm noktası olarak saklama ve güncel gelişimi kümülatif kanıttan hesaplama
  çalışmasına başlandı.

### 2026-07-26 — CF-302 tamamlandı

- Her oturum ve kullanıcı becerisi için benzersiz değerlendirme noktası; oturum
  puanı, kümülatif puan, ölçülen seviye, güven, oturum ve toplam kanıt sayılarıyla
  kalıcı olarak saklanıyor.
- Güncel beceri puanı tüm oturumlardaki kanıt sayısıyla ağırlıklandırılıyor; ölçülen
  seviye bu kümülatif puandan, güven ise toplam bağımsız kanıt sayısından üretiliyor.
- Oturum tamamlama öz puan yerine rubric sistem ölçümünü kullanıyor ve aynı oturum
  yeniden işlense bile benzersiz tarihçe kaydı sayesinde çoğaltılmıyor.
- Kullanıcıya ait beceri geçmişini kronolojik ölçüm noktalarıyla döndüren,
  hesaplar arasında veri sızdırmayan yetkilendirilmiş API endpoint'i eklendi.
- PostgreSQL migration'ı kullanıcı, oturum ve kullanıcı becerisi ilişkilerini
  cascade kuralları ve oturum-beceri benzersizlik kısıtıyla oluşturdu.
- İki oturumun tarihçeyi iki noktaya, toplam kanıtı 1'den 2'ye ve güveni %20'den
  %40'a taşıması ile başka hesabın geçmişi okuyamaması integration testiyle
  doğrulandı.
- Backend testleri 25/25, frontend testleri 4/4 ve production/container build
  başarılıdır; lint yalnızca önceden bilinen engelleyici olmayan `useEffect`
  uyarısını üretmektedir.
- Gerçek PostgreSQL smoke testinde iki tanılama sonrasında geçmiş 2 noktaya ulaştı,
  kanıt sayıları 1 ve 2, güven değerleri %20 ve %40 olarak döndü ve güncel
  `UserSkill` seviyesi ile son tarihçe noktası eşleşti.

### 2026-07-26 — CF-303 başlatıldı

- Tamamlanan oturumdaki soruları kullanıcıya özel tekrar listesine ekleme, listeden
  çıkarma ve beceri/seviye bağlamında filtreleme çalışmasına başlandı.

### 2026-07-26 — CF-303 tamamlandı

- Yayındaki sorular kullanıcıya özel ve soru başına benzersiz bir tekrar kaydı
  olarak eklenebiliyor, listelenebiliyor ve çıkarılabiliyor.
- Yetkilendirilmiş tekrar API'si beceri ile seviye filtrelerini destekliyor; başka
  hesapların kayıtlarını okuma veya silme girişimleri veri izolasyonuyla engelleniyor.
- Sonuç ekranına tekrar listesine ekleme eylemi, ana navigasyona tekrar görünümü ve
  beceri/seviye filtreli yoğun bir tekrar defteri arayüzü eklendi.
- PostgreSQL migration'ı kullanıcı ve soruyla ilişkili tekrar kayıtlarını benzersiz
  kullanıcı-soru kısıtı ve uygun silme davranışlarıyla oluşturdu.
- Yetkilendirme, idempotent ekleme, filtreleme, hesap izolasyonu ve silme davranışı
  integration testiyle; sonuçtan listeye ekleme component testiyle doğrulandı.
- Backend testleri 26/26, frontend testleri 4/4, çözüm ve production/container
  build başarılıdır; lint yalnızca önceden bilinen engelleyici olmayan `useEffect`
  uyarısını üretmektedir.
- Gerçek PostgreSQL smoke testinde soru iki kez eklendiğinde aynı kayıt döndü,
  beceri filtresi tek kaydı buldu, silme sonrasında liste `[]` oldu ve web 200 döndü.

### 2026-07-27 — CF-304 başlatıldı

- Tekrar sorularına kullanıcının hatırlama kalitesine göre yeni aralık ve sonraki
  çalışma tarihi hesaplayan kalıcı planlama akışına başlandı.

### 2026-07-27 — CF-304 tamamlandı

- Tekrar kaydı; son çalışma, sonraki çalışma, gün aralığı, tekrar sayısı ve
  uyarlanabilir kolaylık katsayısını kalıcı olarak saklıyor.
- Tekrar, zor, iyi ve kolay sonuçları için başarısızlıkta sıfırlanan, başarılı
  hatırlamada kontrollü büyüyen deterministik aralık algoritması eklendi.
- Kullanıcıya ait zamanlama endpoint'i geçersiz sonuçları reddediyor, yabancı hesap
  kayıtlarını gizliyor ve hesaplanan yeni planı tek işlemde kaydedip döndürüyor.
- Mevcut tekrar kayıtları ilk eklenme tarihinde çalışılacak biçimde geriye uyumlu
  taşındı; yeni PostgreSQL migration'ı kolaylık başlangıcını 2,5 olarak kuruyor.
- Tekrar defteri bugün/planlandı filtresi, sıradaki tarih damgası ve dört kademeli
  hatırlama şeridiyle bir çalışma takvimine dönüştürüldü.
- Aralık hesabı, geçersiz sonuç ve hesap izolasyonu integration testiyle; kolay
  değerlendirmeden sonra tarih/aralık güncellemesi component testiyle doğrulandı.
- Backend testleri 26/26, frontend testleri 5/5, çözüm ve production/container
  build başarılıdır; lint yalnızca önceden bilinen engelleyici olmayan `useEffect`
  uyarısını üretmektedir.
- Gerçek PostgreSQL smoke testinde ilk kayıt 0 günle başladı, iyi sonucu 1 güne,
  ardından kolay sonucu 7 güne ve iki tekrara taşıdı; sonraki tarih son çalışmadan
  tam 7 gün sonrası oldu ve web 200 döndü.

### 2026-07-30 — CF-305 başlatıldı

- Sıradaki çalışma, zayıf beceri alanı ve son oturum sonucunu tek kullanıcı
  dashboard'unda birleştirme çalışmasına başlandı.

### 2026-07-30 — CF-305 tamamlandı

- Kullanıcıya özel dashboard API'si vadesi gelen tekrarları, ilk tamamlanmamış rota
  adımını, en zayıf aktif beceriyi ve son tamamlanan oturum sonucunu birleştiriyor.
- Sıradaki çalışma; vadesi gelmiş tekrar, öğrenme yolu, planlanmış tekrar ve ilk
  tanılama önceliğiyle deterministik olarak seçiliyor.
- Son sonuç puanı rubric değerlendirme snapshot'larından hesaplanıyor; JSON
  adlandırma uyumsuzluğunun puanı sıfırlaması daha sıkı integration testiyle
  yakalanıp giderildi.
- Endpoint yalnızca oturum sahibinin tekrar, beceri, rota ve sonuç verisini
  topluyor; boş hesap için güvenli başlangıç özeti döndürüyor.
- Dashboard, sıradaki işe doğrudan götüren odak alanı ile bugün bekleyen tekrar,
  zayıf alan ve son kanıtı bağlayan responsive bir kanıt şeridi kazandı.
- Yetkilendirme, hesap izolasyonu ve birleşik özet integration testiyle; özetin
  tüm alanları ve doğru eylemi göstermesi component testiyle doğrulandı.
- Backend testleri 27/27, frontend testleri 6/6, çözüm ve production web build
  başarılıdır; lint yalnızca önceden bilinen engelleyici olmayan `useEffect`
  uyarısını üretmektedir.
- Docker Desktop çalışmadığı için container/PostgreSQL smoke testi bu adımda
  yürütülemedi; yerel API integration testleri SQLite üzerinde tam geçti.

### 2026-07-30 — CF-306 başlatıldı

- Temel ekranlarda klavye odağı, semantik durum bildirimleri, mobil genişlik ve
  yatay taşma davranışları için kabul turuna başlandı.

### 2026-07-30 — CF-306 tamamlandı

- Ana içeriğe geçiş bağlantısı, ekran değişiminde odak aktarımı, görünür klavye
  odağı, sekme ve geçerli sayfa durumları erişilebilir semantiklerle tamamlandı.
- İlerleme göstergeleri, seçim düğmeleri, hata mesajları ve tekrar puanlama
  kontrolleri ekran okuyucu durumlarıyla zenginleştirildi.
- Mobil üst menü ve içerik kolonlarının minimum genişlikleri düzenlenerek 375 px
  mobil ve 1265 px masaüstü görünümünde yatay sayfa taşması giderildi.
- Tarayıcı kabul turunda ilk açılış odağının gövdede kaldığı, atlama bağlantısının
  doğru hedefe bağlandığı ve hesap sekmelerinin seçili durumları doğrulandı.
- Frontend testleri 7/7, backend testleri 27/27 geçti; frontend build/lint ve
  Release çözüm build'i uyarısız tamamlandı.
- Faz 3 bütün işleriyle tamamlandı ve geliştirme sırası CF-401'e taşındı.

### 2026-07-30 — CF-401 başlatıldı

- Öğrenci ve yönetici rollerinin JWT claim'leri ile API policy'lerinde ayrılması
  ve mevcut hesapların geriye uyumlu rol geçişi çalışmasına başlandı.

### 2026-07-30 — CF-401 tamamlandı

- Identity altyapısına öğrenci ve yönetici rolleri ile bunlara bağlı ayrı erişim
  policy'leri eklendi.
- Yeni kayıtlar varsayılan öğrenci rolü alıyor, rolsüz mevcut hesaplar başlangıçta
  idempotent biçimde öğrenci rolüne taşınıyor ve roller JWT'ye standart claim
  olarak yazılıyor.
- Kullanıcıya özel API grupları öğrenci policy'sine, yönetim erişim sınırı ise
  yönetici policy'sine bağlanarak iki yetki alanı API seviyesinde ayrıldı.
- Anonim isteğin `401`, öğrencinin yönetim alanında `403`, yöneticinin yönetim
  alanında `200` ve öğrenci alanında `403` aldığı integration testleriyle
  doğrulandı; rol seed işleminin tekrar çalıştırılabilirliği de kapsandı.
- Backend testleri 30/30 ve frontend testleri 7/7 geçti; çözüm build'i, frontend
  build'i ve lint uyarısız tamamlandı.
- Şema değişikliği gerekmedi; mevcut Identity rol tabloları kullanıldı ve
  geliştirme sırası CF-402 içerik yönetimi API'sine taşındı.

## Karar günlüğü

| Tarih | Karar | Gerekçe |
| --- | --- | --- |
| 2026-07-26 | Takip sohbet geçmişi yerine depoda tutulacak | Yeni görevlerde bağlam kaybını önlemek |
| 2026-07-26 | Tek aktif ana iş kullanılacak | “Devam” komutunu deterministik yapmak |
| 2026-07-26 | Önce dikey dilim ve test güvenliği tamamlanacak | Yeni özelliklerden önce mevcut akışı güvenilir kılmak |
| 2026-07-26 | Her tamamlanan geliştirme tek cümleyle raporlanıp commit edilecek | Değişiklik geçmişini küçük, anlaşılır ve geri alınabilir tutmak |
