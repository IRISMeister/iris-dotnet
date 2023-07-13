using System;
using System.Diagnostics;
using System.Text;
using MathNet.Numerics.Statistics;
using System.Data.Odbc;

// Speed of binary inserts by insert ... select ...
// CREATE TABLE DWH.ODBC (SENT_AT TIMESTAMP,TOPIC VARCHAR(256),RECEIVED_AT TIMESTAMP, BINARY  VARBINARY(1200) )
namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            int repeatcount = 20000;
            int unioncount = 100;
            int cnt = 0;
            int j = 0;
            byte[] data = new byte[1000];
            for (int i = 0; i < 1000; i++) data[i] = (byte)(i % 256);

            double[] stats = new double[repeatcount];

            DateTime t;
            var sw = new Stopwatch();
            double ms = 0;
            double msttl = 0;

            sw.Reset();

            Console.WriteLine("Using ODBC provider");

            OdbcConnection OdbcConnect = new OdbcConnection("DSN=irisdatasource");

            OdbcConnect.Open();
            OdbcCommand cmdInsert = null;

            String tablename = "DWH.ODBC";

            var sqlInsert = new StringBuilder();


            sqlInsert.Append($"INSERT INTO {tablename} SELECT ?,?,?,? ");
            for (cnt = 1; cnt < unioncount; cnt++) sqlInsert.Append("UNION ALL SELECT ?,?,?,? ");
            //Console.WriteLine(sqlInsert);

            cmdInsert = new OdbcCommand(sqlInsert.ToString(), OdbcConnect);
            cmdInsert.Prepare();

            for (j = 0; j < repeatcount; j++)
            {
                sw.Restart();
                cmdInsert.Parameters.Clear();
                for (cnt = 0; cnt < unioncount * 4; cnt += 4)
                {
                    t = DateTime.Now;
                    cmdInsert.Parameters.Add($"@p{cnt}", System.Data.SqlDbType.Int).Value = t;
                    cmdInsert.Parameters.Add($"@p{cnt + 1}", System.Data.SqlDbType.VarChar).Value = "topic";
                    cmdInsert.Parameters.Add($"@p{cnt + 2}", System.Data.SqlDbType.Int).Value = t;
                    cmdInsert.Parameters.Add($"@p{cnt + 3}", System.Data.SqlDbType.VarBinary).Value = data;
                }
                cmdInsert.ExecuteNonQuery();

                sw.Stop();
                ms = sw.ElapsedMilliseconds;
                stats[j] = ms;
                msttl += ms;

            }
            Console.WriteLine(j + " times " + msttl + " msec");

            cmdInsert.Dispose();
            OdbcConnect.Close();
            OdbcConnect.Dispose();

            Console.WriteLine("実件数　　：{0}", repeatcount * unioncount);
            Console.WriteLine("回数　　　：{0}", stats.Length);
            Console.WriteLine("平均　　　：{0}", stats.Mean());
            Console.WriteLine("中央値　　：{0}", stats.Median());
            Console.WriteLine("分散　　　：{0}", stats.PopulationVariance());
            Console.WriteLine("母分散　　：{0}", stats.Variance());
            Console.WriteLine("標準偏差　：{0}", stats.PopulationStandardDeviation());
            Console.WriteLine("母標準偏差：{0}", stats.StandardDeviation());
            Console.WriteLine("最小　　　：{0}", stats.Minimum());
            Console.WriteLine("最大　　　：{0}", stats.Maximum());
            Console.WriteLine("Percentile(95%)：{0}", stats.Percentile(95));

        }
    }
}
