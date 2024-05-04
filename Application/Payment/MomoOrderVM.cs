namespace Application.Payment
{
    public class MomoOrderVM
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public string extraData { get; set; }
        public long amount { get; set; }
        public long transId { get; set; }
        public string payType { get; set; }
        public int resultCode { get; set; }
        public List<dynamic> refundTrans { get; set; }
        public string message { get; set; }
        public long responseTime { get; set; }
        public long lastUpdated { get; set; }
        public string? signature { get; set; }
    }
}