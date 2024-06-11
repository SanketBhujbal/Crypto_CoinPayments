namespace CoinPayment_POC.Models
{
    public class ConversionModel
    {
        public string SelectedCurrency { get; set; }
        public decimal Amount { get; set; }
        public string SelectedCrypto { get; set; }
        public decimal ConvertedAmount { get; set; }
    }
}
