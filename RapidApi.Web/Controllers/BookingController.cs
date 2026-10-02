using Microsoft.AspNetCore.Mvc;

namespace RapidApi.Web.Controllers
{
    public class BookingController : Controller
    {
        // GET /  veya /Booking/Index  -> Ana sayfa
        public IActionResult Index()
        {
            return View();
        }

        // GET /Booking/HotelList?destination=Milano&checkin=...&checkout=...
        public IActionResult HotelList(string? destination)
        {
            return View();
        }

        // GET /Booking/HotelDetail/101
        public IActionResult HotelDetail(int id)
        {
            return View();
        }
    }
}
