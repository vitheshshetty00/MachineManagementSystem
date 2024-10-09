using System;
using System.Data;
using System.Text;
using System.Xml;

namespace Shared.Utils
{
    public static class Base64Helper
    {
        public static string EncodeDatatable(DataTable dataTable)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    dataTable.WriteXml(xmlWriter, XmlWriteMode.WriteSchema);
                }
                string xmlString = stringWriter.ToString();
                byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlString);
                return Convert.ToBase64String(xmlBytes);
            }
        }

        public static string EncodeDataSet(DataSet dataSet)
        {
            using (var memoryStream = new MemoryStream())
            {
                dataSet.WriteXml(memoryStream);

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static DataTable DecodeDatatable(string base64String)
        {
            byte[] xmlBytes = Convert.FromBase64String(base64String);
            string xmlString = Encoding.UTF8.GetString(xmlBytes);

            using (var stringReader = new StringReader(xmlString))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    DataTable dataTable = new DataTable();
                    dataTable.ReadXml(xmlReader);
                    return dataTable;
                }
            }
        }
        public static DataSet DecodeDataSet(string base64)
        {
            var dataSet = new DataSet();
            byte[] decoded = Decode(base64);

            string xmlString = Encoding.UTF8.GetString(decoded);
            //Console.WriteLine(xmlString);
            using (var stringReader = new StringReader(xmlString))
            {
                dataSet.ReadXml(stringReader);
            }
            return dataSet;
        }
        public static string Encode(string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

        public static byte[] Decode(string base64)
        {

            return Convert.FromBase64String( base64);
        }
    }
}
