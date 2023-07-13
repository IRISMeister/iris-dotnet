#define MAINCASE
//#define CASE2
//#define CASE3
using System;
using InterSystems.Data.IRISClient;
using System.Threading;
using MathNet.Numerics.Statistics;
using System.Text;

namespace ADO
{
    class Program2
    {
        static void Main(string[] args)
        {

            String host = "localhost";
            String port = "1972";
            String username = "_SYSTEM";
            String password = "SYS";
            String Namespace = "USER";

            String errstr;
            int loopcnt = 5000;
            int sleeptime = 10;

            if (args.Length >= 1) sleeptime = Int32.Parse(args[0]);
            if (args.Length >= 2) host = args[1];
            if (args.Length >= 3) port = args[2];
            if (args.Length >= 4) loopcnt = Int32.Parse(args[3]);

            double[] data = new double[loopcnt];

            String ConnectionString = "Server = " + host
                + "; Port = " + port + "; Namespace = " + Namespace
                + "; Password = " + password + "; User ID = " + username + "; SharedMemory=false;pooling=true";
            IRISConnection IRISConnect = new IRISConnection();
            IRISConnect.ConnectionString = ConnectionString;

            IRISConnect.Open();

            String tablename = "";

            String sqlStatement = "DROP TABLE TestTable4";
            String sqlStatement3 = "select ts,resp,hr from TestTable";

            var sqlStatement4 = new StringBuilder();
            tablename = "TestTable4"; sqlStatement4.AppendLine($"CREATE TABLE {tablename} (ts TIMESTAMP, resp LONGVARBINARY, hr LONGVARBINARY)");

            IRISCommand cmd = new IRISCommand(sqlStatement, IRISConnect);
            IRISCommand cmd4 = new IRISCommand(sqlStatement4.ToString(), IRISConnect);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { errstr=e.ToString(); }
            cmd4.ExecuteNonQuery();

            cmd.Dispose();
            cmd4.Dispose();
            IRISConnect.Close();


#if MAINCASE
            // This case makes async calls to access IRIS.
            MainJob mainJob = new(ConnectionString, sleeptime);
            for (int r = 0; r < loopcnt; r++)
            {
                // Too few sleeptime creates lots of IRIS processes via connection pool mechanism.
                if (sleeptime > 0)
                {
                    mainJob.Exec(data, r);
                    Thread.Sleep(sleeptime);
                }
                else
                {
                    mainJob.ExecSync(data, r);
                }
                //GC.Collect();
            }

            //wait last job to finish
            if (sleeptime > 0) Thread.Sleep(sleeptime);
#endif

            Console.WriteLine("");

            IRISConnect.Open();
            IRISCommand cmd3 = new IRISCommand(sqlStatement3.ToString(), IRISConnect);
            IRISDataReader Reader = cmd3.ExecuteReader();
            Reader.Read();
            long reccnt = Reader.GetInt64(0);

            Console.WriteLine("IRIS Server Version:"+IRISConnect.ServerVersion);
            Reader.Close();
            IRISConnect.Close();

#if MAINCASE
            Console.WriteLine("MainJob() used.");
#endif

            Console.WriteLine("Sleep time in ms:" + sleeptime);
            Console.WriteLine(ConnectionString);


            Console.WriteLine("実件数　　：{0}", reccnt);
            Console.WriteLine("件数　　　：{0}", data.Length);
            Console.WriteLine("平均　　　：{0}", data.Mean());
            Console.WriteLine("中央値　　：{0}", data.Median());
            Console.WriteLine("分散　　　：{0}", data.PopulationVariance());
            Console.WriteLine("母分散　　：{0}", data.Variance());
            Console.WriteLine("標準偏差　：{0}", data.PopulationStandardDeviation());
            Console.WriteLine("母標準偏差：{0}", data.StandardDeviation());
            Console.WriteLine("最小　　　：{0}", data.Minimum());
            Console.WriteLine("最大　　　：{0}", data.Maximum());
            Console.WriteLine("Percentile(95%)：{0}", data.Percentile(95));

        }

    }
}
