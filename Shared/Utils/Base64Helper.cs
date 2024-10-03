using System.Data;
using System.Xml;

namespace Shared.Utils
{
    public static class Base64Helper
    {
        public static string EncodeDatatable(DataTable dataTable)
        {
            using (var memoryStream = new MemoryStream())
            {
                    dataTable.WriteXml(memoryStream);
               
                return Encode(memoryStream.ToArray());
            }
        }

        public static DataTable DecodeDatatable(string base64)
        {
            var dataTable = new DataTable();
            using (var memoryStream = new MemoryStream(Decode(base64)))
            {   
                    dataTable.ReadXml(memoryStream);
            }
            return dataTable;
        }
        public static string Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] Decode(string base64)
        {
            return Convert.FromBase64String(base64);
        }
    }
}
