﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Papyrus.Business.Documents;
using Papyrus.Business.Exporters;
using Papyrus.Business.Products;
using Papyrus.Business.Topics;
using Papyrus.Business.VersionRanges;
using Papyrus.Desktop.Annotations;
using Papyrus.Desktop.Util.Command;
using Papyrus.Infrastructure.Core.DomainEvents;


namespace Papyrus.Desktop.Features.Topics {
    public class TopicsGridVm : INotifyPropertyChanged
    {
        private readonly MkDocsExporter exporter;
        private readonly WebsiteConstructor websiteConstructor;
        private readonly TopicQueryRepository topicRepository;
        private readonly ProductRepository productRepository;
        public ObservableCollection<TopicSummary> TopicsToList { get; protected set; }
        public ObservableCollection<DisplayableProduct> Products { get; private set; }
        public TopicSummary SelectedTopic { get; set; }
        private string DefaultDirectoryPath { get; }

        private DisplayableProduct selectedProduct;
        public DisplayableProduct SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                OnPropertyChanged();
                RefreshTopicsForCurrentProduct();
            }
        }

        public IAsyncCommand RefreshTopics { get; private set; }
        public IAsyncCommand ExportProductToMkDocs { get; private set; }

        protected TopicsGridVm()
        {
            TopicsToList = new ObservableCollection<TopicSummary>();
            Products = new ObservableCollection<DisplayableProduct>();
            RefreshTopics = RelayAsyncSimpleCommand.Create(LoadAllTopics, CanLoadAllTopics);
            ExportProductToMkDocs = RelayAsyncSimpleCommand.Create(ExportProduct, () => true);
            DefaultDirectoryPath = Directory.GetCurrentDirectory();
            EventBus.AsObservable<OnTopicRemoved>().Subscribe(Handle);
        }

        //TODO: should it return a task?
        private async void Handle(OnTopicRemoved domainEvent) {
            await LoadAllTopics();
        }

        public TopicsGridVm(TopicQueryRepository topicRepo, ProductRepository productRepo, MkDocsExporter exporter, WebsiteConstructor websiteConstructor)
            : this(topicRepo, productRepo) {
            this.exporter = exporter;
            this.websiteConstructor = websiteConstructor;
        }

        private async Task ExportProduct() {
            var product = await productRepository.GetProduct(SelectedProduct.ProductId);
            var websiteCollection = await websiteConstructor.Construct(product, languages);
            await Export(websiteCollection);
        }
        
        private async Task Export(WebsiteCollection websiteCollection) {
            try {
                await TryExportation(websiteCollection);
                ToastNotificator.NotifyMessage("Exportación realizada con éxito");
            }
            catch (Exception ex)
            {
                EventBus.Send(new OnUserMessageRequest("Ha ocurrido un error en la exportación"));
            }
        }

        private async Task TryExportation(WebsiteCollection websiteCollection) {
            foreach (var webSite in websiteCollection) {
                var imagesFolder = ConfigurationManager.AppSettings["ImagesFolder"];
                var siteDir = Path.Combine(DefaultDirectoryPath, webSite.ProductName, webSite.Version, webSite.Language);
                var exportationPath = DefaultDirectoryPath + "/" + GenerateMkdocsPath(webSite);
                await exporter.Export(webSite,
                    new ConfigurationPaths(exportationPath, imagesFolder, siteDir));
            }
        }

        public virtual string GenerateMkdocsPath(WebSite webSite)
        {
            const string separator = "/";
            return webSite.Version + separator + webSite.ProductName + separator + webSite.Language;
        }

        private static Product CastToProductType(DisplayableProduct p) {
            return new Product(p.ProductId, p.ProductName, new List<ProductVersion>());
        }

        public TopicsGridVm(TopicQueryRepository topicRepository, ProductRepository productRepository) : this()
        {
            this.topicRepository = topicRepository;
            this.productRepository = productRepository;
        }

        public async Task Initialize()
        {
            await LoadAllProducts();
            SelectedProduct = Products.FirstOrDefault();
        }

        public async void RefreshTopicsForCurrentProduct()
        {
            await LoadAllTopics();
        }

        public async Task<EditableTopic> GetEditableTopicById(string topicId)
        {
            return await topicRepository.GetEditableTopicById(topicId);
        }

        public async Task<EditableTopic> PrepareNewDocument()
        {
            var fullVersionRange = await DefaultVersionRange();
            var versionRanges = new ObservableCollection<EditableVersionRange> { fullVersionRange };
            return new EditableTopic
            {
                Product = SelectedProduct,
                VersionRanges = versionRanges
            };
        }

        private async Task LoadAllProducts()
        {
            var products = await productRepository.GetAllDisplayableProducts();
            products.ForEach(p => Products.Add(p));
        }

        private async Task LoadAllTopics()
        {
            canLoadTopics = false;
            TopicsToList.Clear();
            (await topicRepository.GetAllTopicsSummariesFor("es-ES"))
                .Where(t => t.Product.ProductId == SelectedProduct.ProductId)
                .ToList()
                .ForEach(topic => TopicsToList.Add(topic));
            canLoadTopics = true;
        }

        private readonly List<string> languages = new List<string>{ "es-ES", "en-GB" };

        private bool canLoadTopics;
        private bool CanLoadAllTopics()
        {
            return canLoadTopics;
        }

        private async Task<EditableVersionRange> DefaultVersionRange()
        {
            var fullVersionRange = await productRepository.GetFullVersionRangeForProduct(SelectedProduct.ProductId);
            var editableVersionRange = new EditableVersionRange
            {
                FromVersion = await productRepository.GetVersion(fullVersionRange.FirstVersionId),
                ToVersion = await productRepository.GetVersion(fullVersionRange.LatestVersionId)
            };
            AddDocumentsForDefaultLanguages(editableVersionRange);
            return editableVersionRange;
        }

        private static void AddDocumentsForDefaultLanguages(EditableVersionRange editableVersionRange)
        {
            editableVersionRange.Documents.Add(new EditableDocument { Language = "es-ES" });
            editableVersionRange.Documents.Add(new EditableDocument { Language = "en-GB" });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OnTopicRemoved {}
}
