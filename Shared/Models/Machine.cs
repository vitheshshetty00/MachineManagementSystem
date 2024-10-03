namespace Shared.Models
{
    public class Machine
    {
        public string? MachineId { get; set; }
        public string? MachineName { get; set; }
        public string? IPAddress { get; set; }
        public int? PortNumber { get; set; }
        public byte[]? Image { get; set; }
        public DateTime? Timestamp { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
