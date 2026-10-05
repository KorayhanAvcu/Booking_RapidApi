using Microsoft.AspNetCore.Mvc;
using RapidApi.Web.Models;
using RapidApi.Web.Services;

namespace RapidApi.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingApiService _bookingApiService;

        public BookingController(BookingApiService bookingApiService)
        {
            _bookingApiService = bookingApiService;
        }

        // GET /  veya /Booking/Index  -> Ana sayfa
        public IActionResult Index()
        {
            return View();
        }

        // GET /Booking/HotelList?destination=Milano&checkin=2026-10-28&checkout=2026-10-31
        //     &adults=2&childrenAges=5&rooms=1&currency=EUR&page=1
        //     &stars=4,5&minScore=8&freeCancellation=true&breakfast=true&minPrice=100&maxPrice=400
        public async Task<IActionResult> HotelList(
            string? destination,
            DateTime? checkin,
            DateTime? checkout,
            int adults = 2,
            string? childrenAges = null,
            int rooms = 1,
            string currency = "EUR",
            int page = 1,
            string? sortBy = null,
            string? stars = null,
            int? minScore = null,
            bool freeCancellation = false,
            bool breakfast = false,
            int? minPrice = null,
            int? maxPrice = null)
        {
            var checkinDate = checkin ?? DateTime.Today.AddDays(14);
            var checkoutDate = checkout ?? checkinDate.AddDays(3);

            // Çıkış, girişten önce veya aynı gün ise düzelt
            if (checkoutDate <= checkinDate)
                checkoutDate = checkinDate.AddDays(1);

            if (page < 1) page = 1;
            if (adults < 1) adults = 1;
            if (rooms < 1) rooms = 1;

            // Min fiyat max fiyattan büyükse yer değiştir
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                (minPrice, maxPrice) = (maxPrice, minPrice);

            // "4,5" -> [4, 5]
            var starList = (stars ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .Where(n => n is >= 1 and <= 5)
                .Distinct()
                .ToList();

            var vm = new HotelListViewModel
            {
                Destination = destination ?? "",
                Checkin = checkinDate,
                Checkout = checkoutDate,
                Adults = adults,
                ChildrenAges = childrenAges ?? "",
                Rooms = rooms,
                Currency = currency,
                Page = page,
                SortBy = sortBy,
                Stars = string.Join(",", starList),
                MinScore = minScore,
                FreeCancellation = freeCancellation,
                Breakfast = breakfast,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            if (string.IsNullOrWhiteSpace(destination))
                return View(vm);

            try
            {
                // 1) Şehir -> dest_id + search_type
                var destinations = await _bookingApiService.SearchDestinationAsync(destination);
                var dest = destinations.FirstOrDefault();

                if (dest == null)
                {
                    ViewBag.Error = "Aradığınız destinasyon bulunamadı.";
                    return View(vm);
                }

                vm.DestinationLabel = dest.label ?? dest.name;

                // 2) Çocuk yaşlarını parse et ("5,8" -> [5, 8])
                var children = (childrenAges ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(a => int.TryParse(a, out var age) ? age : (int?)null)
                    .Where(a => a.HasValue)
                    .Select(a => a!.Value)
                    .ToList();

                // 3) Otelleri filtrelerle birlikte çek
                var data = await _bookingApiService.SearchHotelsAsync(new HotelSearchRequest
                {
                    DestId = dest.dest_id,
                    SearchType = dest.search_type,
                    Checkin = checkinDate,
                    Checkout = checkoutDate,
                    Adults = adults,
                    ChildrenAges = children,
                    Rooms = rooms,
                    Currency = currency,
                    Page = page,
                    SortBy = sortBy,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    CategoriesFilter = BookingApiService.BuildCategoriesFilter(
                        starList, minScore, freeCancellation, breakfast)
                });

                vm.TotalCount = BookingApiService.ParseTotal(data);
                vm.Hotels = data.Hotels.Select(h => BookingApiService.MapToCard(h, currency)).ToList();
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Oteller şu anda getirilemedi. Lütfen daha sonra tekrar deneyin.";
            }

            return View(vm);
        }

        // GET /Booking/HotelDetail/89300?checkin=2026-10-18&checkout=2026-10-22&adults=2&childrenAges=5&rooms=1&currency=EUR
        //     &stars=4&score=8,7&scoreWord=Çok İyi     (son üçü liste sayfasından gelir, isteğe bağlı)
        public async Task<IActionResult> HotelDetail(
            int id,
            DateTime? checkin,
            DateTime? checkout,
            int adults = 2,
            string? childrenAges = null,
            int rooms = 1,
            string currency = "EUR",
            int stars = 0,
            string? score = null,
            string? scoreWord = null)
        {
            var checkinDate = checkin ?? DateTime.Today.AddDays(14);
            var checkoutDate = checkout ?? checkinDate.AddDays(3);
            if (checkoutDate <= checkinDate) checkoutDate = checkinDate.AddDays(1);
            if (adults < 1) adults = 1;
            if (rooms < 1) rooms = 1;

            var vm = new HotelDetailViewModel
            {
                HotelId = id,
                Checkin = checkinDate,
                Checkout = checkoutDate,
                Adults = adults,
                ChildrenAges = childrenAges ?? "",
                Rooms = rooms,
                Currency = currency,
                Stars = Math.Clamp(stars, 0, 5),
                Score = score is { Length: <= 5 } ? score : null,
                ScoreWord = scoreWord is { Length: <= 20 } ? scoreWord : null
            };

            var children = (childrenAges ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(a => int.TryParse(a, out var age) ? age : (int?)null)
                .Where(a => a.HasValue)
                .Select(a => a!.Value)
                .ToList();

            try
            {
                var data = await _bookingApiService.GetHotelDetailsAsync(new HotelDetailRequest
                {
                    HotelId = id,
                    Checkin = checkinDate,
                    Checkout = checkoutDate,
                    Adults = adults,
                    ChildrenAges = children,
                    Rooms = rooms,
                    Currency = currency
                });

                if (data == null || string.IsNullOrWhiteSpace(data.HotelName))
                {
                    vm.Error = "Otel bilgisi bulunamadı.";
                    return View(vm);
                }

                BookingApiService.MapDetailInto(vm, data);
            }
            catch (HttpRequestException)
            {
                vm.Error = "Otel bilgileri şu anda getirilemedi. Lütfen daha sonra tekrar deneyin.";
            }

            return View(vm);
        }
    }
}