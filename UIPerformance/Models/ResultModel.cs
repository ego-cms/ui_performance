namespace UIPerformance.Models
{
    public class ResultModel
    {
        public ViewType Type { get; set; }
        public int ElapsedTime { get; set; }
        public decimal ElapsedMemory { get; set; }
        public decimal ElapsedJavaMemory { get; set; }
    }
}
