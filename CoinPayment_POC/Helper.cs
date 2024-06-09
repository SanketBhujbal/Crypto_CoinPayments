using System.Security.Cryptography;
using System.Text;

namespace CoinPayment_POC
{
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
