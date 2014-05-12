﻿using HtmlAgilityPack;
using MimeKit;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailProcessor
{
    public static class EmailHelper
    {
        public static bool TryParseMessage(MimeMessage message, out Menu menu)
        {
            menu = null;

            foreach (var part in message.BodyParts)
            {
                //Trace.TraceInformation(" - {0} {1}", part.ContentType.MediaType, part.ContentType.MediaSubtype);
                if (part.ContentType.MediaType == "text" && part.ContentType.MediaSubtype == "html")
                {
                    var text_part = part as TextPart;
                    if (text_part == null)
                        return false;

                    var doc = new HtmlDocument();
                    doc.LoadHtml(text_part.Text);

                    // Find current year and week for the message
                    var week = -1;
                    var matches = Regex.Matches(doc.DocumentNode.InnerText, @"(Menuer i denne uge) (\d*)", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                        week = int.Parse(matches[0].Groups[2].ToString());
                    var year = DateTime.Now.Year;

                    menu = new Menu(year, week);

                    // Find the individual items (or days)
                    var items = doc.DocumentNode.Descendants().Where(n => n.Name == "td" &&
                                                                  n.Attributes.Contains("class") &&
                                                                  n.Attributes["class"].Value == "productItem");

                    // Parse item
                    foreach (var item in items)
                    {
                        var rows = item.Descendants("tr").Select(n => n.InnerText.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                        var date = DateTime.ParseExact(rows[0], "dddd 'd.' d. MMMM", CultureInfo.CreateSpecificCulture("da-DK"));
                        var text = string.Format("{0} {1}", HtmlEntity.DeEntitize(rows[3]).Trim(), HtmlEntity.DeEntitize(rows[4]).Trim());

                        //Trace.TraceInformation("-----------------------------------------------------------------------");
                        //Trace.TraceInformation(date.ToString("dddd 'd.' d. MMMM", CultureInfo.CreateSpecificCulture("da-DK")));
                        //Trace.TraceInformation(text);

                        menu.Add(date, text);
                    }

                    return true;
                }

                Trace.TraceInformation(" --- Wrong content type");
            }

            return false;
        }

        public static bool IsValidMenuMail(MimeMessage message)
        {
            if (message.From.Count != 1)
                return false;

            var mailbox = message.From[0] as MailboxAddress;
            if (mailbox == null)
                return false;

            return string.Compare(mailbox.Address, "oticon@wip.dk", StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
