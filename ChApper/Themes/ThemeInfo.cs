using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapper.Themes
{
    public class ThemeInfo
    {
        public string name { get; set; }
        public string filepath { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string copyright { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public string homepage { get; set; }

        public bool useDefaultDarkItems { get; set; }

        public string tooltip
        {
            get
            {
                string tt = name;
                if (!string.IsNullOrEmpty(title))
                {
                    tt = title;
                }
                if (!string.IsNullOrEmpty(version))
                {
                    tt += " " + version;
                }
                if (!string.IsNullOrEmpty(description))
                {
                    tt += "\r\n" + description;
                }
                if (!string.IsNullOrEmpty(author))
                {
                    tt += "\r\nBy: " + author;
                }
                if (!string.IsNullOrEmpty(copyright))
                {
                    tt += "\r\nCopyright: " + copyright;
                }
                if (!string.IsNullOrEmpty(homepage))
                {
                    tt += "\r\nHomepage: " + homepage;
                }
                return tt;
            }
        }
    }
}
