namespace RapidApi.Web.Models
{
    public class HotelSearchRequestVm
    {
        public bool status { get; set; }
        public string message { get; set; }
        public long timestamp { get; set; }
        public Data data { get; set; }

        public class Data
        {
            public List<Hotel> hotels { get; set; }
            public List<Meta> meta { get; set; }
            public List<Appear> appear { get; set; }
        }

        public class Hotel
        {
            public int hotel_id { get; set; }
            public string accessibilityLabel { get; set; }
            public Property1 property { get; set; }
        }

        public class Property1
        {
            public string wishlistName { get; set; }
            public int mainPhotoId { get; set; }
            public float longitude { get; set; }
            public float reviewScore { get; set; }
            public int accuratePropertyClass { get; set; }
            public List<string> photoUrls { get; set; }
            public int propertyClass { get; set; }
            public string countryCode { get; set; }
            public Checkout checkout { get; set; }
            public string checkinDate { get; set; }
            public int position { get; set; }
            public string reviewScoreWord { get; set; }
            public int optOutFromGalleryChanges { get; set; }
            public int rankingPosition { get; set; }
            public string currency { get; set; }
            public int reviewCount { get; set; }
            public bool isPreferred { get; set; }
            public int id { get; set; }
            public string checkoutDate { get; set; }
            public float latitude { get; set; }
            public string recommendedUnitsConfigurationLabel { get; set; }
            public Pricebreakdown priceBreakdown { get; set; }
            public string name { get; set; }
            public List<string> blockIds { get; set; }
            public bool isFirstPage { get; set; }
            public int ufi { get; set; }
            public Checkin checkin { get; set; }
            public int qualityClass { get; set; }
            public bool isPreferredPlus { get; set; }
        }

        public class Checkout
        {
            public string untilTime { get; set; }
            public string fromTime { get; set; }
        }

        public class Pricebreakdown
        {
            public Grossprice grossPrice { get; set; }
            public Strikethroughprice strikethroughPrice { get; set; }
            public string chargesInfo { get; set; }
            public List<object> taxExceptions { get; set; }
            public List<Benefitbadge> benefitBadges { get; set; }
        }

        public class Grossprice
        {
            public float value { get; set; }
            public string currency { get; set; }
            public string amountRounded { get; set; }
        }

        public class Strikethroughprice
        {
            public string amountRounded { get; set; }
            public string currency { get; set; }
            public float value { get; set; }
        }

        public class Benefitbadge
        {
            public string identifier { get; set; }
            public string variant { get; set; }
            public string text { get; set; }
            public string explanation { get; set; }
        }

        public class Checkin
        {
            public string fromTime { get; set; }
            public string untilTime { get; set; }
        }

        public class Meta
        {
            public string title { get; set; }
        }

        public class Appear
        {
            public Component component { get; set; }
            public string id { get; set; }
            public string contentUrl { get; set; }
        }

        public class Component
        {
            public Props props { get; set; }
        }

        public class Props
        {
            public bool fill { get; set; }
            public Content content { get; set; }
            public string title { get; set; }
            public string text { get; set; }
            public Component2 component { get; set; }
            public string id { get; set; }
        }

        public class Content
        {
            public Props1 props { get; set; }
        }

        public class Props1
        {
            public bool fitContentWidth { get; set; }
            public List<Item> items { get; set; }
        }

        public class Item
        {
            public Props2 props { get; set; }
        }

        public class Props2
        {
            public Component1 component { get; set; }
        }

        public class Component1
        {
            public Props3 props { get; set; }
        }

        public class Props3
        {
            public List<Item1> items { get; set; }
            public string spacing { get; set; }
            public string icon { get; set; }
            public string accessibilityLabel { get; set; }
            public string tertiaryTintedColor { get; set; }
            public string variant { get; set; }
        }

        public class Item1
        {
            public Props4 props { get; set; }
        }

        public class Props4
        {
            public List<Text> text { get; set; }
        }

        public class Text
        {
            public string font { get; set; }
            public string text { get; set; }
            public string color { get; set; }
            public List<Linkaction> linkActions { get; set; }
        }

        public class Linkaction
        {
            public Props5 props { get; set; }
        }

        public class Props5
        {
            public string url { get; set; }
        }

        public class Component2
        {
            public Props6 props { get; set; }
        }

        public class Props6
        {
            public string contentUrl { get; set; }
            public Payload payload { get; set; }
            public string method { get; set; }
        }

        public class Payload
        {
            public string display_mode { get; set; }
            public List<Placement> placements { get; set; }
            public int screen_width { get; set; }
            public string currency_code { get; set; }
            public Header_Override header_override { get; set; }
            public string language_code { get; set; }
        }

        public class Header_Override
        {
            public int XBookingDeeplinkAffiliateId { get; set; }
            public string XBookingCCXPVersion { get; set; }
            public int XBookingAffiliateId { get; set; }
        }

        public class Placement
        {
            public Accommodation_Context accommodation_context { get; set; }
            public string client_id { get; set; }
            public string placement_name { get; set; }
            public Destination_Location destination_location { get; set; }
            public string page { get; set; }
        }

        public class Accommodation_Context
        {
            public string check_out { get; set; }
            public int adults { get; set; }
            public List<int> children { get; set; }
            public string check_in { get; set; }
        }

        public class Destination_Location
        {
            public string cc1 { get; set; }
            public int ufi { get; set; }
        }
    }
}