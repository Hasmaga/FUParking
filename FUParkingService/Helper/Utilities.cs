using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUParkingService.Helper
{
    public class Utilities
    {
        public static string FormatPlateNumber(string plateNumber)
        {
            return plateNumber.Length switch
            {
                8 => $"{plateNumber.Substring(0, 4)}-{plateNumber.Substring(4)}",
                9 => $"{plateNumber.Substring(0, 4)}-{plateNumber.Substring(4, 3)}.{plateNumber.Substring(7)}",
                10 => $"{plateNumber.Substring(0, 5)}-{plateNumber.Substring(5, 3)}.{plateNumber.Substring(8)}",
                _ => plateNumber
            };
        }

        public static string FormatMoney(int number)
        {
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            return number.ToString("#,###", new CultureInfo("vi-VN"));
        }

        public static string FormatDateTime(DateTime? dateTime) => dateTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
    }
}
