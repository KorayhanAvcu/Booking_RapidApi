using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using RapidApi.Web.Models;

namespace RapidApi.Web.Services
{
    public class BookingApiService
    {
        private const string Host = "booking-com15.p.rapidapi.com";
        private const string BaseUrl = "https://booking-com15.p.rapidapi.com/api/v1/hotels";

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly CultureInfo Tr = new("tr-TR");

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BookingApiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // ---------------------------------------------------------------
        // Ortak GET yardımcısı
        // ---------------------------------------------------------------
        private async Task<T?> GetAsync<T>(string endpoint, string queryString)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{BaseUrl}/{endpoint}?{queryString}");

            request.Headers.Add("x-rapidapi-key", _configuration["RapidApi:Key"]);
            request.Headers.Add("x-rapidapi-host", Host);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }

        // ---------------------------------------------------------------
        // 1) Destinasyon arama
        // ---------------------------------------------------------------
        public async Task<List<Destination>> SearchDestinationAsync(string query)
        {
            var result = await GetAsync<DestinationResponse>(
                "searchDestination",
                $"query={Uri.EscapeDataString(query)}");

            return result?.data ?? new List<Destination>();
        }

        // ---------------------------------------------------------------
        // 2) Otel arama
        // ---------------------------------------------------------------
        public async Task<HotelSearchData> SearchHotelsAsync(HotelSearchRequest r)
        {
            var inv = CultureInfo.InvariantCulture;

            var q = new Dictionary<string, string?>
            {
                ["dest_id"] = r.DestId,
                ["search_type"] = r.SearchType,
                ["arrival_date"] = r.Checkin.ToString("yyyy-MM-dd"),
                ["departure_date"] = r.Checkout.ToString("yyyy-MM-dd"),
                ["adults"] = r.Adults.ToString(inv),
                ["room_qty"] = r.Rooms.ToString(inv),
                ["page_number"] = r.Page.ToString(inv),
                ["currency_code"] = r.Currency,
                ["units"] = "metric",
                ["sort_by"] = r.SortBy,
                ["price_min"] = r.MinPrice?.ToString(inv),
                ["price_max"] = r.MaxPrice?.ToString(inv),
                ["categories_filter"] = r.CategoriesFilter,
                ["children_age"] = r.ChildrenAges.Count > 0
                    ? string.Join(",", r.ChildrenAges)
                    : null
            };

            var qs = string.Join("&", q
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}"));

            var result = await GetAsync<HotelSearchResponse>("searchHotels", qs);
            return result?.Data ?? new HotelSearchData();
        }

        // ---------------------------------------------------------------
        // API modelinden kart modeline
        // ---------------------------------------------------------------
        // Seçili filtreleri API'nin beklediği "anahtar::değer,anahtar::değer" biçimine çevirir.
        // Aynı anahtarın birden fazla değeri (class::4,class::5) "veya" mantığıyla çalışır.
        public static string? BuildCategoriesFilter(
            IEnumerable<int> stars, int? minScore, bool freeCancellation, bool breakfast)
        {
            var parts = new List<string>();

            parts.AddRange(stars.Where(s => s is >= 1 and <= 5).Distinct().Select(s => $"class::{s}"));

            if (minScore is >= 6 and <= 9)
                parts.Add($"reviewscore::{minScore.Value * 10}");

            if (freeCancellation) parts.Add("free_cancellation::1");
            if (breakfast) parts.Add("mealplan::1");

            return parts.Count > 0 ? string.Join(",", parts) : null;
        }

        public static int ParseTotal(HotelSearchData data)
        {
            var title = data.Meta.FirstOrDefault()?.Title ?? "";   // "992 properties"
            var digits = new string(title.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var n) ? n : data.Hotels.Count;
        }

        // fallbackCurrency: API fiyat nesnesinde para birimi gelmezse kullanıcının seçtiği para birimi
        public static HotelCardVm MapToCard(HotelItem item, string fallbackCurrency)
        {
            var p = item.Property;

            // accessibilityLabel içindeki gizli yön işaretlerini temizle
            var label = Regex.Replace(
                item.AccessibilityLabel ?? "",
                "[\u200E\u200F\u202A-\u202E]", "");
            var lines = label.Split('\n').Select(l => l.Trim()).ToList();

            var gross = p.PriceBreakdown?.GrossPrice;
            var old = p.PriceBreakdown?.StrikethroughPrice;

            var vm = new HotelCardVm
            {
                Id = item.HotelId,
                Name = p.Name,
                Stars = p.AccuratePropertyClass > 0 ? p.AccuratePropertyClass : p.PropertyClass,
                Img = p.PhotoUrls.Count > 1 ? p.PhotoUrls[1] : p.PhotoUrls.FirstOrDefault() ?? "",
                Score = p.ReviewScore.ToString("0.0", Tr),
                ScoreWord = TranslateScoreWord(p.ReviewScoreWord),
                Reviews = p.ReviewCount.ToString("N0", Tr),
                // Detay sayfasıyla aynı biçim: FormatMoney + fiyatın kendi para birimi
                Price = gross != null
                    ? FormatMoney(gross.Value, string.IsNullOrWhiteSpace(gross.Currency) ? fallbackCurrency : gross.Currency)
                    : "",
                OldPrice = old != null
                    ? FormatMoney(old.Value, string.IsNullOrWhiteSpace(old.Currency) ? fallbackCurrency : old.Currency)
                    : null,
                PriceNote = (p.PriceBreakdown?.ChargesInfo ?? "").Contains("Includes")
                    ? "Vergiler ve ücretler dahil"
                    : "+ vergi ve ücretler"
            };

            // Bölge • merkeze uzaklık   örn: "Bahcelievler • 13.6 km from centre"
            var locLine = lines.FirstOrDefault(l => l.Contains("from centre"));
            if (locLine != null)
            {
                var parts = locLine.Split('•', StringSplitOptions.TrimEntries);
                vm.Location = parts.Length > 1 ? parts[0] : "";
                var dist = parts.Last().Replace("from centre", "").Trim();
                vm.Distance = $"Merkeze {dist}";
            }

            // Olanaklar (label'dan)
            void Add(string key, string icon, string text)
            {
                if (label.Contains(key, StringComparison.OrdinalIgnoreCase))
                    vm.Facilities.Add((icon, text));
            }
            Add("Swimming pool", "bi-water", "Havuz");
            Add("Breakfast included", "bi-cup-hot", "Kahvaltı Dahil");
            Add("Family rooms", "bi-people", "Aile Odaları");
            Add("Sustainability", "bi-leaf", "Sürdürülebilirlik Sertifikalı");

            // Rozet
            if (label.Contains("Free cancellation", StringComparison.OrdinalIgnoreCase))
            {
                vm.BadgeKind = "deal";
                vm.BadgeIcon = "bi-check2-circle";
                vm.BadgeText = "Ücretsiz İptal";
            }
            else if (p.PriceBreakdown?.BenefitBadges.FirstOrDefault()?.Text is { } b)
            {
                vm.BadgeKind = "deal";
                vm.BadgeIcon = "bi-tag";
                vm.BadgeText = b switch
                {
                    "Late Escape Deal" => "Son Dakika Fırsatı",
                    "Mobile-only price" => "Mobil'e Özel Fiyat",
                    _ => b
                };
            }

            if (label.Contains("Preferred", StringComparison.OrdinalIgnoreCase))
                vm.MediaBadge = "Tercih Edilen";

            return vm;
        }

        // ---------------------------------------------------------------
        // 3) Otel detayı
        // ---------------------------------------------------------------
        public async Task<HotelDetailData?> GetHotelDetailsAsync(HotelDetailRequest r)
        {
            var inv = CultureInfo.InvariantCulture;

            var q = new Dictionary<string, string?>
            {
                ["hotel_id"] = r.HotelId.ToString(inv),
                ["arrival_date"] = r.Checkin.ToString("yyyy-MM-dd"),
                ["departure_date"] = r.Checkout.ToString("yyyy-MM-dd"),
                ["adults"] = r.Adults.ToString(inv),
                ["room_qty"] = r.Rooms.ToString(inv),
                ["currency_code"] = r.Currency,
                ["units"] = "metric",
                ["temperature_unit"] = "c",
                ["children_age"] = r.ChildrenAges.Count > 0 ? string.Join(",", r.ChildrenAges) : null,
                // İsteğe bağlı: appsettings.json -> "RapidApi": { "LanguageCode": "tr" }
                // Tanımlı değilse gönderilmez (yanıt İngilizce gelir).
                ["languagecode"] = _configuration["RapidApi:LanguageCode"]
            };

            var qs = string.Join("&", q
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}"));

            var result = await GetAsync<HotelDetailResponse>("getHotelDetails", qs);
            return result is { Status: true } ? result.Data : null;
        }

        // API modelinden detay view modeline
        public static void MapDetailInto(HotelDetailViewModel vm, HotelDetailData d)
        {
            var inv = CultureInfo.InvariantCulture;

            vm.Name = d.HotelName ?? "";
            vm.AccommodationType = TranslateAccommodationType(d.AccommodationTypeName);
            vm.Address = d.Address ?? "";
            vm.City = d.City ?? "";
            vm.District = d.District ?? "";
            vm.Country = d.CountryTrans ?? "";
            vm.Latitude = d.Latitude;
            vm.Longitude = d.Longitude;
            vm.DistanceToCenter = d.DistanceToCc > 0 ? d.DistanceToCc : null;
            vm.ReviewCount = d.ReviewNr;
            vm.SoldOut = d.Soldout == 1;

            // --- rooms sözlüğü (boş dizi gelirse atlanır) ---
            var roomInfos = new Dictionary<string, RoomInfo>();
            if (d.Rooms.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in d.Rooms.EnumerateObject())
                {
                    try
                    {
                        var info = prop.Value.Deserialize<RoomInfo>(JsonOpts);
                        if (info != null) roomInfos[prop.Name] = info;
                    }
                    catch (JsonException) { /* bozuk oda kaydını atla */ }
                }
            }

            // --- Fotoğraflar: bu endpoint otel fotoğrafı vermiyor, oda fotoğraflarından toplanır ---
            var seen = new HashSet<string>();
            foreach (var (roomId, info) in roomInfos)
            {
                var roomName = d.Block.FirstOrDefault(b => b.RoomId.ToString() == roomId)?.RoomName ?? "";
                foreach (var ph in info.Photos)
                {
                    var url = ph.UrlMax1280 ?? ph.UrlMax750;
                    if (string.IsNullOrEmpty(url) || !seen.Add(url)) continue;
                    vm.Photos.Add(new PhotoVm { Url = url, Thumb = ph.UrlMax750 ?? url, Caption = roomName });
                }
            }

            // --- Öne çıkanlar / olanaklar ---
            vm.Highlights = d.PropertyHighlightStrip
                .Where(h => !string.IsNullOrWhiteSpace(h.Name))
                .GroupBy(h => h.Name!)
                .Select(g => new FacilityVm
                {
                    Name = g.Key,
                    Icon = IconFor(g.First().IconList.FirstOrDefault()?.Icon)
                })
                .Take(10)
                .ToList();

            vm.Facilities = (d.FacilitiesBlock?.Facilities ?? new List<FacilityItem>())
                .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                .GroupBy(f => f.Name!)
                .Select(g => new FacilityVm { Name = g.Key, Icon = IconFor(g.First().Icon) })
                .ToList();

            vm.FamilyFacilities = d.FamilyFacilities.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().ToList();

            vm.ImportantInfo = d.ImportantInformation
                .Select(i => Regex.Replace(i.Phrase ?? "", "<.*?>", "").Trim())
                .Where(s => s.Length > 0)
                .ToList();

            // --- Fiyat özeti (önerilen oda için) ---
            var pb = d.ProductPriceBreakdown;
            if (pb?.GrossAmount != null)
            {
                // Para birimi: fiyatın kendi currency alanı; yoksa kullanıcının seçtiği para birimi
                var cur = string.IsNullOrWhiteSpace(pb.GrossAmount.Currency) ? vm.Currency : pb.GrossAmount.Currency;

                string CurOf(MoneyValue? m) => string.IsNullOrWhiteSpace(m?.Currency) ? cur : m!.Currency!;

                var gross = pb.GrossAmount.Value;
                var excluded = pb.ExcludedAmount?.Value ?? 0;
                var total = pb.AllInclusiveAmount?.Value ?? gross + excluded;

                vm.Price = new PriceSummaryVm
                {
                    Gross = FormatMoney(gross, cur),
                    PerNight = pb.GrossAmountPerNight != null
                        ? FormatMoney(pb.GrossAmountPerNight.Value, CurOf(pb.GrossAmountPerNight))
                        : null,
                    Taxes = excluded > 0 ? FormatMoney(excluded, CurOf(pb.ExcludedAmount)) : null,
                    Total = FormatMoney(total, pb.AllInclusiveAmount != null ? CurOf(pb.AllInclusiveAmount) : cur)
                };
            }

            // --- Oda seçenekleri ---
            var recommendedId = d.RoomRecommendation.FirstOrDefault()?.BlockId;
            for (var i = 0; i < d.Block.Count; i++)
            {
                var b = d.Block[i];
                roomInfos.TryGetValue(b.RoomId.ToString(), out var info);

                var room = new DetailRoomVm
                {
                    Id = b.BlockId,
                    Name = b.RoomName ?? b.Name ?? "Oda",
                    Img = info?.Photos.FirstOrDefault()?.UrlMax750
                };

                // Özellikler
                var beds = info?.BedConfigurations.FirstOrDefault()?.BedTypes
                    .Select(t => t.NameWithCount)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                if (beds is { Count: > 0 }) room.Specs.Add(("bi-moon-stars", string.Join(", ", beds)));

                var occupancy = ParseInt(b.MaxOccupancy);
                if (occupancy > 0) room.Specs.Add(("bi-people", $"En fazla {occupancy} kişi"));

                if (b.RoomSurfaceInM2 > 0)
                    room.Specs.Add(("bi-arrows-angle-expand", $"{b.RoomSurfaceInM2.ToString("0.#", Tr)} m²"));

                if (b.BreakfastIncluded == 1)
                    room.Perks.Add(("bi-cup-hot", "Kahvaltı dahil", true));

                var roomHighlights = (info?.Highlights ?? new List<RoomHighlight>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.TranslatedName))
                    .Take(4);
                foreach (var h in roomHighlights)
                    room.Perks.Add((IconFor(h.Icon), h.TranslatedName!, false));

                // İptal koşulu (kod alanına bakılır, metin diline bağlı değil)
                var cancelType = b.PaymentTerms?.Cancellation?.Type ?? "";
                if (cancelType.Contains("free", StringComparison.OrdinalIgnoreCase))
                {
                    room.PolicyKind = "free";
                    room.PolicyText = TryParseDate(b.RefundableUntil) is { } until
                        ? $"{until.ToString("d MMMM yyyy", Tr)} tarihine kadar ücretsiz iptal"
                        : "Ücretsiz iptal";
                }
                else if (cancelType.Contains("non", StringComparison.OrdinalIgnoreCase))
                {
                    room.PolicyKind = "none";
                    room.PolicyText = "İade edilmez";
                }
                else
                {
                    room.PolicyKind = "other";
                    room.PolicyText = "İptal koşulları geçerlidir";
                }

                if ((b.PaymentTerms?.Prepayment?.Type ?? "").Contains("no_prepayment", StringComparison.OrdinalIgnoreCase))
                    room.PrepayText = "Ön ödeme gerekmez";

                // Fiyat sadece önerilen blok için var
                var isRecommended = recommendedId != null ? b.BlockId == recommendedId : d.Block.Count == 1;
                if (isRecommended && vm.Price != null)
                {
                    room.IsRecommended = true;
                    room.Price = vm.Price.Gross;
                    room.PriceNote = vm.Price.Taxes != null ? $"+ {vm.Price.Taxes} vergi ve ücret" : "Vergiler ve ücretler dahil";
                }

                vm.RoomOptions.Add(room);
            }

            // --- Özet metin (API otel açıklaması vermiyor) ---
            var typeLower = vm.AccommodationType.ToLower(Tr);
            var loc = string.Join(", ", new[] { vm.District, vm.City }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            var summary = string.IsNullOrEmpty(loc)
                ? $"{vm.Name}, bir {typeLower} seçeneğidir."
                : $"{vm.Name}, {loc} bölgesinde yer alan bir {typeLower} seçeneğidir.";
            if (vm.DistanceToCenter is { } km)
                summary += $" Şehir merkezine {km.ToString("0.#", Tr)} km uzaklıktadır.";
            if (d.AvailableRooms > 0 && !vm.SoldOut)
                summary += $" Seçtiğiniz tarihler için {d.AvailableRooms} oda seçeneği müsait.";
            vm.Summary = summary;

            // --- Harita (OpenStreetMap, API anahtarı gerektirmez) ---
            if (d.Latitude != 0 || d.Longitude != 0)
            {
                var lat = d.Latitude; var lon = d.Longitude;
                string F(double v) => v.ToString("0.######", inv);
                vm.MapEmbedUrl = "https://www.openstreetmap.org/export/embed.html?bbox="
                    + $"{F(lon - 0.008)}%2C{F(lat - 0.005)}%2C{F(lon + 0.008)}%2C{F(lat + 0.005)}"
                    + $"&layer=mapnik&marker={F(lat)}%2C{F(lon)}";
                vm.MapLinkUrl = $"https://www.google.com/maps?q={F(lat)},{F(lon)}";
            }
        }

        // Liste ve detay sayfası aynı biçimlendiriciyi kullanır
        public static string FormatMoney(double value, string? currency)
        {
            var code = (currency ?? "").Trim().ToUpperInvariant();

            var culture = code switch
            {
                "TRY" => new CultureInfo("tr-TR"),
                "EUR" => new CultureInfo("de-DE"),
                "USD" => new CultureInfo("en-US"),
                "GBP" => new CultureInfo("en-GB"),
                _ => CultureInfo.InvariantCulture
            };

            var symbol = code switch
            {
                "EUR" => "€",
                "USD" => "$",
                "GBP" => "£",
                "TRY" => "₺",
                _ => code
            };

            return $"{symbol}{value.ToString("N0", culture)}";
        }

        private static int ParseInt(JsonElement e) => e.ValueKind switch
        {
            JsonValueKind.Number => e.TryGetInt32(out var n) ? n : 0,
            JsonValueKind.String => int.TryParse(e.GetString(), out var n) ? n : 0,
            _ => 0
        };

        // "2026-10-17 23:59:59 +0300" -> 2026-10-17
        private static DateTime? TryParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Length < 10) return null;
            return DateTime.TryParseExact(s[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt) ? dt : null;
        }

        // Booking ikon kodlarını Bootstrap Icons'a çevirir (bilinmeyenler için varsayılan ikon)
        private static string IconFor(string? code)
        {
            var key = (code ?? "").Replace("iconset/", "").ToLowerInvariant();
            return key switch
            {
                "wifi" => "bi-wifi",
                "snowflake" => "bi-snow",
                "pool" => "bi-water",
                "spa" => "bi-flower1",
                "fitness" => "bi-heart-pulse",
                "parking_sign" => "bi-p-circle",
                "food" or "food_and_drink" => "bi-egg-fried",
                "family" => "bi-people",
                "pawprint" => "bi-heart",
                "nosmoking" => "bi-slash-circle",
                "clean" => "bi-bell",
                "disabled" => "bi-universal-access",
                "bath" => "bi-droplet",
                "screen" => "bi-tv",
                "mountains" or "eye" => "bi-eye",
                "checkmark" => "bi-check2",
                _ => "bi-check2-circle"
            };
        }

        private static string TranslateAccommodationType(string? t) => (t ?? "").Trim().ToLowerInvariant() switch
        {
            "hotels" or "hotel" => "Otel",
            "apartments" or "apartment" => "Daire",
            "aparthotels" or "aparthotel" => "Apart Otel",
            "resorts" or "resort" => "Resort",
            "hostels" or "hostel" => "Hostel",
            "guest houses" or "guest house" => "Misafirhane",
            "villas" or "villa" => "Villa",
            "holiday homes" or "holiday home" => "Tatil Evi",
            "bed and breakfasts" or "bed and breakfast" => "Pansiyon",
            "" => "Konaklama",
            _ => t!.Trim()
        };

        private static string TranslateScoreWord(string? w) => w switch
        {
            "Exceptional" => "Olağanüstü",
            "Superb" => "Mükemmel",
            "Fabulous" => "Harika",
            "Very good" => "Çok İyi",
            "Good" => "İyi",
            "Pleasant" => "Keyifli",
            _ => w ?? ""
        };
    }
}