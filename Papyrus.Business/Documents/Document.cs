using System;

namespace Papyrus.Business.Documents
{
    public class Document
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Content { get; private set; }
        public string DocumentId { get; private set; }
        public string Language { get; private set; }

        public Document(string title, string description, string content, string language) {
            if (string.IsNullOrEmpty(title)) throw new CannotCreateDocumentsWithoutTitleException();
            Title = title;
            Description = description;
            Content = content;
            Language = language;
        }

        public Document WithId(string id)
        {
            DocumentId = id;
            return this;
        }

        public void GenerateAutomaticId()
        {
            DocumentId = Guid.NewGuid().ToString();
        }
    }
}