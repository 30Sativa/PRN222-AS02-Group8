using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Implement.Helpers;
using OnlineLearningPlatform.Services.Interface;
using System.Text.Json;

namespace OnlineLearningPlatform.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;

        public PaymentService(IConfiguration configuration, IOrderService orderService)
        {
            _configuration = configuration;
            _orderService = orderService;
        }

        public string CreateVnPayPaymentUrl(Order order, HttpContext context)
        {
            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? "");
            
            // Số tiền VNPAY yêu cầu nhân 100
            int finalAmount = (int)(order.TotalAmount * 100);
            vnpay.AddRequestData("vnp_Amount", finalAmount.ToString());

            vnpay.AddRequestData("vnp_CreateDate", order.CreatedAt.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {order.OrderId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            
            string returnUrl = _configuration["Vnpay:ReturnUrl"] ?? "https://localhost:7088/Payment/VnPayCallback";
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString()); 

            var paymentUrl = vnpay.CreateRequestUrl(
                _configuration["Vnpay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", 
                _configuration["Vnpay:HashSecret"] ?? "");

            return paymentUrl;
        }

        public async Task<bool> ProcessVnPayCallbackAsync(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash.ToString(), _configuration["Vnpay:HashSecret"] ?? "");
            
            if (int.TryParse(vnp_orderIdStr, out int orderId))
            {
                // Serialize collection to JSON for saving response raw
                var responseDict = collections.ToDictionary(k => k.Key, v => v.Value.ToString());
                var gatewayResponse = JsonSerializer.Serialize(responseDict);

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        // Success -> Complete order -> Auto Enroll
                        return await _orderService.CompleteOrderAsync(orderId, vnp_TransactionId, gatewayResponse);
                    }
                    else
                    {
                        // Failed
                        return await _orderService.FailOrderAsync(orderId, gatewayResponse);
                    }
                }
                else
                {
                    // Invalid signature
                    await _orderService.FailOrderAsync(orderId, "Invalid signature");
                    return false;
                }
            }
            return false;
        }
    }
}
