using Application.Orders;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Payment
{
    public interface IPayment
    {
        Task<string> CreateOrder(CreateMomoOrderVM model);
        Task PostPaymentProcess(MomoIpnVM ipnData);
    }

    public class MomoService : IPayment
    {
        private const string API_ENDPOINT = "https://test-payment.momo.vn/v2/gateway/api";
        private const string partnerCode = "MOMO";
        private const string accessKey = "F8BBA842ECF85";
        private const string secretkey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        private const string orderInfo = "Payment through Momo";
        private const string requestType = "captureWallet";
        private const string extraData = "";
        private const string redirect = "/checkout/paymentresult";
        private const string ipn = "/checkout/receiveipn";
        private readonly string webAdressUrl;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderService _orderService;

        public MomoService(IHttpContextAccessor httpContextAccessor, IOrderService orderService)
        {
            _httpContextAccessor = httpContextAccessor;
            _orderService = orderService;
            webAdressUrl = "https://cleanly-powerful-lion.ngrok-free.app";
            //webAdressUrl = $"{_httpContextAccessor.HttpContext!.Request.Scheme}" +
            //                $"://{_httpContextAccessor.HttpContext.Request.Host}";
        }

        public async Task<string> CreateOrder(CreateMomoOrderVM model)
        {
            var requestId = Guid.NewGuid().ToString();
            string rawSignature = $"accessKey={accessKey}&" +
                                    $"amount={model.Amount}&" +
                                    $"extraData={extraData}&" +
                                    $"ipnUrl={webAdressUrl + ipn}&" +
                                    $"orderId={model.OrderId}&" +
                                    $"orderInfo={orderInfo}&" +
                                    $"partnerCode={partnerCode}&" +
                                    $"redirectUrl={webAdressUrl + redirect}&" +
                                    $"requestId={requestId}&" +
                                    $"requestType={requestType}";

            string signature = HashData(secretkey, rawSignature);

            string requestBody = $@"{{
                                        ""partnerCode"": ""{partnerCode}"",
                                        ""accessKey"": ""{accessKey}"",
                                        ""requestId"": ""{requestId}"",
                                        ""amount"": {model.Amount},
                                        ""orderId"": ""{model.OrderId}"",
                                        ""orderInfo"": ""{orderInfo}"",
                                        ""redirectUrl"": ""{webAdressUrl + redirect}"",
                                        ""ipnUrl"": ""{webAdressUrl + ipn}"",
                                        ""extraData"": ""{extraData}"",
                                        ""requestType"": ""{requestType}"",
                                        ""signature"": ""{signature}"",
                                        ""lang"": ""en"",
                                        ""items"": {JsonSerializer.Serialize(model.items)},
                                        ""userInfo"": {JsonSerializer.Serialize(model.userInfo)}
                                    }}";

            using var client = new HttpClient();
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{API_ENDPOINT}/create", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Create order failed");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var momoOrder = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return momoOrder.GetProperty("payUrl").GetString()!;
        }

        private async Task<MomoOrderVM> QueryOrder(string orderId)
        {
            var requestId = Guid.NewGuid().ToString();
            string rawSignature = $"accessKey={accessKey}" +
                                    $"&orderId={orderId}" +
                                    $"&partnerCode={partnerCode}" +
                                    $"&requestId={requestId}";
            var signature = HashData(secretkey, rawSignature);

            string requestBody = $@"{{
                ""partnerCode"": ""{partnerCode}"",
                ""accessKey"": ""{accessKey}"",
                ""requestId"": ""{requestId}"",
                ""orderId"": ""{orderId}"",
                ""signature"": ""{signature}"",
                ""lang"": ""en""
            }}";

            using var client = new HttpClient();
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{API_ENDPOINT}/query", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Query order failed");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("data from query:", responseContent);
            return JsonSerializer.Deserialize<MomoOrderVM>(responseContent)!;
        }

        public async Task PostPaymentProcess(MomoIpnVM ipnData)
        {
            try
            {
                var exist = await _orderService.IsExsit(Guid.Parse(ipnData.OrderId));
                Console.WriteLine("exist:", exist);
                if (!exist) throw new Exception("Order not found");
                var queryOrder = await QueryOrder(ipnData.OrderId);
                Console.WriteLine("ammount:",queryOrder.amount.ToString(), ipnData.Amount.ToString());
                if (queryOrder.amount != ipnData.Amount) throw new Exception("Amount mismatch");
                if(queryOrder.resultCode != 0) throw new Exception("Payment failed");
                var orderUpdate = new OrderUpdateViewModel
                {
                    OrderId = Guid.Parse(ipnData.OrderId),
                    Status = EntityStatus.Completed
                };
                await _orderService.UpdateOrder(orderUpdate);
            }
            catch(Exception)
            {
                return;
            }         
        }

        private static string HashData(string secretkey, string data)
        {
            string signature;
            using (HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secretkey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            return signature;
        }
    }
}
