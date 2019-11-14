using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Nacos.Config.Utils
{
    public class ContentUtils
    {
        public const int SHOW_CONTENT_SIZE = 100;

        public static string TruncateContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";
            else if (content.Length <= SHOW_CONTENT_SIZE)
            {
                return content;
            }
            else
            {
                return content.Substring(0, SHOW_CONTENT_SIZE) + "...";
            }
        }
    }
}
