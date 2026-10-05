using System.Text.Json;
using System.Text.Json.Serialization;

namespace RapidApi.Web.Models
{
    // =====================================================================
    //  API response  (GET /api/v1/hotels/getHotelDetails)
    // =====================================================================
    public class HotelDetailResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public HotelDetailData? Data { get; set; }
    }

    public class HotelDetailData
    {
        [JsonPropertyName("hotel_id")] public int HotelId { get; set; }
        [JsonPropertyName("hotel_name")] public string? HotelName { get; set; }
        public string? Url { get; set; }
        [JsonPropertyName("review_nr")] public int ReviewNr { get; set; }
        [JsonPropertyName("accommodation_type_name")] public string? AccommodationTypeName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        [JsonPropertyName("country_trans")] public string? CountryTrans { get; set; }
        [JsonPropertyName("distance_to_cc")] public double DistanceToCc { get; set; }
        [JsonPropertyName("currency_code")] public string? CurrencyCode { get; set; }
        public int Soldout { get; set; }
        [JsonPropertyName("available_rooms")] public int AvailableRooms { get; set; }

        [JsonPropertyName("family_facilities")] public List<string> FamilyFacilities { get; set; } = new();
        [JsonPropertyName("product_price_breakdown")] public DetailPriceBreakdown? ProductPriceBreakdown { get; set; }
        [JsonPropertyName("property_highlight_strip")] public List<HighlightStripItem> PropertyHighlightStrip { get; set; } = new();
        [JsonPropertyName("facilities_block")] public FacilitiesBlock? FacilitiesBlock { get; set; }
        [JsonPropertyName("room_recommendation")] public List<RoomRecommendation> RoomRecommendation { get; set; } = new();
        [JsonPropertyName("hotel_important_information_with_codes")] public List<ImportantInfo> ImportantInformation { get; set; } = new();

        // "rooms" bir sözlük: { "8930048": { photos, highlights, ... } }
        // Müsait oda yoksa API bunu boş dizi [] olarak döndürebildiği için JsonElement tutuluyor.
        public JsonElement Rooms { get; set; }

        public List<RoomBlock> Block { get; set; } = new();
    }

    public class MoneyValue
    {
        public double Value { get; set; }
        public string? Currency { get; set; }
        [JsonPropertyName("amount_rounded")] public string? AmountRounded { get; set; }
    }

    public class DetailPriceBreakdown
    {
        [JsonPropertyName("gross_amount")] public MoneyValue? GrossAmount { get; set; }
        [JsonPropertyName("excluded_amount")] public MoneyValue? ExcludedAmount { get; set; }
        [JsonPropertyName("all_inclusive_amount")] public MoneyValue? AllInclusiveAmount { get; set; }
        [JsonPropertyName("gross_amount_per_night")] public MoneyValue? GrossAmountPerNight { get; set; }
        [JsonPropertyName("nr_stays")] public int NrStays { get; set; }
    }

    public class HighlightStripItem
    {
        public string? Name { get; set; }
        [JsonPropertyName("icon_list")] public List<IconRef> IconList { get; set; } = new();
    }

    public class IconRef { public string? Icon { get; set; } }

    public class FacilitiesBlock
    {
        public List<FacilityItem> Facilities { get; set; } = new();
    }

    public class FacilityItem
    {
        public string? Name { get; set; }
        public string? Icon { get; set; }
    }

    public class RoomRecommendation
    {
        [JsonPropertyName("block_id")] public string? BlockId { get; set; }
    }

    public class ImportantInfo { public string? Phrase { get; set; } }

    // ---- rooms sözlüğündeki her oda ----
    public class RoomInfo
    {
        public List<RoomPhoto> Photos { get; set; } = new();
        public List<RoomHighlight> Highlights { get; set; } = new();
        public string? Description { get; set; }
        [JsonPropertyName("bed_configurations")] public List<BedConfiguration> BedConfigurations { get; set; } = new();
    }

    public class RoomPhoto
    {
        [JsonPropertyName("url_max750")] public string? UrlMax750 { get; set; }
        [JsonPropertyName("url_max1280")] public string? UrlMax1280 { get; set; }
    }

    public class RoomHighlight
    {
        [JsonPropertyName("translated_name")] public string? TranslatedName { get; set; }
        public string? Icon { get; set; }
    }

    public class BedConfiguration
    {
        [JsonPropertyName("bed_types")] public List<BedType> BedTypes { get; set; } = new();
    }

    public class BedType
    {
        [JsonPropertyName("name_with_count")] public string? NameWithCount { get; set; }
    }

    // ---- block[]: oda + koşul (iptal / ödeme) seçenekleri ----
    public class RoomBlock
    {
        [JsonPropertyName("block_id")] public string BlockId { get; set; } = "";
        public string? Name { get; set; }
        [JsonPropertyName("room_name")] public string? RoomName { get; set; }
        [JsonPropertyName("room_id")] public long RoomId { get; set; }
        [JsonPropertyName("max_occupancy")] public JsonElement MaxOccupancy { get; set; }   // bazen "2", bazen 2
        [JsonPropertyName("room_surface_in_m2")] public double RoomSurfaceInM2 { get; set; }
        [JsonPropertyName("breakfast_included")] public int BreakfastIncluded { get; set; }
        [JsonPropertyName("refundable_until")] public string? RefundableUntil { get; set; }  // "2026-10-17 23:59:59 +0300"
        [JsonPropertyName("paymentterms")] public PaymentTerms? PaymentTerms { get; set; }
    }

    public class PaymentTerms
    {
        public PrepaymentInfo? Prepayment { get; set; }
        public CancellationInfo? Cancellation { get; set; }
    }

    public class PrepaymentInfo { public string? Type { get; set; } }          // "no_prepayment" ...
    public class CancellationInfo { public string? Type { get; set; } }        // "free_cancellation" ...

    // =====================================================================
    //  Servise giden istek
    // =====================================================================
    public class HotelDetailRequest
    {
        public int HotelId { get; set; }
        public DateTime Checkin { get; set; }
        public DateTime Checkout { get; set; }
        public int Adults { get; set; } = 2;
        public List<int> ChildrenAges { get; set; } = new();
        public int Rooms { get; set; } = 1;
        public string Currency { get; set; }
    }

    // =====================================================================
    //  View modeli
    // =====================================================================
    public class HotelDetailViewModel
    {
        public string? Error { get; set; }
        public bool HasError => !string.IsNullOrEmpty(Error);

        // Arama bağlamı
        public int HotelId { get; set; }
        public DateTime Checkin { get; set; }
        public DateTime Checkout { get; set; }
        public int Nights => Math.Max(1, (Checkout - Checkin).Days);
        public int Adults { get; set; } = 2;
        public string ChildrenAges { get; set; } = "";
        public int ChildCount => string.IsNullOrWhiteSpace(ChildrenAges)
            ? 0 : ChildrenAges.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        public int Rooms { get; set; } = 1;
        public string Currency { get; set; } = "EUR";

        // Liste sayfasından taşınan bilgiler (getHotelDetails yıldız ve puan döndürmüyor)
        public int Stars { get; set; }
        public string? Score { get; set; }
        public string? ScoreWord { get; set; }

        // Otel
        public string Name { get; set; } = "";
        public string AccommodationType { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? DistanceToCenter { get; set; }
        public int ReviewCount { get; set; }
        public bool SoldOut { get; set; }
        public string Summary { get; set; } = "";
        public string? MapEmbedUrl { get; set; }
        public string? MapLinkUrl { get; set; }

        public List<PhotoVm> Photos { get; set; } = new();
        public List<FacilityVm> Highlights { get; set; } = new();
        public List<FacilityVm> Facilities { get; set; } = new();
        public List<string> FamilyFacilities { get; set; } = new();
        public List<string> ImportantInfo { get; set; } = new();
        public List<DetailRoomVm> RoomOptions { get; set; } = new();
        public PriceSummaryVm? Price { get; set; }
    }

    public class PhotoVm
    {
        public string Url { get; set; } = "";
        public string Thumb { get; set; } = "";
        public string Caption { get; set; } = "";
    }

    public class FacilityVm
    {
        public string Icon { get; set; } = "bi-check2-circle";
        public string Name { get; set; } = "";
    }

    public class DetailRoomVm
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Img { get; set; }
        public List<(string Icon, string Text)> Specs { get; set; } = new();
        public List<(string Icon, string Text, bool Positive)> Perks { get; set; } = new();
        public string PolicyKind { get; set; } = "other";       // "free" | "none" | "other"
        public string PolicyText { get; set; } = "";
        public string? PrepayText { get; set; }
        public bool IsRecommended { get; set; }
        public string? Price { get; set; }                       // sadece fiyatı olan (önerilen) oda için dolu
        public string? PriceNote { get; set; }
    }

    public class PriceSummaryVm
    {
        public string Gross { get; set; } = "";
        public string? PerNight { get; set; }
        public string? Taxes { get; set; }
        public string Total { get; set; } = "";
    }
}
