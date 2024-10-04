using Microsoft.Data.SqlClient;

class DBServices
{
    static string connStr = "Data Source = AMIT-LT60\\SQLEXPRESS; Integrated Security = True; Encrypt = True; Trust Server Certificate = True; Initial Catalog = Dummy";
    static string connStrNoDB = "Data Source = AMIT-LT60\\SQLEXPRESS; Integrated Security = True; Encrypt = True; Trust Server Certificate = True; ";//change the connection strings
    public static void CreateDataBases()
    {
        using (SqlConnection conn = new SqlConnection(connStrNoDB))
        {
            SqlCommand cmdDB = new SqlCommand();
            cmdDB.CommandText = "IF NOT EXISTS (SELECT NAME FROM SYS.DATABASES WHERE NAME LIKE 'DUMMY') BEGIN CREATE DATABASE DUMMY; END";
            cmdDB.Connection = conn;
            conn.Open();
            cmdDB.ExecuteNonQuery();
            conn.Close();
        }
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmdTable = new SqlCommand();
            cmdTable.CommandText = "IF NOT EXISTS " +
                "(SELECT * FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_NAME = 'STUDENT' AND TABLE_SCHEMA = 'dbo')" +
                "BEGIN " +
                "CREATE TABLE STUDENT (" +
                "StudentID INT PRIMARY KEY IDENTITY(1,1)," +
                "StudentNAME VARCHAR(50)," +
                "EMAIL VARCHAR(50)," +
                "DOB DATE," +
                "ENROLLDATE DATETIME," +
                "IsFullTime BIT," +
                "paymentstatus varchar(10));" +
                "END";
            conn.Open();
            cmdTable.Connection = conn;
            cmdTable.ExecuteNonQuery();
            conn.Close();
        }
    }
}