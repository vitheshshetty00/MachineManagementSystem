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
            using (var memoryStream = new MemoryStream())
            {
                dataTable.WriteXml(memoryStream);

                return Convert.ToBase64String(memoryStream.ToArray());
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

        public static DataTable DecodeDatatable(string base64)
        {
            var dataTable = new DataTable();
            byte[] decoded = Decode(base64);

            string xmlString = Encoding.UTF8.GetString(decoded);
            //Console.WriteLine(xmlString);
            using (var stringReader = new StringReader(xmlString))
            {
                dataTable.ReadXml(stringReader);
            }
            return dataTable;
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
