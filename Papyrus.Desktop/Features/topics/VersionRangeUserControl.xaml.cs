﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Papyrus.Business.Topics;

namespace Papyrus.Desktop.Features.Topics
{
    public partial class VersionRangeUserControl : UserControl
    {
        public VersionRangeVM ViewModel
        {
            get { return (VersionRangeVM) DataContext; }
        }

        public VersionRangeUserControl()
        {
            InitializeComponent();

            this.Loaded += VersionRangeUserControl_Loaded;
        }

        private void VersionRangeUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Initialize();
        }
    }
}
