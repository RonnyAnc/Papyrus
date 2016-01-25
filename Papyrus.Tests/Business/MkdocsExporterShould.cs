﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Papyrus.Business.Exporters;
using Papyrus.Business.Products;

namespace Papyrus.Tests.Business {
    [TestFixture]
    public class MkdocsExporterShould {
        private const string AnyMkdocsPath = "AnyLanguage/AnyVersion";
        private DirectoryInfo testDirectory;
        private readonly string newLine = Environment.NewLine;

        [SetUp]
        public void SetUp() {
            var path = Path.GetTempPath();
            var testDirectoryPath = Path.Combine(path, Guid.NewGuid().ToString());
            testDirectory = Directory.CreateDirectory(testDirectoryPath);
        }

        [TearDown]
        public void TearDown() {
            testDirectory.Delete(true);
        }

        [Test]
        public async Task generate_mkdocs_yml_file_in_the_given_path() {
            var webSite = WebSiteWithDocuments(AnyDocument());

            await new MkdocsExporter().Export(webSite, GetAnyExportationPath());

            GetFilesFrom(AnyMkdocsPath).Should().Contain(x => x.Name == "mkdocs.yml");
        }

        [Test]
        public async Task generate_documents_in_docs_file() {
            var webSite = WebSiteWithDocuments(new ExportableDocument("Title", "Content", ""));

            await new MkdocsExporter().Export(webSite, GetAnyExportationPath());

            var content = GetFileContentFrom(Path.Combine(GetAnyExportationPath(), "docs/Title.md"));
            content.Should().Be("Content" + System.Environment.NewLine);
        }
        
        [Test]
        public async Task generate_docs_folder_in_the_given_folder() {
            var webSite = WebSiteWithDocuments(AnyDocument());

            await new MkdocsExporter().Export(webSite, GetAnyExportationPath());

            GetFoldersFrom(AnyMkdocsPath).Should().Contain(x => x.Name == "docs");
        }

        [Test]
        public async Task write_the_theme_in_the_yml_file() {
            var webSite = WebSiteWithDocuments(AnyDocument());

            await new MkdocsExporter().Export(webSite, GetAnyExportationPath());

            var documentPath = Path.Combine(GetAnyExportationPath(), "mkdocs.yml");
            GetFileContentFrom(documentPath).Should().Contain("theme: readthedocs");
        }
        
        [Test]
        public async Task write_the_site_name_in_the_yml_file() {
            var webSite = WebSiteWithDocuments(AnyDocument());

            await new MkdocsExporter().Export(webSite, GetAnyExportationPath());

            var documentPath = Path.Combine(GetAnyExportationPath(), "mkdocs.yml");
            GetFileContentFrom(documentPath).Should().Contain("site_name: SIMA Documentation");
        }
        
        [Test]
        public async Task write_the_index_file_into_docs_directory() {
            var webSite = WebSiteWithDocuments(AnyDocument());

            await new MkdocsExporter().Export(webSite, GetAnyExportationPath());

            GetDocsFrom(GetAnyExportationPath()).Should().Contain(d => d.Name == "index.md");
        }

        [Test]
        public async Task replace_unavailabe_characters_for_a_file_for_available_ones() {
            var website = WebSiteWithDocuments(
                new ExportableDocument("this/is|the*Title", "AnyContent", ""),
                new ExportableDocument("otro>título?ñ", "AnotherContent", ""));

            await new MkdocsExporter().Export(website, GetAnyExportationPath());

            var ymlPath = Path.Combine(GetAnyExportationPath(), "mkdocs.yml");
            GetFileContentFrom(ymlPath).Should().Contain(
                "pages:" + newLine +
                "- 'Home': 'index.md'" + newLine +
                "- 'this/is|the*Title': 'this-is-the-Title.md'" + newLine +
                "- 'otro>título?ñ': 'otro-titulo-n.md'");
        }
        
        [Test]
        public async Task replace_unavailabe_characters_for_a_file_for_available_ones_when_have_a_route_in_each_document() {
            var firstWebsite = WebSiteWithDocuments(
                new ExportableDocument("first-file", "AnyContent", "first-route"),
                new ExportableDocument("second>file", "AnotherContent", "first-route"));
            var secondWebsite = WebSiteWithDocuments(
                new ExportableDocument("third>file", "MoreContent", "second-route"));

            await new MkdocsExporter().Export(firstWebsite, GetAnyExportationPath());
            await new MkdocsExporter().Export(secondWebsite, GetAnyExportationPath());

            var ymlPath = Path.Combine(GetAnyExportationPath(), "mkdocs.yml");
            GetFileContentFrom(ymlPath).Should().Contain(
                "pages:" + newLine +
                "- 'Home': 'index.md'" + newLine +
                "- 'first-route':" + newLine +
                "    - 'first-file': 'first-route\\first-file.md'" + newLine +
                "    - 'second>file': 'first-route\\second-file.md'" + newLine +
                "- 'second-route':" + newLine + 
                "    - 'third>file': 'second-route\\third-file.md'");
        }

        private string GetAnyExportationPath() {
            return Path.Combine(testDirectory.FullName, AnyMkdocsPath);
        }

        private string GetFileContentFrom(string documentPath) {
            return File.ReadAllText(documentPath);
        }

        private FileInfo[] GetDocsFrom(string path) {
            var docsPath = Path.Combine(path, "docs");
            return new DirectoryInfo(Path.Combine(testDirectory.FullName, docsPath)).GetFiles();
        }

        private DirectoryInfo[] GetFoldersFrom(string path) {
            return new DirectoryInfo(Path.Combine(testDirectory.FullName, path)).GetDirectories();
        }

        private FileInfo[] GetFilesFrom(string path) {
            return new DirectoryInfo(Path.Combine(testDirectory.FullName, path)).GetFiles();
        }

        private static ExportableDocument AnyDocument() {
            return new ExportableDocument("AnyTitle", "AnyContent", "/AnyWebsitePath");
        }

        private static WebSite WebSiteWithDocuments(params ExportableDocument[] exportableDocuments) {
            return new WebSite(exportableDocuments);
        }
    }
}