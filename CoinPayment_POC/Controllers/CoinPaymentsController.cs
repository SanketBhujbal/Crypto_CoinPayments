﻿using CoinPayment_POC.Models;
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
                ProductName = "Test payment - Pay By crypto currency",
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
                ["ipn_version"] = ConfigurationConstants.ipn_version,
                ["cmd"] = ConfigurationConstants.cmd,
                ["ipn_type"] = ConfigurationConstants.ipn_type,
                ["ipn_mode"] = ConfigurationConstants.ipn_mode,
                ["merchant"] = _configuration.GetSection("CoinPayments")["MerchantId"],
                ["allow_extra"] = ConfigurationConstants.allow_extra,
                ["currency"] = ConfigurationConstants.currency,
                ["amountf"] = orderModel.OrderTotal.ToString("N2"),
                ["item_name"] = orderModel.ProductName,

                //IPN, success and cancel URL  
                ["success_url"] = $"{storeLocation}"+ ConfigurationConstants.success_url +"{orderModel.OrderId}",
                ["ipn_url"] = $"{storeLocation}"+ ConfigurationConstants.ipn_url+" ",
                ["cancel_url"] = $"{storeLocation}" + ConfigurationConstants.cancel_url + "",

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
                values.TryGetValue(ConfigurationConstants.first_name, out string firstName);
                values.TryGetValue(ConfigurationConstants.last_name, out string lastName);
                values.TryGetValue(ConfigurationConstants.email, out string email);
                values.TryGetValue(ConfigurationConstants.amount1, out string amount1);
                values.TryGetValue(ConfigurationConstants.subtotal, out string subtotal);
                values.TryGetValue(ConfigurationConstants.status, out string status);
                values.TryGetValue(ConfigurationConstants.status_text, out string statusText);

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
    }
}
