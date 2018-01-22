using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialLogger
{
    class Utils
    {
        public static int DurationNameToSeconds(string duration) {
            int retval = 0;
            switch (duration)
            {
                case "Hourly":
                    retval = 60 * 60;
                    break;
                case "Daily":
                    retval = 60 * 60 * 24;
                    break;
                case "Weekly":
                    retval = 60 * 60 * 24 * 7;
                    break;
                default:
                    retval = 60 * 60 * 24;
                    break;
            }
            return retval;
        }
    }
}
