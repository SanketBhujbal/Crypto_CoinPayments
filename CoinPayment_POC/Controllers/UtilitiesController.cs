using Microsoft.AspNetCore.Mvc;

namespace CoinPayment_POC.Controllers
{
    public class UtilitiesController : Controller
    {
        [HttpPost]
        public IActionResult ConvertCurrency(string selectedCurrency, decimal amount, string selectedCrypto)
        {
            // Placeholder for conversion logic. Replace with actual conversion logic.
            var conversionRate = 0.05m; // Example conversion rate
            var convertedAmount = amount * conversionRate;

            return Json(new { convertedAmount });
        }
    }
}
