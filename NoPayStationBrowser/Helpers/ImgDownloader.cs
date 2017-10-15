using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NoPayStationBrowser.Helpers
{
    public static class DataDownloader
    {
        static string SafeTitle(string title)
        {
            return title.Replace(" (DLC)", "");
        }

        public static Renascene GetRenascene(string titleId)
        {
            try
            {
                titleId = SafeTitle(titleId);
                WebClient wc = new WebClient();
                string content = wc.DownloadString(@"http://renascene.com/psv/?target=search&srch=" + titleId + "&srchser=1");
                string url = ExtractString(content, "<td class=\"l\"><a href=\"", "\">");
                content = wc.DownloadString(url);
                Renascene r = new Renascene();

                var imgUrl = ExtractString(content, "<td width=\"300pt\" style=\"vertical-align: top; padding: 0 0 0 5px;\">", "</td>");
                r.imgUrl = ExtractString(imgUrl, "<img src=", ">");

                var genre = ExtractString(content, "<td class=\"infLeftTd\">Genre</td>", "</tr>");
                r.genre = ExtractString(genre, "<td class=\"infRightTd\">", "</td>");
                r.genre = r.genre.Replace("Â»", "/");

                var language = ExtractString(content, "<td class=\"infLeftTd\">Language</td>", "</tr>");
                r.language = ExtractString(language, "<td class=\"infRightTd\">", "</td>");

                var publish = ExtractString(content, "<td class=\"infLeftTd\">Publish Date</td>", "</tr>");
                r.publish = ExtractString(publish, "<td class=\"infRightTd\">", "</td>");

                var developer = ExtractString(content, "<td class=\"infLeftTd\">Developer</td>", "</tr>");
                r.developer = ExtractString(developer, "<td class=\"infRightTd\">", "</td>");

                return r;
            }
            catch (Exception err)
            {
                return null;
            }
        }


        static string ExtractString(string s, string start, string end)
        {
            int startIndex = s.IndexOf(start) + start.Length;
            int endIndex = s.IndexOf(end, startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }




    }


    public class Renascene
    {
        public string imgUrl, genre, language, publish, developer;

        public override string ToString()
        {
            return string.Format(@"Genre: {0}
Language: {1}
Published: {2}
Developer: {3}", this.genre, this.language, this.publish, this.developer);
        }
    }
}
