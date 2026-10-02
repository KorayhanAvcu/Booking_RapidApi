namespace RapidApi.Web.Models
{
    public class CityIdSearchRequestVM
    {

        public class Rootobject
        {
            public bool status { get; set; }
            public string message { get; set; }
            public long timestamp { get; set; }
            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public string dest_id { get; set; }
            public string search_type { get; set; }
            public float latitude { get; set; }
            public string dest_type { get; set; }
            public string lc { get; set; }
            public int nr_hotels { get; set; }
            public string label { get; set; }
            public int hotels { get; set; }
            public string image_url { get; set; }
            public string cc1 { get; set; }
            public float longitude { get; set; }
            public string country { get; set; }
            public string name { get; set; }
            public int? city_ufi { get; set; }
            public string type { get; set; }
            public string region { get; set; }
            public string city_name { get; set; }
            public string roundtrip { get; set; }
        }

    }
}
