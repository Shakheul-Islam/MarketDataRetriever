using MarketDataRetriever.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataRetriever.Common
{
    public static class Utilities
    {
        public static DateTime GetServiceStartTime()
        {
            string timeString = System.Configuration.ConfigurationManager.AppSettings["serviceStartTime"];
            return Convert.ToDateTime(timeString);
        }

        public static DateTime GetServiceStopTime()
        {
            string timeString = System.Configuration.ConfigurationManager.AppSettings["serviceStopTime"];
            return Convert.ToDateTime(timeString);
        }

        public static double GetIntervalTime()
        {
            return Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["intervalTime"]);
        }

        public static string GetDocCode()
        {
            return System.Configuration.ConfigurationManager.AppSettings["docCode"];
        }

        public static int GetCompanyID()
        {
            return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["companyID"]);
        }

        public static int GetUserID()
        {
            return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["userID"]);
        }

        public static string GetExchange()
        {
            return System.Configuration.ConfigurationManager.AppSettings["exchange"];
        }

        public static void ProcessStartOrEndTime(string startMessage = "", string endMessage = "")
        {
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            string sfdt = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            Filehelper.LogWrite(startMessage + sfdt + endMessage);
        }
    }
}
