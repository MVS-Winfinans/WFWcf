using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Text.RegularExpressions;
/// <summary>
/// Summary description for wfsh
/// </summary>
/// 
namespace wfws
{
    public class wfsh
    {
        public static string Left(string str, int length)
        {
            return str.Substring(0, Math.Min(str.Length, length));
        }
        public static int GetWeekNumber(DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
        public static string GetWeekDay(DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            ciCurr.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
            DayOfWeek WeekDay = ciCurr.Calendar.GetDayOfWeek(dtPassed);
            //int Aux = Convert.ToInt32(ciCurr.Calendar.GetDayOfWeek(dtPassed));
            // int Aux = Convert.ToInt32(WeekDay);
            // if (Aux == 8) Aux = 7;
            string weekday = WeekDay.ToString();
            //return Aux;
            return weekday;
        }
        public static DateTime ToSqlDateTime(string MyDate)
        {
            //'Returns a valid SQL Date in a datetime
            //'Checks if is a legal date - and then if it is within permitted SQL datatime values
            //'Observe: An illegal datetime string results in Min SQL Date
            DateTime NewDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            if (DateTime.TryParse(MyDate, out NewDate))
            {
                if (NewDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue) NewDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                if (NewDate > (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue) NewDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;
            }
            return NewDate;
        }
        public static DateTime ToSqlDateTime(DateTime MyDate)
        {
            if (MyDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue) MyDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            if (MyDate > (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue) MyDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;
            return MyDate;
        }
        public static Boolean IsSqlDateTime(string MyDate)
        {
            Boolean is_date = false;
            DateTime NewDate;
            if (DateTime.TryParse(MyDate, out NewDate))
                if (NewDate > (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue && NewDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
                    is_date = true;
            return is_date;
        }
        public static Boolean IsSqlDateTime(DateTime MyDate)
        {
            Boolean is_date = false;
            if (MyDate > (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue && MyDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
                    is_date = true;
            return is_date;
        }
        static public decimal Cdec(string MyVal, string def = "0,0000")
        {
            decimal retval;
            if (!(decimal.TryParse(MyVal, out retval))) decimal.TryParse(def, out retval);
            return retval;
        }
        static public int Cint(string MyVal, int def = 0)
        {
            int retval;
            if (!(int.TryParse(MyVal, out retval))) retval = def;
            return retval;
        }
        static public string makeinitials(string strName)
        {
            Regex extractInitials = new Regex(@"\s*([^\s])[^\s]*\s*");
            string retval = extractInitials.Replace(strName, "$1").ToUpper();
            if ((retval.Length < 2) && (strName.Length > 2)) retval = strName.Substring(0, 3).ToUpper();
            return retval;
        }

    }
}