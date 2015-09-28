﻿using System;

namespace Papyrus.Business
{
    public class TopicService
    {
        public TopicRepository TopicRepository { get; set; }

        public TopicService(TopicRepository topicRepo)
        {
            TopicRepository = topicRepo;
        }

        public void Create(Topic topic)
        {
            TopicRepository.Save(topic);
        }

        public void Update(Topic topic)
        {
            if (String.IsNullOrEmpty(topic.TopicId))
                throw new CannotUpdateWithoutTopicIdDeclaredException();
            TopicRepository.Update(topic);
        }
    }
}