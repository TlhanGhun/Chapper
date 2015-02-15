using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapper.Model
{
    public class Error
    {
        public string title { get; set; }
        public string description { get; set; }
        public string stacktrace { get; set; }
        public string userComment { get; set; }
    }
}
