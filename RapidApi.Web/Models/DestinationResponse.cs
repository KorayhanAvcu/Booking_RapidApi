namespace RapidApi.Web.Models
{
    public class DestinationResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public long timestamp { get; set; }
        public List<Destination> data { get; set; }
    }

    public class Destination
    {
        public string dest_id { get; set; }
        public string search_type { get; set; }
        public string dest_type { get; set; }
        public string country { get; set; }
        public string city_name { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public int nr_hotels { get; set; }
    }
}