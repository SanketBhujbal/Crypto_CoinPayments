using System.ComponentModel;

namespace CoinPayment_POC.Models
{
    public class OrderModel
    {
        public string OrderId { get; set; }

        [DisplayName("Total Bill Amount")]
        public decimal OrderTotal { get; set; }

        [DisplayName("Product Name")]
        public string ProductName { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Email Address")]
        public string Email { get; set; }
    }
}
