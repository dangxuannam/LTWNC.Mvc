using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CheapDeal.WebApp.Services
{

    public class PaymentService
    {

        private static string MoMo_PartnerCode = ConfigurationManager.AppSettings["MoMo.PartnerCode"] ?? "MOMOBKUN20180529";
        private static string MoMo_AccessKey = ConfigurationManager.AppSettings["MoMo.AccessKey"] ?? "klm05TvNBzhg7h7j";
        private static string MoMo_SecretKey = ConfigurationManager.AppSettings["MoMo.SecretKey"] ?? "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
        private static string MoMo_Endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
        private static string MoMo_ReturnUrl = ConfigurationManager.AppSettings["MoMo.ReturnUrl"] ?? "http://localhost:44341/Checkout/MomoCallback";
        private static string MoMo_NotifyUrl = ConfigurationManager.AppSettings["MoMo.NotifyUrl"] ?? "http://localhost:44341/Checkout/MomoNotify";

        private static string VNPay_TmnCode = ConfigurationManager.AppSettings["VNPay.TmnCode"] ?? "DEMOV210";
        private static string VNPay_HashSecret = ConfigurationManager.AppSettings["VNPay.HashSecret"] ?? "RAOEXHYVSDDIIENYWSLDIIZTANXUXZFJ";
        private static string VNPay_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private static string VNPay_ReturnUrl = ConfigurationManager.AppSettings["VNPay.ReturnUrl"] ?? "http://localhost:44341/Checkout/VNPayCallback";

        public static string CreateMoMoPaymentUrl(int orderId, decimal amount, string orderInfo)
        {
            try
            {
                string requestId = Guid.NewGuid().ToString();
                string extraData = "";
                string requestType = "captureWallet";

                // Tạo raw signature
                string rawHash = $"accessKey={MoMo_AccessKey}" +
                                $"&amount={amount}" +
                                $"&extraData={extraData}" +
                                $"&ipnUrl={MoMo_NotifyUrl}" +
                                $"&orderId={orderId}" +
                                $"&orderInfo={orderInfo}" +
                                $"&partnerCode={MoMo_PartnerCode}" +
                                $"&redirectUrl={MoMo_ReturnUrl}" +
                                $"&requestId={requestId}" +
                                $"&requestType={requestType}";

                string signature = ComputeHmacSha256(rawHash, MoMo_SecretKey);

                // Tạo request body
                var requestData = new
                {
                    partnerCode = MoMo_PartnerCode,
                    partnerName = "CheapDeal",
                    storeId = "CheapDealStore",
                    requestId = requestId,
                    amount = amount.ToString("0"),
                    orderId = orderId.ToString(),
                    orderInfo = orderInfo,
                    redirectUrl = MoMo_ReturnUrl,
                    ipnUrl = MoMo_NotifyUrl,
                    requestType = requestType,
                    extraData = extraData,
                    lang = "vi",
                    signature = signature
                };

                string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

                // Gửi request đến MoMo
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("Content-Type", "application/json");
                    string response = client.UploadString(MoMo_Endpoint, jsonRequest);

                    dynamic responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                    if (responseObj.resultCode == 0)
                    {
                        return responseObj.payUrl;
                    }
                    else
                    {
                        throw new Exception($"MoMo Error: {responseObj.message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tạo thanh toán MoMo: {ex.Message}");
            }
        }
        public static bool VerifyMoMoCallback(Dictionary<string, string> parameters)
        {
            try
            {
                string signature = parameters.ContainsKey("signature") ? parameters["signature"] : "";

                string rawHash = $"accessKey={parameters["accessKey"]}" +
                                $"&amount={parameters["amount"]}" +
                                $"&extraData={parameters["extraData"]}" +
                                $"&message={parameters["message"]}" +
                                $"&orderId={parameters["orderId"]}" +
                                $"&orderInfo={parameters["orderInfo"]}" +
                                $"&orderType={parameters["orderType"]}" +
                                $"&partnerCode={parameters["partnerCode"]}" +
                                $"&payType={parameters["payType"]}" +
                                $"&requestId={parameters["requestId"]}" +
                                $"&responseTime={parameters["responseTime"]}" +
                                $"&resultCode={parameters["resultCode"]}" +
                                $"&transId={parameters["transId"]}";

                string computedSignature = ComputeHmacSha256(rawHash, MoMo_SecretKey);

                return signature.Equals(computedSignature);
            }
            catch
            {
                return false;
            }
        }

        public static string CreateVNPayPaymentUrl(int orderId, decimal amount, string orderInfo, string ipAddress)
        {
            try
            {
                var vnpay = new VnPayLibrary();

                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", VNPay_TmnCode);
                vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString("0")); // VNPay yêu cầu * 100
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", ipAddress);
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", VNPay_ReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(VNPay_Url, VNPay_HashSecret);
                return paymentUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tạo thanh toán VNPay: {ex.Message}");
            }
        }

        public static bool VerifyVNPayCallback(Dictionary<string, string> parameters)
        {
            try
            {
                var vnpay = new VnPayLibrary();

                foreach (var param in parameters)
                {
                    if (!string.IsNullOrEmpty(param.Value) && param.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(param.Key, param.Value);
                    }
                }

                string vnp_SecureHash = parameters.ContainsKey("vnp_SecureHash") ? parameters["vnp_SecureHash"] : "";

                return vnpay.ValidateSignature(vnp_SecureHash, VNPay_HashSecret);
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeHmacSha256(string message, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }

    public class VnPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.ContainsKey(key) ? _responseData[key] : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            queryString = queryString.TrimEnd('&');

            string signData = queryString;
            string vnp_SecureHash = ComputeHmacSha256(signData, vnp_HashSecret);

            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHashType" && kv.Key != "vnp_SecureHash")
                {
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }

            string signData = data.ToString().TrimEnd('&');
            string myChecksum = ComputeHmacSha256(signData, secretKey);

            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string ComputeHmacSha256(string message, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}