using System.ComponentModel;

namespace CoinPayment_POC.Models {
    public class OrderModel {
        public string OrderId { get; set; }

        [DisplayName("Product Name")]
        public string ProductName { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Middle Name")]
        public string MiddleName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Service Address")]
        public string ServiceAddress { get; set; }

        [DisplayName("Mailing Address")]
        public string MailingAddress { get; set; }

        [DisplayName("Email Address")]
        public string EmailAddress { get; set; }

        [DisplayName("Payment Date")]
        public string PaymentDate { get; set; }

        [DisplayName("Payment Amount")]
        public string PaymentAmount { get; set; }
    }
}
