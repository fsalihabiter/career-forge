# CareerForge çalışma protokolü

Bu dosya, CareerForge deposunda çalışan tüm Codex görevleri için kalıcı proje
talimatıdır.

## Her yeni görevde başlangıç

1. Önce `docs/DEVELOPMENT_TRACKER.md` dosyasını tamamen oku.
2. `git status --short` ile çalışma ağacını kontrol et. Kullanıcının mevcut
   değişikliklerini koru.
3. Takip dosyasındaki `Şu anki durum` bölümünden ilk `Sıradaki` işi belirle.
4. Kullanıcının yeni mesajı belirli ve bağımsız bir görev istemiyorsa, ilk yanıtta
   sıradaki işi tek cümleyle söyle ve şu soruyla başla:

   `Sıradaki geliştirme <ID — başlık>. Buradan devam etmemi ister misin?`

5. Kullanıcı `devam`, `evet`, `başla` veya eşdeğer kısa bir onay verirse ek
   açıklama istemeden `Sıradaki` işi uygula.
6. Kullanıcı belirli başka bir geliştirme isterse onu uygula; takip dosyasındaki
   bağımlılıkları ve sıralamayı bozacaksa riski açıkça belirt.

## Takip kuralları

- Tek doğruluk kaynağı `docs/DEVELOPMENT_TRACKER.md` dosyasıdır.
- Bir işe başlamadan önce ilgili kaydı `Devam ediyor` yap ve `Son güncelleme`
  alanını güncelle.
- Aynı anda yalnızca bir ana iş `Devam ediyor` olabilir.
- İş tamamlanınca doğrulama komutlarını çalıştır, kabul kriterlerini kontrol et ve
  kaydı `Tamamlandı` yap.
- İş tamamlanamadıysa `Engelli` veya `Sıradaki` durumuna geçir; nedeni ve güvenli
  devam noktasını `Çalışma günlüğü` bölümüne yaz.
- Yeni bir gereksinim doğarsa uygun faza yeni, benzersiz bir iş kimliğiyle ekle.
- Bir iş bağımlılığı tamamlanmadan onu `Sıradaki` yapma.
- Takip dosyası güncellemesini, geliştirme değişikliğinin bir parçası kabul et.
- Durumları yalnızca şu değerlerden biriyle yaz:
  `Tamamlandı`, `Devam ediyor`, `Sıradaki`, `Bekliyor`, `Engelli`, `İptal`.

## Geliştirme yaklaşımı

- Yol haritasındaki sıraya ve bağımlılıklara göre çalışan dikey dilimleri tercih
  et.
- Her özellikte backend, frontend, veri modeli, güvenlik ve test etkisini birlikte
  değerlendir.
- En az ilgili build, lint ve test komutları başarılı olmadan işi tamamlanmış
  sayma. Ortam kaynaklı doğrulama yapılamıyorsa bunu kayda geçir.
- API davranışı değişirse integration testi; kullanıcı akışı değişirse component
  veya uçtan uca test ekle.
- Gizli anahtarları ve gerçek parolaları depoya yazma.
- Her tamamlanan geliştirme adımını kullanıcıya tek cümleyle özetle.
- Her tamamlanan geliştirme adımını, yalnızca o adıma ait dosyaları içeren anlamlı
  bir Git commit'iyle kaydet. Commit mesajında ilgili CareerForge iş kimliğini
  kullan.
- Kullanıcı açıkça istemedikçe push veya pull request oluşturma.

## “Devam” komutunun anlamı

`devam`, takip dosyasındaki `Devam ediyor` işi kaldığı yerden sürdürmek demektir.
`Devam ediyor` iş yoksa ilk `Sıradaki` iş başlatılır. Önce çalışma ağacı ve çalışma
günlüğü okunarak daha önce yapılmış adımlar tekrarlanmaz.
