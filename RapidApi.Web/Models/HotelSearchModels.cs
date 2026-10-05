using System.Text.Json.Serialization;

namespace RapidApi.Web.Models
{
    // ---------- API response ----------
    public class HotelSearchResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public HotelSearchData? Data { get; set; }
    }

    public class HotelSearchData
    {
        public List<HotelItem> Hotels { get; set; } = new();
        public List<MetaItem> Meta { get; set; } = new();
    }

    public class MetaItem { public string? Title { get; set; } }

    public class HotelItem
    {
        [JsonPropertyName("hotel_id")]
        public int HotelId { get; set; }
        public string? AccessibilityLabel { get; set; }
        public HotelProperty Property { get; set; } = new();
    }

    public class HotelProperty
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double ReviewScore { get; set; }
        public string? ReviewScoreWord { get; set; }
        public int ReviewCount { get; set; }
        public int PropertyClass { get; set; }
        public int AccuratePropertyClass { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PriceBreakdown? PriceBreakdown { get; set; }
    }

    public class PriceBreakdown
    {
        public Price? GrossPrice { get; set; }
        public Price? StrikethroughPrice { get; set; }
        public List<BenefitBadge> BenefitBadges { get; set; } = new();
        public string? ChargesInfo { get; set; }
    }

    public class Price
    {
        public double Value { get; set; }
        public string? Currency { get; set; }
        public string? AmountRounded { get; set; }
    }

    public class BenefitBadge { public string? Text { get; set; } }

    // ---------- Servise giden istek ----------
    public class HotelSearchRequest
    {
        public string DestId { get; set; } = "";
        public string SearchType { get; set; } = "";
        public DateTime Checkin { get; set; }
        public DateTime Checkout { get; set; }
        public int Adults { get; set; } = 2;
        public List<int> ChildrenAges { get; set; } = new();
        public int Rooms { get; set; } = 1;
        public string Currency { get; set; } = "EUR";
        public int Page { get; set; } = 1;
        public string? SortBy { get; set; }

        // Filtreler
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string? CategoriesFilter { get; set; }   // "class::4,class::5,reviewscore::80"
    }

    // ---------- View modeli ----------
    public class HotelListViewModel
    {
        public string Destination { get; set; } = "";
        public string DestinationLabel { get; set; } = "";
        public DateTime Checkin { get; set; }
        public DateTime Checkout { get; set; }
        public int Nights => Math.Max(1, (Checkout - Checkin).Days);
        public int Adults { get; set; } = 2;
        public string ChildrenAges { get; set; } = "";   // "5,8"
        public int ChildCount => string.IsNullOrWhiteSpace(ChildrenAges) ? 0 : ChildrenAges.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        public int Rooms { get; set; } = 1;
        public string Currency { get; set; } = "EUR";
        public string? SortBy { get; set; }

        // Filtreler (view'da seçili durumu göstermek ve sayfalama linklerinde korumak için)
        public string? Stars { get; set; }               // "4,5"
        public int? MinScore { get; set; }               // 6, 7, 8, 9
        public bool FreeCancellation { get; set; }
        public bool Breakfast { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public List<HotelCardVm> Hotels { get; set; } = new();
    }

    public class HotelCardVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Stars { get; set; }
        public string Img { get; set; } = "";
        public string? MediaBadge { get; set; }
        public string Location { get; set; } = "";
        public string Distance { get; set; } = "";
        public List<(string Icon, string Text)> Facilities { get; set; } = new();
        public string? BadgeKind { get; set; }
        public string? BadgeIcon { get; set; }
        public string? BadgeText { get; set; }
        public string ScoreWord { get; set; } = "";
        public string Score { get; set; } = "";
        public string Reviews { get; set; } = "";
        public string Price { get; set; } = "";
        public string? OldPrice { get; set; }
        public string PriceNote { get; set; } = "";
    }
}
