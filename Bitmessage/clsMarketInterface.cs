using System;
using System.Data.Odbc;
using System.Collections.Generic;

namespace SQL
{
    public static class MarketInterface
    {
        public static int Exec(string SQL)
        {
            return Exec(SQL, null);
        }

        public static int Exec(string SQL, object[] Args)
        {
            OdbcConnection OC = new OdbcConnection("DSN=BitMarket");

            OC.Open();

            var CMD = OC.CreateCommand();
            int result = 0;

            CMD.CommandText = SQL;
            if (Args != null)
            {
                for (int i = 0; i < Args.Length; i++)
                {
                    if (Args[i] is DateTime)
                    {
                        CMD.Parameters.Add(new OdbcParameter("@arg" + i, ((DateTime)Args[i]).ToString(Base.DT_FORMAT)));
                    }
                    else
                    {
                        CMD.Parameters.Add(new OdbcParameter("@arg" + i, Args[i] == null ? DBNull.Value : Args[i]));
                    }
                }
            }

            try
            {
                result = CMD.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                //OOPS
                Console.WriteLine("Error executing query: {1}\r\nError: {0}", ex.Message, CMD.CommandText);
            }
            CMD.Dispose();

            OC.Close();
            OC.Dispose();

            return result;
        }

        public static SQLRow[] ExecReader(string SQL)
        {
            return ExecReader(SQL, null);
        }

        public static SQLRow[] ExecReader(string SQL, object[] Args)
        {
            OdbcConnection OC = new OdbcConnection("DSN=BitMarket");

            OC.Open();

            var CMD = OC.CreateCommand();

            CMD.CommandText = SQL;
            if (Args != null)
            {
                for (int i = 0; i < Args.Length; i++)
                {
                    if (Args[i] is DateTime)
                    {
                        CMD.Parameters.Add(new OdbcParameter("@arg" + i, ((DateTime)Args[i]).ToString(Base.DT_FORMAT)));
                    }
                    else
                    {
                        CMD.Parameters.Add(new OdbcParameter("@arg" + i, Args[i] == null ? DBNull.Value : Args[i]));
                    }
                }
            }

            var reader = CMD.ExecuteReader();

            List<SQLRow> SR = new List<SQLRow>();

            while (reader.Read())
            {
                SQLRow SRR = new SQLRow();
                for (int j = 0; j < reader.FieldCount; j++)
                {
                    SRR.Add(reader.GetName(j), reader.IsDBNull(j) ? null : reader[j]);
                }
                SR.Add(SRR);
            }

            reader.Close();
            reader.Dispose();

            CMD.Dispose();

            OC.Close();
            OC.Dispose();

            return SR.ToArray();
        }
    }
}
