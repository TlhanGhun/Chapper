using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapper.Helper
{
    public class DateHelpers
    {
        public static string getHumanReadableAgo(DateTime date, out int NextUpdateInXSeconds, out bool stopUpdating)
        {
            string output = String.Empty;
            stopUpdating = false;
            TimeSpan time = DateTime.Now - date;

            NextUpdateInXSeconds = 60;

            if (time.TotalSeconds <= 0)
            {
                output += "right now";
                // if date in the future wait until it in the current time
                NextUpdateInXSeconds = Math.Min(2, -1 * Convert.ToInt32(time.TotalSeconds));
                return output;
            }

            if (time.Days > 0 && time.Days < 4)
            {

                if (time.Days == 1)
                {
                    output += "one day ago";
                }
                else
                {
                    output += time.Days + " days ago";
                }
                NextUpdateInXSeconds = 10;
                NextUpdateInXSeconds += (24 - time.Hours) * 60 * 60;
                NextUpdateInXSeconds += (60 - time.Minutes) * 60;
                NextUpdateInXSeconds += (60 - time.Seconds);

            }
            else if (time.Days > 3)
            {
                output = date.ToLongDateString();
                NextUpdateInXSeconds = -1;
                stopUpdating = true;

            }
            else if (time.Hours > 0)
            {

                if (time.Hours == 1)
                {
                    output += "one hour ago";
                }
                else
                {
                    output += time.Hours + " hours ago";
                }
                NextUpdateInXSeconds = 10;
                NextUpdateInXSeconds += (60 - time.Minutes) * 60;
                NextUpdateInXSeconds += (60 - time.Seconds);
            }

            else if (time.Minutes > 0)
            {

                if (time.Minutes == 1)
                {
                    output += "one minute ago ";
                }
                else
                {
                    output += time.Minutes + " minutes ago ";
                }
                NextUpdateInXSeconds += (60 - time.Seconds);
            }
            else
            {
                NextUpdateInXSeconds = 1;
                if (time.Seconds == 1)
                {
                    output += time.Seconds + " second ago";
                }
                else
                {
                    output += time.Seconds + " seconds ago";
                }
            }
            return output;
        }
    }
}
