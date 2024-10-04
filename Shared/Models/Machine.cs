namespace Shared.Models
{
    public class Machine
    {
        public string? MachineId { get; set; }
        public string? MachineName { get; set; }
        public string? IPAddress { get; set; }
        public string? PortNumber { get; set; }
        public byte[]? Image { get; set; }
        public DateTime? Timestamp { get; set; }


        public static List<Machine> DataforInitization()
        {
            string imagePath = @"C:\Users\Devteam\Downloads\celebrating-popcorn.png";
            byte[] imageBytes;
            try
            {
                imageBytes = File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading image file: {ex.Message}");
                imageBytes = null;
            }

            // List of IP addresses and ports mapped to machine names
            var machines = new List<(string machineName, string ip, string port)>
        {
            ("Machine1", "172.36.10.37", "5001"),
            ("Machine2", "172.36.10.37", "5002"),
            ("Machine3", "172.36.10.135", "21"),
            ("Machine4", "172.36.0.111", "22"),
            ("Machine5", "172.36.0.111", "5040"),
            ("Machine6", "172.36.10.135", "80"),
            ("Machine7", "172.36.0.111", "80"),
            ("Machine8", "172.36.10.250", "80"),
            ("Machine9", "172.36.10.240", "80"),
            ("Machine10", "172.36.10.238", "80"),
            ("Machine11", "172.36.10.240", "102"),
            ("Machine12", "172.36.10.238", "102"),
            ("Machine13", "172.36.10.37", "102"),
            ("Machine14", "172.36.10.173", "102"),
            ("Machine15", "172.36.10.36", "102"),
            ("Machine16", "172.36.0.111", "135"),
            ("Machine17", "172.36.0.111", "139"),
            ("Machine18", "172.36.0.111", "7680"),
            ("Machine19", "172.36.0.111", "5432"),
            ("Machine20", "172.36.10.240", "443"),
            ("Machine21", "172.36.0.111", "445"),
            ("Machine22", "172.36.10.238", "443"),
            ("Machine23", "172.36.10.238", "502"),
            ("Machine24", "172.36.10.135", "502"),
            ("Machine25", "172.36.10.135", "503"),
            ("Machine26", "172.36.10.135", "504"),
            ("Machine27", "172.36.0.111", "8000"),
            ("Machine28", "172.36.10.36", "502"),
            ("Machine29", "172.36.0.111", "8051"),
            ("Machine30", "172.36.0.111", "8069"),
            ("Machine31", "172.36.0.111", "8070"),
            ("Machine32", "172.36.0.111", "8083"),
            ("Machine33", "172.36.0.111", "8086"),
            ("Machine34", "172.36.0.111", "8088"),
            ("Machine35", "172.36.0.111", "8089"),
            ("Machine36", "172.36.0.111", "8090"),
            ("Machine37", "172.36.0.111", "8092"),
            ("Machine38", "172.36.0.111", "8095"),
            ("Machine39", "172.36.0.111", "8097"),
            ("Machine40", "172.36.10.194", "8193"),
            ("Machine41", "172.36.0.111", "3223"),
            ("Machine42", "172.36.0.111", "3389"),
            ("Machine43", "172.36.10.135", "5900"),
            ("Machine44", "172.36.10.135", "6000"),
            ("Machine45", "172.36.0.111", "8524"),
            ("Machine46", "172.36.10.250", "1050"),
            ("Machine47", "172.36.0.111", "1433"),
            ("Machine48", "172.36.0.111", "1434"),
            ("Machine49", "172.36.0.111", "1435"),
            ("Machine50", "172.36.0.111", "1436"),
            ("Machine51", "172.36.0.111", "9011"),
            ("Machine52", "172.36.0.111", "9012"),
            ("Machine53", "172.36.0.111", "9022"),
            ("Machine54", "172.36.0.111", "9082"),
            ("Machine55", "172.36.0.111", "9090"),
            ("Machine56", "172.36.0.111", "9092"),
            ("Machine57", "172.36.0.111", "9097"),
            ("Machine58", "172.36.0.111", "9099"),
            ("Machine59", "172.36.0.111", "1883"),
            ("Machine60", "172.36.10.135", "2000"),
            ("Machine61", "172.36.0.111", "7070"),
            ("Machine62", "172.36.0.111", "7072"),
            ("Machine63", "172.36.0.111", "7088"),
            ("Machine64", "172.36.0.111", "7090"),
            ("Machine65", "172.36.0.111", "7091")
        };

            return machines.Select(m => new Machine
            {
                MachineName = m.machineName,
                IPAddress = m.ip,
                PortNumber = m.port,
                Image = imageBytes,
            }).ToList();
        }
    }



}
