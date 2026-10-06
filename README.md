# Booking RapidApi

Booking.com verilerini [RapidAPI](https://rapidapi.com) üzerinden çekerek otel arama, listeleme ve otel detayı sayfalarını sunan bir ASP.NET Core MVC uygulaması.

## Özellikler

**Otel arama ve listeleme**
- Şehir veya bölge adıyla destinasyon arama
- Giriş/çıkış tarihi, yetişkin sayısı, çocuk yaşları ve oda sayısı seçimi
- Para birimi seçimi: EUR, USD, TRY, GBP
- Filtreler: toplam fiyat aralığı, yıldız sayısı, misafir puanı, ücretsiz iptal, kahvaltı dahil
- Sıralama: önerilen, en düşük fiyat, en yüksek puan, yıldıza göre
- Sayfalama (arama ve filtreler sayfa değişince korunur)
- Kartlarda puan, mesafe, olanaklar, üstü çizili eski fiyat ve kampanya rozetleri

**Otel detayı**
- Oda fotoğraflarından oluşan galeri ve fotoğraf kaydırıcısı
- Öne çıkan özellikler, popüler olanaklar ve aile imkanları
- Oda seçenekleri: yatak düzeni, kapasite, metrekare, iptal ve ön ödeme koşulları
- Konaklama özeti: brüt fiyat, gecelik fiyat, ek vergi ve ücretler, toplam
- OpenStreetMap haritası ve Google Haritalar bağlantısı
- Önemli bilgiler (depozito, havuz çalışma dönemi vb.)

## Teknolojiler

- .NET 8, ASP.NET Core MVC (Razor Views)
- `HttpClient` ile yazılmış `BookingApiService`
- Bootstrap, Bootstrap Icons, jQuery
- [Booking.com (booking-com15) RapidAPI](https://rapidapi.com/DataCrawler/api/booking-com15)

Kullanılan API uç noktaları (`https://booking-com15.p.rapidapi.com/api/v1/hotels/`):

| Uç nokta | Kullanım |
|---|---|
| `searchDestination` | Şehir adından `dest_id` ve `search_type` bulma |
| `searchHotels` | Otel listesi (filtre, sıralama, sayfalama) |
| `getHotelDetails` | Otel detayı, odalar ve fiyat dökümü |

## Proje yapısı

```
Booking_RapidApi.slnx
└── RapidApi.Web
    ├── Controllers
    │   ├── BookingController.cs     # Index, HotelList, HotelDetail
    │   └── HomeController.cs
    ├── Models
    │   ├── DestinationResponse.cs
    │   ├── HotelSearchModels.cs     # Liste istek/yanıt ve görünüm modelleri
    │   ├── HotelDetailResponse.cs   # Detay yanıt modelleri
    │   └── ErrorViewModel.cs
    ├── Services
    │   └── BookingApiService.cs     # API çağrıları ve model eşlemeleri
    ├── Views
    │   ├── Booking                  # Index, HotelList, HotelDetail
    │   ├── Home
    │   └── Shared
    ├── wwwroot
    │   ├── css                      # home, hotel-list, hotel-detail, site
    │   ├── js                       # home, hotel-detail, site
    │   └── lib                      # bootstrap, jquery, jquery-validation
    ├── Program.cs
    └── appsettings.json
```

## Kurulum

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- RapidAPI hesabı ve [Booking.com (booking-com15)](https://rapidapi.com/DataCrawler/api/booking-com15) API'sine abonelik

### 1. Projeyi klonla

```bash
git clone https://github.com/KorayhanAvcu/Booking_RapidApi.git
cd Booking_RapidApi/RapidApi.Web
```

### 2. API anahtarını tanımla

Uygulama anahtarı `RapidApi:Key` ayarından okur. **Anahtarı asla `appsettings.json` içine yazıp commit'leme.** Geliştirme için user-secrets kullan:

```bash
dotnet user-secrets init
dotnet user-secrets set "RapidApi:Key" "<RAPIDAPI_ANAHTARIN>"
```

Alternatif olarak ortam değişkeni de kullanabilirsin:

```bash
# Windows (PowerShell)
$env:RapidApi__Key = "<RAPIDAPI_ANAHTARIN>"

# Linux / macOS
export RapidApi__Key="<RAPIDAPI_ANAHTARIN>"
```

İsteğe bağlı: otel detay yanıtlarının dilini değiştirmek için `appsettings.json` içine `"RapidApi": { "LanguageCode": "tr" }` eklenebilir. Tanımlı değilse yanıt İngilizce gelir.

### 3. Çalıştır

```bash
dotnet run
```

Uygulama varsayılan olarak `https://localhost:7111` ve `http://localhost:5288` adreslerinde açılır (`Properties/launchSettings.json`).

## Kullanım

Liste sayfası URL ile de çağrılabilir:

```
/Booking/HotelList?destination=Istanbul&checkin=2026-10-07&checkout=2026-10-10
    &adults=2&childrenAges=5,8&rooms=1&currency=TRY&page=1
    &stars=4,5&minScore=8&freeCancellation=true&breakfast=true&minPrice=1000&maxPrice=50000
```

Detay sayfası:

```
/Booking/HotelDetail/6965925?checkin=2026-10-07&checkout=2026-10-10&adults=2&rooms=1&currency=TRY
```

## Fiyat gösterimi hakkında not

Liste ve detay sayfaları Booking'in iki farklı uç noktasından veri alır, bu yüzden aynı otelin fiyatı iki sayfada birebir aynı olmayabilir:

- **Liste kartı:** `searchHotels` yanıtındaki `grossPrice` değerini gösterir. Yanıtta `chargesInfo` alanı "Includes taxes and charges" ise kart "Vergiler ve ücretler dahil" yazar.
- **Detay sayfası:** `getHotelDetails` yanıtındaki `gross_amount` (konaklama), `excluded_amount` (ek vergi ve ücretler) ve `all_inclusive_amount` (toplam) değerlerini gösterir.

Uygulama API'den gelen rakamları hesaplama yapmadan basar. Kampanyalar (Late Escape Deal, "Booking.com pays" vb.) dinamik olduğu için iki uç nokta bazı otellerde birkaç yüz lira fark verebilir. Örneğin 6 Ekim 2026'da CHER HOTEL & SPA Taksim için liste 19.443 TL, detay toplamı 19.254 TL döndürdü.

## Güvenlik

- API anahtarını repoya koyma. Daha önce commit edilmiş bir anahtar varsa RapidAPI panelinden yenile; commit geçmişinde kaldığı için dosyadan silmek yeterli değildir.
- `.gitignore`, `appsettings.*.json` dosyalarının yerel kopyalarını dışarıda bırakacak şekilde gözden geçirilmelidir.

## Lisans

Bu repo için henüz bir lisans belirtilmemiş.
