﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Papyrus.Business.Topics;
using Papyrus.Desktop.Features.Documents;

namespace Papyrus.Desktop.Features.Topics {
    public class TopicsGridVM
    {
        private readonly TopicRepository topicRepository;

        public ObservableCollection<TopicToShow> TopicsToShow { get; }
        public TopicToShow SelectedTopic { get; set; }

        public TopicsGridVM()
        {
            TopicsToShow = new ObservableCollection<TopicToShow>();
        }

        public TopicsGridVM(TopicRepository topicRepository) : this()
        {
            this.topicRepository = topicRepository;
        }

        public async Task Initialize()
        {
            await LoadAllTopics();
        }

        private async Task LoadAllTopics()
        {
            TopicsToShow.Clear();
            (await topicRepository.GetAllTopicsToShow()).ForEach(topic => TopicsToShow.Add(topic));
        }

        public async void RefreshDocuments()
        {
            await LoadAllTopics();
        }
    }

    public class DesignModeDocumentsGridVM : DocumentsGridVM {
        public DesignModeDocumentsGridVM()
        {
        }
    }

}
