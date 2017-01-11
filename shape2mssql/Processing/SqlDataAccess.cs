using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shape2mssql.Entities;

namespace shape2mssql.Processing
{
    public class SqlDataAccess
    {
        private const string GEOM_COLUMN_NAME = "geom";
        private string connectionString;
        private string keyFieldName;

        public SqlDataAccess(string connectionString, string keyFieldName)
        {
            this.connectionString = connectionString;
            this.keyFieldName = keyFieldName;
        }

        public void InsertGeoData(GeoObjectCollection geoObjects)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            string createTableCmd = createTableCmdText(geoObjects.TypeInfos, geoObjects.Name);
            string insertTableCmd = createInsertCmdText(geoObjects.TypeInfos, geoObjects.Name);            
            using(SqlCommand cmd = new SqlCommand(createTableCmd, conn))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                cmd.Transaction = tran;
                try
                {
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = insertTableCmd;
                    foreach (GeoObject obj in geoObjects.GeoObjects)
                    {
                        cmd.Parameters.Clear();
                        foreach (KeyValuePair<string, object> data in obj.SemanticInfo)
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + data.Key, data.Value ?? DBNull.Value ));
                        }
                        cmd.Parameters.Add(new SqlParameter("@" + GEOM_COLUMN_NAME, obj.Geography) 
                        { 
                            SqlDbType = System.Data.SqlDbType.Udt, 
                            UdtTypeName = "Geography", 
                            Value = obj.Geography 
                        });
                       cmd.ExecuteNonQuery();
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                conn.Close();
            }           
        }

        private string createTableCmdText(IDictionary<string, TypeInfo> typeInfos, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'").Append(tableName).Append("')");
            sb.Append("CREATE TABLE [").Append(tableName).Append("] (").Append(keyFieldName).Append(" int identity (1, 1) primary key, ");
            foreach (var pair in typeInfos)
            {
                sb.Append("[").Append(pair.Key).Append("] ");
                if (pair.Value.Type == typeof(string))
                {
                    sb.Append("nvarchar(").Append(getVarcharSize(pair.Value.Length)).Append(")");
                }
                else if (pair.Value.Type == typeof(int))
                {
                    sb.Append("int");
                }
                else if (pair.Value.Type == typeof(double))
                {
                    sb.Append("float");
                }
                else
                {
                    throw new NotSupportedException("Type is not supported");
                }
                sb.Append(" NULL, ");
            }
            sb.Append("[").Append(GEOM_COLUMN_NAME).Append("] geography NOT NULL)");

            return sb.ToString();
        }

        private string createInsertCmdText(IDictionary<string, TypeInfo> typeInfos, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbParams = new StringBuilder();

            sb.Append("INSERT INTO [").Append(tableName).Append("] (");
            foreach (var pair in typeInfos)
            {
                sb.Append("[").Append(pair.Key).Append("], ");
                sbParams.Append("@").Append(pair.Key).Append(", ");
            }
            sb.Append("[").Append(GEOM_COLUMN_NAME).Append("]) ");
            sb.Append("VALUES (").Append(sbParams).Append("@").Append(GEOM_COLUMN_NAME).Append(")");

            return sb.ToString();
        }

        private string getVarcharSize(int length)
        {
            if (length <= 50)
                return "50";
            else if (length <= 200)
                return "200";
            else if (length <= 500)
                return "500";
            else if (length <= 1000)
                return "1000";
            else if (length <= 4000)
                return "4000";
            else
                return "max";
        }
    }
}
