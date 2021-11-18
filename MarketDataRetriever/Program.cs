using MarketDataRetriever.Common;
using MarketDataRetriever.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace MarketDataRetriever
{
    class Program
    {
        private static DateTime effectiveDocDate;
        private static DateTime startTime;
        private static DateTime stopTime;

        private static Timer timer;

        private static Thread threadMain;

        private static string snToken = string.Empty, inputString = string.Empty;
        private static bool setTokenFlag = false;
        private static bool exitFlag = false;

        static Program()
        {
            Utilities.ProcessStartOrEndTime("Main Service Method Started At : ", "");
            SetTimer();
            Utilities.ProcessStartOrEndTime("Main Service Method Ended At : ", "");
        }
        private static void SetTimer()
        {
            //This section validates the time span when the trade hour is occurs in any day.
            try
            {
                startTime = Utilities.GetServiceStartTime();
                stopTime = Utilities.GetServiceStopTime();
                if (DateTime.Now.TimeOfDay > startTime.TimeOfDay && DateTime.Now.TimeOfDay < stopTime.TimeOfDay)
                {
                    timer = new Timer();
                    timer.Interval = Utilities.GetIntervalTime();
                    timer.Elapsed += Timer_Elapsed;
                    timer.AutoReset = true;
                    timer.Start();

                    threadMain = new Thread(Start);
                    threadMain.Start();
                }
                else
                {
                    exitFlag = true;
                    Console.WriteLine("Live Data Retriever only works during trade hours. Please close this application.");
                }
            }
            catch (Exception exception)
            {
                Filehelper.LogWrite("Error occurred while initialization.\n", exception);
            }
        }
        private static void Start()
        {
            Filehelper.LogWrite("Service method started after initialization");
            timer.Enabled = false;
            ProcessData();
            timer.Enabled = true;
        }

        private static void ProcessData()
        {
            Filehelper.LogWrite("Excel File Downloading Started.\n");
            RetrieveLiveDataFromMottai();
            Filehelper.LogWrite("Downloading data from Mottai is completed.\n");
            GC.Collect();
            Console.WriteLine("The process will be restarted after 30 seconds. You can minimize this screen.");
        }
        private static void RetrieveLiveDataFromMottai()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            string uri = GetUri();
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string filePath = GetFilePath();
                    webClient.Headers.Add("User-Agent",
                        "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.3; WOW64; Trident/7.0; .NET4.0E; .NET4.0C; .NET CLR 3.5.30729; .NET CLR 2.0.50727; .NET CLR 3.0.30729)");

                    webClient.Headers.Add("Accept", "image/jpeg");
                    webClient.Headers.Add("Accept", "application/x-ms-application");
                    webClient.Headers.Add("Accept", "image/gif");
                    webClient.Headers.Add("Accept", "application/xaml+xml");
                    webClient.Headers.Add("Accept", "image/pjpeg");
                    webClient.Headers.Add("Accept", "application/x-ms-xbap");
                    webClient.Headers.Add("Accept", "*/*");

                    webClient.Headers.Add("Referer",
                        "http://10.250.128.10:81/users/right/admin/report.asp?institution=121&idList=32");
                    webClient.Headers.Add("Accept-Language", "en-US");
                    webClient.Headers.Add("DNT", "1");
                    webClient.Headers.Add("Accept-Encoding", "gzip");
                    webClient.Headers.Add("Accept-Encoding", "deflate");

                    //webClient.Headers.Add("Connection", "Keep-Alive");

                    /*for GSL_ADMIN*/
                    //webClient.Headers.Add(HttpRequestHeader.Cookie,
                    //    "stopUpdateRem=0; ASPSESSIONIDCQBTRSRQ=DJFBEAPCJIGAJJFHJCDDEMCK; userId=7662; role=21; userLogin=GSL%5Fadmin; user%5Fprefs=0; lastAccessDate=2017%2D06%2D18+13%3A36%3A19; user%5Fintra4=GSL%5Fadmin; user%5Fintra3=6; user%5Fintra=GSL%5Fadmin; user%5Fintra2=7662; sessionId=788798748; rightTool=Teams; ASPSESSIONIDSAASSTRQ=DJFBEAPCOJNIIMGNKFDHDIJH; IsRegulationInProcess=0; ASPSESSIONIDQABSSTTQ=LGFLDAPCFPFIKFOOAPAHKPGB; SubSellingInst=; SubSellingId=0; idg=3; ASPSESSIONIDSCCQTSQR=PANFGBPCCEIBDEKJGBPKAOHN; ASPSESSIONIDCSCSRTRQ=ACKPHOOCBNAGDLIAHKNBKPNE; user%5Fintr2=; ASPSESSIONIDAQASTRRR=CLFBEAPCELMIGMGKNDLDGDHO");

                    /*for GSL_CCD*/
                    webClient.Headers.Add(HttpRequestHeader.Cookie,
                  "stopUpdateRem=0; ASPSESSIONIDCCASTAST=LEDHKHAANEMPLKJDKILGGLMF; role=22; user%5Fprefs=0; user%5Fintra4=GSL%5FCCD; user%5Fintra3=6; user%5Fintra2=7663; user%5Fintra=GSL%5FCCD; ASPSESSIONIDAABTRCSS=IIPNCCAACHJICMIBOHPAECJE; userId=7663; userLogin=GSL%5FCCD; lastAccessDate=2018%2D01%2D18+16%3A42%3A38; sessionId=62912744; rightTool=Teams; ASPSESSIONIDCAASTASS=IIPNCCAAEJDEEPKNCGOEHPBI; IsRegulationInProcess=0; SubSellingInst=; SubSellingId=0; idg=3; ASPSESSIONIDSCCTASQD=BANNDEAAEJKADOMFPEJGEABC; ASPSESSIONIDACBTRDRS=APMLDEAAONEHGMNMACKOMCFG; ASPSESSIONIDACAQRBTT=JAPLBCAAKOCMPEAGHHDIDCFI; user%5Fintr2=; ASPSESSIONIDQABSDSQC=COIPPLDALFFGHKNBPJOGNGKB; ASPSESSIONIDCCBSRBSS=ALHBNLDAOOPKCIOJCLHOEGPI");

                    webClient.Headers.Add("Host", "10.250.128.10");
                    webClient.Headers.Add("Pragma", "no-cache");

                    try
                    {
                        Utilities.ProcessStartOrEndTime("Download Excel File Started At : ", "");
                        webClient.DownloadFile(uri, filePath + "tradeexcelfile.xls");
                        Utilities.ProcessStartOrEndTime("Download Excel File Ended At : ", "");
                    }
                    catch (WebException exception)
                    {
                        //string error = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd().ToString();
                        Console.WriteLine("The process is terminated. Please close this window and start the app again.");
                        Filehelper.LogWrite("Error occurred while downloading file", exception);
                    }

                }
            }
            catch (Exception exception)
            {
                Filehelper.LogWrite(exception.ToString());
                Console.WriteLine("Error occurred on WebClient.\nPlease contact Dimik IT.");
            }

            // Time Calculation \\
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Filehelper.LogWrite("Spend" + elapsedTime + " time to download excel file and create transaction list object");
        }
        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.TimeOfDay > startTime.TimeOfDay && DateTime.Now.TimeOfDay < stopTime.TimeOfDay)
            {
                threadMain = new Thread(Start);
                threadMain.Start();
            }
            else
            {
                exitFlag = true;
                Console.WriteLine("Live Data Retriever only works during trade hours. Please close this application.");
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Service Initialized.");

            while (true)
            {
                if (exitFlag)
                {
                    break;
                }

                System.Threading.Thread.Sleep(60000);
            }
        }

        private static string GetFilePath()
        {
            string filepath = Filehelper.GetExcelFolderPath();
            Filehelper.CheckAndCreatePath(filepath);
            return filepath;
        }
        private static string GetUri()
        {
            //string currentDate = DateTime.Now.Date.ToString("d", new CultureInfo("en-GB"));
            string currentDate = DateTime.UtcNow.Date.ToString("d", new CultureInfo("en-GB"));
            currentDate = currentDate.Replace("/", "%2F");

            //string snToken = "00c712e03bcc98a41f8ac521ba63329686addc0ebc94afb6a7fa9e6a5d6308a7";
            //string snToken = "c5f4b1399d770833c6cb968e9c12ae02a74d3b84b89f163e32dc4782bc71987c";

            if (setTokenFlag == false)
            {
                Console.Write("Input current sn and press enter:");
                inputString = Console.ReadLine();
                //inputString = snToken;
                while (string.IsNullOrEmpty(inputString) || !System.Text.RegularExpressions.Regex.IsMatch(inputString, @"^[a-zA-Z0-9]+$") || inputString.Length != 64)
                {
                    Console.Write("Wrong input. Try again.");
                    inputString = Console.ReadLine();
                }

                snToken = inputString;

                /*
                bool isValid = true;

                foreach (char c in snToken)
                {
                    if (!char.IsLetterOrDigit(c))
                    {
                        isValid = false;
                        break;
                    }
                }
                */

                setTokenFlag = true;
                Console.WriteLine("Working. Please wait.");
            }

            //return "http://10.250.128.10/CSTFWSrv/report/ReportHandler.ashx?sn=" + snToken + "&ReportId=32&format=EXCEL&requestor=GSL_admin%20(GSL_admin%20)&Source_id=-1&Source_name=&branchid_id=-1&branchid_name=&dealerid_id=-1&dealerid_name=&clientid_id=&clientid_name=&instrumentid_id=&instrumentid_name=&effectivedate_id=" + currentDate + "&effectivedate_name=" + currentDate + "&extendedParam=Source%2Cbranchid%2Cdealerid%2Cclientid%2Cinstrumentid%2Ceffectivedate%2Cinstitutionid";

            //for GSL_Admin
            //return "http://10.250.128.10/CSTFWSrv/report/ReportHandler.ashx?sn=" + snToken + "&ReportId=32&format=EXCEL&requestor=GSL_admin%20(GSL_admin%20)&Source_id=-1&Source_name=&branchid_id=-1&branchid_name=&dealerid_id=-1&dealerid_name=&clientid_id=&clientid_name=&instrumentid_id=&instrumentid_name=&effectivedate_id=" + currentDate + "&effectivedate_name=" + currentDate + "&extendedParam=Source%2Cbranchid%2Cdealerid%2Cclientid%2Cinstrumentid%2Ceffectivedate%2Cinstitutionid";

            //for GSL_CCD
            return "http://10.250.128.10/CSTFWSrv/report/ReportHandler.ashx?sn=" + snToken + "&ReportId=32&format=EXCEL&requestor=GSL_CCD%20(GSL_CCD%20)&Source_id=-1&Source_name=&branchid_id=-1&branchid_name=&dealerid_id=-1&dealerid_name=&clientid_id=&clientid_name=&instrumentid_id=&instrumentid_name=&effectivedate_id=" + currentDate + "&effectivedate_name=" + currentDate + "&extendedParam=Source%2Cbranchid%2Cdealerid%2Cclientid%2Cinstrumentid%2Ceffectivedate%2Cinstitutionid";

        }
    }
}

