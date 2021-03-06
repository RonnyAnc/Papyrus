﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Papyrus.Business.Exporters {
    internal class MkdocsConfiguration {
        private const string SimaSiteName = "SIMA Documentation";
        private const string DefaultTheme = "readthedocs";
        private const string KeyValueSeparator = ": ";
        private static readonly string NewLine = Environment.NewLine;
        
        public string Theme { get; private set; }
        public string SiteName { get; private set; }
        public string SiteDir { get; private set; }
        private readonly Dictionary<string, string> pages = new Dictionary<string, string>();

        public MkdocsConfiguration(string siteDir) {
            Theme = DefaultTheme;
            SiteName = SimaSiteName;
            SiteDir = siteDir;
        }

        public void AddPage(string pageName, string fileName) {
            pages.Add(pageName, fileName);
        }

        public override string ToString() {
            var themeLine = "theme" + KeyValueSeparator + Theme + NewLine;
            var siteNameLine = "site_name" + KeyValueSeparator + SiteName + NewLine;
            var siteDir = "site_dir" + KeyValueSeparator + SiteDir + NewLine;
            var pagesLines = "pages" + KeyValueSeparator + NewLine;
            pagesLines = pages.Aggregate(pagesLines, (current, page) => current + ToMkdocsPageFormat(page));
            return themeLine + siteNameLine + siteDir + pagesLines;
        }

        private static string ToMkdocsPageFormat(KeyValuePair<string, string> page) {
            return "- " + "'" + page.Key + "'" + KeyValueSeparator + "'" + page.Value + "'" + NewLine;
        }
    }
}