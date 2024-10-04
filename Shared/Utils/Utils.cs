using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utils
{
    public class Utils
    {
        public static void printDataSet(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                printDataTable(table);
                Console.WriteLine();
            }
        }
        public static void printDataTable(DataTable table)
        {
            Console.WriteLine($"Table: {table.TableName}");
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Console.Write($"{col.ColumnName}: {row[col]} |");
                }
                Console.WriteLine();
            }
        }
    }
}
