﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Papyrus.Business.Documents;
using Papyrus.Business.Products;
using Papyrus.Business.Topics;
using Papyrus.Business.VersionRanges;
using Papyrus.Infrastructure.Core.Database;
using Papyrus.Tests.Infrastructure.Repositories.Helpers;

namespace Papyrus.Tests.Infrastructure.Repositories.TopicRepository
{
    [TestFixture]
    public class SqlTopicWhenSaveATopicShould : SqlTest
    {
        private Topic anyTopic;
        private SqlTopicCommandRepository topicRepository;
        private const string ProductId = "OpportunityId";
        private const string FirstVersionId = "FirstVersionId";
        private ProductVersion version1 = new ProductVersion(FirstVersionId, "1.0", DateTime.Today);

        [SetUp]
        public async void Initialize()
        {
            anyTopic = new Topic(ProductId).WithId("AnyTopicId");
            topicRepository = new SqlTopicCommandRepository(dbConnection);
            await new DataBaseTruncator(dbConnection).TruncateDataBase();
        }

        [Test]
        public async Task save_a_topic()
        {
            await topicRepository.Save(anyTopic);

            var topicFromRepo = await GetTopicById("AnyTopicId");
            Assert.That(topicFromRepo.ProductId, Is.EqualTo(ProductId));
        }

        [Test]
        public async Task save_version_ranges_of_a_topic()
        {
            var versionRange = new VersionRange(fromVersion: version1, toVersion: version1).WithId("FirstVersionRangeId");
            anyTopic.AddVersionRange(versionRange);

            await topicRepository.Save(anyTopic);

            var versionRangeFromRepo = await GetVersionRangeById("FirstVersionRangeId");
            Assert.That(versionRangeFromRepo.FromVersionId, Is.EqualTo(FirstVersionId));
            Assert.That(versionRangeFromRepo.ToVersionId, Is.EqualTo(FirstVersionId));
            Assert.That(versionRangeFromRepo.TopicId, Is.EqualTo("AnyTopicId"));
        }

        [Test]
        public async Task save_documents_foreach_version_range_in_a_topic()
        {
            var versionRange = new VersionRange(fromVersion: version1, toVersion: version1).WithId("AnyVersionRangeId");
            anyTopic.AddVersionRange(versionRange);
            versionRange.AddDocument(
                new Document("AnyTitle", "AnyDescription", "AnyContent", "es-ES").WithId("AnyDocumentId")
            );

            await topicRepository.Save(anyTopic);

            var documentFromRepo = await GetDocumentById("AnyDocumentId");
            Assert.That(documentFromRepo.Title, Is.EqualTo("AnyTitle"));
            Assert.That(documentFromRepo.Description, Is.EqualTo("AnyDescription"));
            Assert.That(documentFromRepo.Content, Is.EqualTo("AnyContent"));
            Assert.That(documentFromRepo.Language, Is.EqualTo("es-ES"));
            Assert.That(documentFromRepo.VersionRangeId, Is.EqualTo("AnyVersionRangeId"));
        }

        private async Task<dynamic> GetDocumentById(string id)
        {
            return (await dbConnection.Query<dynamic>(@"SELECT Title, Description, Content, Language, VersionRangeId 
                                                        FROM Document 
                                                        WHERE DocumentId = @DocumentId",
                                                        new { DocumentId = id }))
                                                        .FirstOrDefault();
        }

        private async Task<dynamic> GetTopicById(string topicId)
        {
            return (await dbConnection.Query<dynamic>(@"SELECT TopicId, ProductId FROM Topic
                                                        WHERE TopicId = @TopicId;",
                                                        new { TopicId = topicId }))
                                                        .FirstOrDefault();
        }

        private async Task<dynamic> GetVersionRangeById(string id)
        {
            return (await dbConnection.Query<dynamic>(@"SELECT FromVersionId, ToVersionId, TopicId FROM VersionRange
                                                        WHERE VersionRangeId = @VersionRangeId;",
                                                        new { VersionRangeId = id }))
                                                        .FirstOrDefault();
        }
    }
}