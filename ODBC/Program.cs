﻿using System;
using System.Data.Odbc;
using System.Diagnostics;
using System.Text;
using MathNet.Numerics.Statistics;

// Speed of binary inserts by insert ... select ...
// CREATE TABLE DWH.ODBC (SENT_AT TIMESTAMP,TOPIC VARCHAR(256),RECEIVED_AT TIMESTAMP, BINARY  VARBINARY(1200) )
namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            String value="abcde";
            int repeatcount = 20000;
            int unioncount = 100;
            int cnt = 0;
            int j = 0;
            byte[] data = new byte[1000];
            for (int i = 0; i < 1000; i++) data[i] = (byte)(i % 256);
            //byte[] bytesvalue;

            double[] stats = new double[repeatcount];

            DateTime t;
            var sw = new Stopwatch();
            var swElasped = new Stopwatch();
            double ms = 0;
            double msttl = 0;

            sw.Reset();

            Console.WriteLine("Using ODBC.NET");

            OdbcConnection connection = new OdbcConnection("DSN=irisdatasource");

            connection.Open();
            OdbcCommand cmdInsert = null;

            String tablename = "DWH.ODBC";

            var sqlInsert = new StringBuilder();


            sqlInsert.Append($"INSERT INTO {tablename} SELECT ?,?,?,? ");
            for (cnt = 1; cnt < unioncount; cnt++) sqlInsert.Append("UNION ALL SELECT ?,?,?,? ");
            //Console.WriteLine(sqlInsert);

            cmdInsert = new OdbcCommand(sqlInsert.ToString(), connection);
            cmdInsert.Prepare();

            swElasped.Start();

            for (j = 0; j < repeatcount; j++)
            {
//                sw.Restart();
                cmdInsert.Parameters.Clear();
                for (cnt = 0; cnt < unioncount * 4; cnt += 4)
                {
                    t = DateTime.Now;
                    cmdInsert.Parameters.AddWithValue($"@p{cnt}",t);
                    //bytesvalue = Encoding.ASCII.GetBytes(value);
                    cmdInsert.Parameters.AddWithValue($"@p{cnt + 1}",value);
                    cmdInsert.Parameters.AddWithValue($"@p{cnt + 2}",t);
                    cmdInsert.Parameters.AddWithValue($"@p{cnt + 3}",data);
                }
                sw.Restart();
                cmdInsert.ExecuteNonQuery();

                sw.Stop();
                ms = sw.ElapsedMilliseconds;
                stats[j] = ms;
                msttl += ms;

            }
            Console.WriteLine(j + " executions in " + msttl + " msec");

            swElasped.Stop();
            Console.WriteLine("Elapded time " + swElasped.ElapsedMilliseconds + " msec");

            cmdInsert.Dispose();
            connection.Close();
            connection.Dispose();

            Console.WriteLine("Count:実件数      :{0}", repeatcount * unioncount);
            Console.WriteLine("Execute Count:回数:{0}", stats.Length);
            Console.WriteLine("Mean:平均         :{0}", stats.Mean());
            Console.WriteLine("Median:中央値     :{0}", stats.Median());
            Console.WriteLine("PopVar:分散       :{0}", stats.PopulationVariance());
            Console.WriteLine("Var:母分散        :{0}", stats.Variance());
            Console.WriteLine("PopStdDev:標準偏差:{0}", stats.PopulationStandardDeviation());
            Console.WriteLine("StdDev:母標準偏差 :{0}", stats.StandardDeviation());
            Console.WriteLine("Min:最小          :{0}", stats.Minimum());
            Console.WriteLine("Max:最大          :{0}", stats.Maximum());
            Console.WriteLine("Percentile(95%)   :{0}", stats.Percentile(95));
        }
    }
}
