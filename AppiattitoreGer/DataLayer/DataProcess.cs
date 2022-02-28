using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace WpfApp1.DataLayer
{
    internal class DataProcess
    {
        internal static List<string> GetDatabases(string connectionString)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var output = connection.Query<string>("SELECT name FROM master.dbo.sysdatabases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') ORDER BY name");
                return output.ToList();
            }
        }
        internal static List<string> GetTables(string connectionString)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var output = connection.Query<string>("SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME like 'H%' ORDER BY TABLE_SCHEMA + '.' + TABLE_NAME");
                return output.ToList();
            }
        }        
        internal static List<string> GetDescriptionTables(string connectionString)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var output = connection.Query<string>("SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME like 'D%' ORDER BY TABLE_SCHEMA + '.' + TABLE_NAME");
                return output.ToList();
            }
        }
        internal static List<string> GetFields(string connectionString, string schema_table)
        {
            string schema = schema_table.Split('.')[0];
            string table = schema_table.Split('.')[1];

            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var output = connection.Query<string>($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{table}' ORDER BY ORDINAL_POSITION");
                return output.ToList();
            }
        }

        internal static DataTable GeneratePreview(string connectionString, string query)
        {
            DataTable dt = new DataTable();
            query = query.Replace("SELECT", "SELECT TOP 5");
            using (var da = new SqlDataAdapter(query, connectionString))
            {
                da.Fill(dt);
            }
            return dt;
        }

        //funzione generica, non va bene per 4A. Non utilizzato
        internal static string GenerateQuery(string connectionString, string field, string description, string fatherfield, string fatherdescription, string table)
        {
            int counter = 1;
            int level = 0;

            string select_query = @$"SELECT 
                a0.{field} AS {field}_0
                ,a0.{fatherfield} AS {field}_1
                ";
            string from_query = $"FROM {table} as a{level}\n";
            string query = string.Empty;
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                while (counter > 0)
                {
                    level++;
                    select_query += $",a{level}.{fatherfield} AS {field}_{level + 1}\n";
                    from_query += $"LEFT JOIN {table} a{level} on a{level - 1}.{fatherfield} = a{level}.{field}\n";
                    query = "SELECT COUNT (*) FROM (" + select_query + from_query + $") b WHERE b.{field}_{level + 1} IS NOT NULL";
                    counter = connection.Query<int>(query).ToList()[0];
                    if (level > 20)
                    {
                        counter = 0;
                    }
                }
            }
            level--;

            if (field == description)
            {
                select_query = @$"SELECT 
a0.{field} AS {field}_0
,COALESCE(a0.{fatherfield}, a0.{field}) as {field}_1
";
            }
            else
            {
                select_query = @$"SELECT 
a0.{field} AS {field}_0, a0.{description} AS {description}_0 
,COALESCE(a0.{fatherfield}, a0.{field}) as {field}_1, COALESCE(a0.{fatherdescription}, a0.{description}) as {description}_1
";
            }
            from_query = $"FROM {table} as a0\n";

            string coalesce_code;
            string coalesce_des;

            for (int i = 1; i <= level; i++)
            {
                coalesce_code = "COALESCE(";
                coalesce_des = "COALESCE(";
                for (int j = i; j > 0; j--)
                {
                    coalesce_code += $"a{j}.{fatherfield},";
                    coalesce_des += $"a{j}.{fatherdescription},";
                }
                coalesce_code = coalesce_code.TrimEnd(',');
                coalesce_des = coalesce_des.TrimEnd(',');
                coalesce_code += $",a0.{fatherfield}, a0.{field}) AS {field}_{i + 1}";
                coalesce_des += $",a0.{fatherdescription}, a0.{description}) AS {description}_{i + 1}";


                select_query += $",{coalesce_code}, {coalesce_des}\n";
                from_query += $"LEFT JOIN {table} a{i} on a{i - 1}.{fatherfield} = a{i}.{field}\n";
            }

            query = select_query + from_query;
            return query;
        }        
        internal static string GenerateQuery(string connectionString, string table, string des_table)
        {
            int counter = 1;
            int level = 0;

            string select_query = @$"SELECT 
                a0.IDElement AS ID_0
                ,a0.IDParent AS ID_1
                ";
            string from_query = $"FROM {table} as a{level}\n";
            string query = string.Empty;
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                while (counter > 0)
                {
                    level++;
                    select_query += $",a{level}.IDParent AS ID_{level + 1}\n";
                    from_query += $"LEFT JOIN {table} a{level} on a{level - 1}.IDParent = a{level}.IDElement\n";
                    query = "SELECT COUNT (*) FROM (" + select_query + from_query + $") b WHERE b.ID_{level + 1} IS NOT NULL";
                    counter = connection.Query<int>(query).ToList()[0];
                    if (level > 20)
                    {
                        counter = 0;
                    }
                }
            }
            level--;
            string coalesce = "COALESCE(";
            for (int i = level; i >= 0; i--)
            {
                coalesce += $"d{i}.id,";
            }
            coalesce = coalesce.Trim(',');
            coalesce += $") AS ID_{level}";
            select_query = @$"SELECT 
{coalesce}, d0.Codice AS COD_0, d0.Descrizione AS DES_0
";

            from_query = $"FROM {table} as a0\nLEFT JOIN {des_table} d0 on a0.IDElement = d0.ID\n";

            string coalesce_code;
            string coalesce_des;
            
            for (int i = 1; i <= level; i++)
            {
                coalesce_code = "COALESCE(";
                coalesce_des = "COALESCE(";
                for (int j = i; j >= 0; j--)
                {
                    coalesce_code += $"d{j}.Codice,";
                    coalesce_des += $"d{j}.Descrizione,";
                }

                coalesce_code = coalesce_code.TrimEnd(',');
                coalesce_des = coalesce_des.TrimEnd(',');

                coalesce_code += $") AS COD_{i}";
                coalesce_des += $") AS DES_{i}";

                coalesce = coalesce_code + "," + coalesce_des;
                select_query += "," + coalesce + "\n";
                from_query += $"LEFT JOIN {table} a{i} on a{i-1}.IDElement=a{i}.IDParent\nLEFT JOIN {des_table} d{i} on d{i}.ID = a{i}.IDElement\n";
            }

            query = select_query + from_query + "WHERE (A0.IDParent=0)";
            return query;
        }
    }
}
