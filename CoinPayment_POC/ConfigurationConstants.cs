namespace CoinPayment_POC
{
    public static class ConfigurationConstants
    {
        public const string ipn_version = "1.0";
        public const string cmd = "_pay_auto";
        public const string ipn_type = "simple";
        public const string ipn_mode = "hmac";
        public const string allow_extra = "0";
        public const string currency = "USD";
        public const string success_url = "/CoinPayments/SuccessHandler?orderNumber=";
        public const string ipn_url = "/CoinPayments/IPNHandler";
        public const string cancel_url = "/CoinPayments/CancelOrder";
        public const string first_name = "first_name";
        public const string last_name = "last_name";
        public const string email = "email";
        public const string amount1 = "amount1";
        public const string subtotal = "subtotal";
        public const string status = "status";
        public const string status_text = "status_text";
    }
}
