using System.Data;
using System.Text;
using System.Xml;

namespace Shared.Utils
{
    public static class Base64Helper
    {
        public static string EncodeDatatable(this DataTable dataTable)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
                {
                    dataTable.WriteXml(xmlWriter, XmlWriteMode.WriteSchema);
                }
                return Base64Helper.Encode(memoryStream.ToArray());
            }
        }

        public static DataTable DecodeDatatable(string base64)
        {
            var dataTable = new DataTable();
            using (var memoryStream = new MemoryStream(Base64Helper.Decode(base64)))
            {
                using (var xmlReader = XmlReader.Create(memoryStream))
                {
                    dataTable.ReadXml(xmlReader);
                }
            }
            return dataTable;
        }
        public static string Encode(byte[] data)
        {
            return System.Convert.ToBase64String(data);
        }

        public static byte[] Decode(string base64)
        {
            return System.Convert.FromBase64String(base64);
        }
    }
}
