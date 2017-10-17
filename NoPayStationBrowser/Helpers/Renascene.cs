using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NPS.Helpers
{

    public class Renascene
    {
        public string imgUrl, genre, language, publish, developer;

        public Renascene(string titleId)
        {
            try
            {
                titleId = SafeTitle(titleId);

                WebClient wc = new WebClient();
                string content = wc.DownloadString(@"http://renascene.com/psv/?target=search&srch=" + titleId + "&srchser=1");
                string url = ExtractString(content, "<td class=\"l\"><a href=\"", "\">");
                content = wc.DownloadString(url);

                this.imgUrl = ExtractString(content, "<td width=\"300pt\" style=\"vertical-align: top; padding: 0 0 0 5px;\">", "</td>");
                this.imgUrl = ExtractString(imgUrl, "<img src=", ">");

                genre = ExtractString(content, "<td class=\"infLeftTd\">Genre</td>", "</tr>");
                genre = ExtractString(genre, "<td class=\"infRightTd\">", "</td>");
                genre = genre.Replace("Â»", "/");

                language = ExtractString(content, "<td class=\"infLeftTd\">Language</td>", "</tr>");
                language = ExtractString(language, "<td class=\"infRightTd\">", "</td>");

                publish = ExtractString(content, "<td class=\"infLeftTd\">Publish Date</td>", "</tr>");
                publish = ExtractString(publish, "<td class=\"infRightTd\">", "</td>");

                developer = ExtractString(content, "<td class=\"infLeftTd\">Developer</td>", "</tr>");
                developer = ExtractString(developer, "<td class=\"infRightTd\">", "</td>");
            }
            catch (Exception err)
            {
                imgUrl = genre = language = publish = developer = null;
            }
        }



        public override string ToString()
        {
            return string.Format(@"Genre: {0}
Language: {1}
Published: {2}
Developer: {3}", this.genre, this.language, this.publish, this.developer);
        }


        string SafeTitle(string title)
        {
            return title.Replace(" (DLC)", "");
        }

        string ExtractString(string s, string start, string end)
        {
            int startIndex = s.IndexOf(start) + start.Length;
            int endIndex = s.IndexOf(end, startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }
    }
}
