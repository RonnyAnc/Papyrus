﻿namespace Papyrus.Business.Documents
{
    using System.Threading.Tasks;
    using Exceptions;

    public class DocumentService
    {
        private readonly DocumentRepository repository;

        public DocumentService(DocumentRepository repository)
        {
            this.repository = repository;
        }

        public virtual async Task Create(Document document)
        {
            if (string.IsNullOrWhiteSpace(document.ProductVersionId))
                throw new DocumentMustBeAssignedToAProductVersionException();

            if (string.IsNullOrWhiteSpace(document.Language))
                throw new DocumentMustHaveALanguageException();

            if (!string.IsNullOrWhiteSpace(document.Id))
                throw new DocumentIdMustNotBeDefinedException();
            document.GenerateAutomaticId();
            await repository.Save(document);
        }

        public virtual async Task<Document> GetDocumentById(string id)
        {
            return await repository.GetDocument(id);
        }

        public virtual async Task Update(Document document)
        {
            if (string.IsNullOrWhiteSpace(document.Id))
                throw new DocumentIdMustBeDefinedException();
            if (await GetDocumentById(document.Id) == null)
                throw new DocumentNotFoundException();
            await repository.Update(document);
        }

        public virtual async Task Remove(string documentId)
        {
            if (await GetDocumentById(documentId) == null)
                throw new DocumentNotFoundException();
            await repository.Delete(documentId);
        }

        public virtual async Task<Document[]> AllDocuments()
        {
            return (await repository.GetAllDocuments()).ToArray();
        }
    }
}