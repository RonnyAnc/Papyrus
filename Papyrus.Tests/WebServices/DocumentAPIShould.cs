﻿namespace Papyrus.Tests.WebServices.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using FluentAssertions;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using NUnit.Framework;
    using Papyrus.Business.Documents;
    using Papyrus.Business.Documents.Exceptions;
    using Papyrus.WebServices;
    using Papyrus.WebServices.Controllers;
    using Papyrus.WebServices.Models;

    [TestFixture]
    public class DocumentApiShould : OwinRunner
    {
        // TODO:
        //  when getting all Documents, it should return a list of dtos corresponding to all existant documents
        //  when creating a document, it should return a 201 http status code
        //  when update a document, it should return a 200 http status code
        //  when try to update a no existing document, it should return a 404 http status code
        //  when remove a document, it should return a 204 http status code
        //  when try to remove a no existing document, it should return a 404 http status code 

        private const string AnyId = "AnyId";
        private const string AnyTitle = "AnyTitle";
        private const string AnyContent = "AnyContent";
        private const string AnyDescription = "AnyDescription";
        private const string AnyLanguage = "AnyLanguage";

        [Test]
        public async void when_looking_for_an_existing_document_it_should_return_a_dto_with_its_information()
        {
            GivenAWebApiWithADocumentServiceWhichWhenTryingToGetADocumentReturns(AnyDocument());
            
            var client = new RestClient(baseAddress);
            var documentDto = await client.Get<DocumentDto>("documents/" + AnyId);

            ShouldBeADocumentDtoWithNoNullFields(documentDto);
        }

        [Test]
        public void return_a_404_http_status_code_when_looking_for_a_no_existing_document()
        {
            GivenAWebApiWithADocumentServiceWhichWhenTryingToGetADocumentReturns(null);

            var client = new RestClient(baseAddress);
            Func<Task> asyncCall = async () => await client.Get<DocumentDto>("documents/" + AnyId);

            asyncCall.ShouldThrow<HttpResponseException>()
                .Which.Response.StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async void return_a_list_whit_a_documentdto_for_each_existing_document_when_getting_all_documents()
        {
            var documentService = SubstituteForDocumentService();
            var expectedDocumentsList = new[] { new Document().WithTitle(AnyTitle) };
            documentService.AllDocuments().Returns(
                Task.FromResult(expectedDocumentsList)
            );
            WebApiConfig.Container.RegisterInstance(documentService);

            var client = new RestClient(baseAddress);
            var documents = await client.Get<DocumentDto[]>("documents/");

            documents.Length.Should().Be(1);
            documents[0].Title.Should().Be("AnyTitle");
        }

        private static DocumentService SubstituteForDocumentService()
        {
            var anyRepository = Substitute.For<DocumentRepository>();
            return Substitute.For<DocumentService>(anyRepository);
        }

        private void GivenAWebApiWithADocumentServiceWhichWhenTryingToGetADocumentReturns(Document anyDocument)
        {
            var documentService = SubstituteForDocumentService();
            documentService.GetDocumentById(AnyId).Returns(
                Task.FromResult(anyDocument)
            );
            WebApiConfig.Container.RegisterInstance(documentService);
        }

        private static void ShouldBeADocumentDtoWithNoNullFields(DocumentDto documentDto)
        {
            documentDto.Title.Should().Be(AnyTitle);
            documentDto.Description.Should().Be(AnyDescription);
            documentDto.Content.Should().Be(AnyContent);
            documentDto.Language.Should().Be(AnyLanguage);
        }

        private static Document AnyDocument()
        {
            return new Document()
                .WithId(AnyId)
                .WithTitle(AnyTitle)
                .WithContent(AnyContent)
                .WithDescription(AnyDescription)
                .ForLanguage(AnyLanguage);
        }
    }
}