using CoinPayment_POC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace CoinPayment_POC.Controllers
{
    public class CoinPaymentsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CoinPaymentsController(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ProcessCheckout()
        {
            var model = new OrderModel
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderTotal = 5,
                ProductName = "Pay By crypto - CoinPayment",
                FirstName = "Sanket",
                LastName = "Bhujbal",
                Email = "sanketbhujbal.sb@gmail.com"
            };
            return View(model);
        }
        [HttpPost]
        public void ProcessCheckout(OrderModel orderModel)
        {
            var queryParameters = CreateQueryParameters(orderModel);

            var baseUrl = _configuration.GetSection("CoinPayments")["BaseUrl"];
            var redirectUrl = QueryHelpers.AddQueryString(baseUrl, queryParameters);

            _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
        }
        private IDictionary<string, string> CreateQueryParameters(OrderModel orderModel)
        {
            //get store location  
            var storeLocation = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var queryParameters = new Dictionary<string, string>()
            {
                //IPN settings  
                ["ipn_version"] = "1.0",
                ["cmd"] = "_pay_auto",
                ["ipn_type"] = "simple",
                ["ipn_mode"] = "hmac",
                ["merchant"] = _configuration.GetSection("CoinPayments")["MerchantId"],
                ["allow_extra"] = "0",
                ["currency"] = "USD",
                ["amountf"] = orderModel.OrderTotal.ToString("N2"),
                ["item_name"] = orderModel.ProductName,

                //IPN, success and cancel URL  
                ["success_url"] = $"{storeLocation}/CoinPayments/SuccessHandler?orderNumber={orderModel.OrderId}",
                ["ipn_url"] = $"{storeLocation}/CoinPayments/IPNHandler",
                ["cancel_url"] = $"{storeLocation}/CoinPayments/CancelOrder",

                //order identifier                  
                ["custom"] = orderModel.OrderId,

                //billing info  
                ["first_name"] = orderModel.FirstName,
                ["last_name"] = orderModel.LastName,
                ["email"] = orderModel.Email,

            };
            return queryParameters;
        }

        public IActionResult SuccessHandler(string orderNumber)
        {
            ViewBag.OrderNumber = orderNumber;

            return View("PaymentResponse");
        }

        public IActionResult CancelOrder()
        {
            return View("PaymentResponse");
        }
        [HttpPost]
        public IActionResult IPNHandler()
        {
            byte[] parameters;
            using (var stream = new MemoryStream())
            {
                Request.Body.CopyTo(stream);
                parameters = stream.ToArray();
            }
            var strRequest = Encoding.ASCII.GetString(parameters);
            var ipnSecret = _configuration.GetSection("CoinPayments")["IpnSecret"];

            if (Helper.VerifyIpnResponse(strRequest, Request.Headers["hmac"], ipnSecret,
                out Dictionary<string, string> values))
            {
                values.TryGetValue("first_name", out string firstName);
                values.TryGetValue("last_name", out string lastName);
                values.TryGetValue("email", out string email);
                values.TryGetValue("amount1", out string amount1);
                values.TryGetValue("subtotal", out string subtotal);
                values.TryGetValue("status", out string status);
                values.TryGetValue("status_text", out string statusText);

                var newPaymentStatus = Helper.GetPaymentStatus(status, statusText);

                switch (newPaymentStatus)
                {
                    case PaymentStatus.Pending:
                        {
                            //TODO: update order status and use logging mechanism
                            //order is pending                      
                        }
                        break;
                    case PaymentStatus.Authorized:
                        {
                            //order is authorized                      
                        }
                        break;
                    case PaymentStatus.Paid:
                        {
                            //order is paid                      
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Issue Occurred with CoinPayments IPN  
            }

            //nothing should be rendered to visitor  
            return Content("");
        }
        public static class Helper
        {
            public static bool VerifyIpnResponse(string formString, string hmac, string ipnSecret, out Dictionary<string, string> values)
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var l in formString.Split('&'))
                {
                    var line = l.Trim();
                    var equalPox = line.IndexOf('=');
                    if (equalPox >= 0)
                        values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
                }

                values.TryGetValue("merchant", out string merchant);

                //verify hmac with secret  
                if (!string.IsNullOrEmpty(merchant) && !string.IsNullOrEmpty(hmac))
                {
                    if (hmac.Trim() == HashHmac(formString, ipnSecret))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }

            public static string HashHmac(string message, string secret)
            {
                Encoding encoding = Encoding.UTF8;
                using (HMACSHA512 hmac = new HMACSHA512(encoding.GetBytes(secret)))
                {
                    var msg = encoding.GetBytes(message);
                    var hash = hmac.ComputeHash(msg);
                    return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                }
            }

            public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
            {
                var result = PaymentStatus.Pending;

                int status = Convert.ToInt32(paymentStatus);

                switch (status)
                {
                    case 0:
                        result = PaymentStatus.Pending;
                        break;
                    case 1:
                        result = PaymentStatus.Authorized;
                        break;
                    case -1:
                        result = PaymentStatus.Cancelled;
                        break;
                    case 100:
                        result = PaymentStatus.Paid;
                        break;
                    default:
                        break;
                }
                return result;
            }
        }
        public enum PaymentStatus
        {
            Pending = 10,

            Authorized = 20,

            Paid = 30,

            Cancelled = 50,
        }
    }
}
