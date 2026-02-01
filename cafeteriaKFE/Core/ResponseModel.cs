namespace cafeteriaKFE.Core
{
    public class ResponseModel
    {
        public bool success { get; set; } = false;
        public string message { get; set; } = string.Empty;
        public int totalItems { get; set; } = 0;
        public object data { get; set; } = new object();
        public string responseTime { get; set; } = "0s";
    }
}
