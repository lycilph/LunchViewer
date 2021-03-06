﻿using System.Collections.Generic;
using System.Web.Http;
using HtmlAgilityPack;
using LunchViewerService.DataObjects;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.Utils
{
    public static class EmailHelper
    {
        public static bool TryParseMessage(ApiServices services, string message, out Menu menu)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(message);

            // Find current year and week for the message
            var week = -1;
            var matches = Regex.Matches(doc.DocumentNode.InnerText, @"(Menuer i denne uge) (\d*)", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
                week = int.Parse(matches[0].Groups[2].ToString());
            var year = DateTime.Now.Year;
            
            menu = new Menu { Year = year, Week = week, Items = new List<Item>() };

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
                var link = item.Descendants("a").Select(n => n.Attributes["href"].Value).Single();

                menu.Items.Add(new Item { Date = date, Text = text, Link = link });
            }

            services.Log.Info(string.Format("Found week {0}, year {1}, items {2}", week, year, menu.Items.Count()));

            // Success if we found any items and a valid week number
            return week > -1 && menu.Items.Any();
        }
    }
}
