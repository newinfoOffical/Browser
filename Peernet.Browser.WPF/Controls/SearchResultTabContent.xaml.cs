﻿using DevExpress.Xpf.Grid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.ApplicationServices;
using Peernet.Browser.Application.Download;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Application.Utilities;
using Peernet.Browser.Application.ViewModels;
using Peernet.Browser.Application.ViewModels.Parameters;
using Peernet.Browser.WPF.Extensions;
using Peernet.SDK.Client.Clients;
using Peernet.SDK.Models.Presentation.Footer;
using Peernet.SDK.Models.Presentation.Home;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Peernet.Browser.WPF.Controls
{
    /// <summary>
    /// Interaction logic for SearchResultTabContent.xaml
    /// </summary>
    public partial class SearchResultTabContent : UserControl
    {
        public SearchResultTabContent()
        {
            InitializeComponent();
            SearchResultsTable.HiddenColumnChooser += SearchResultsTable_ColumnChooserStateChanged;
            SearchResultsTable.ShownColumnChooser += SearchResultsTable_ColumnChooserStateChanged;
            App.MainWindowClicked += OnMainWindowClicked;
            pager.Loaded += Pager_Loaded;
        }

        private void FilterIconControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchResultsTable.IsColumnChooserVisible ^= true;
        }

        private void OnMainWindowClicked(object sender, RoutedEventArgs e)
        {
            var filterIconControl = ((DependencyObject)e.OriginalSource).FindParent<FilterIconControl>();
            var filterType = (filterIconControl?.DataContext as IconModel)?.FilterType;
            var columnsControl = ((DependencyObject)e.OriginalSource).FindParent<ColumnsSelectorControl>();
            var filtersControl = ((DependencyObject)e.OriginalSource).FindParent<FiltersControl>();
            if (columnsControl == null && filtersControl == null && filterType == null)
            {
                if (DataContext is SearchTabElementViewModel viewModel)
                {
                    SearchResultsTable.IsColumnChooserVisible = false;
                    viewModel.FiltersIconModel.IsSelected = false;
                }
            }
        }

        private void Open_OnClick(object sender, MouseButtonEventArgs e)
        {
            var cellData = (EditGridCellData)((FrameworkElement)e.OriginalSource).DataContext;
            var model = (DownloadModel)cellData.RowData.Row;
            var dataTransferManager = App.ServiceProvider.GetRequiredService<IDataTransferManager>();
            var downloadClient = App.ServiceProvider.GetRequiredService<IDownloadClient>();
            var filePath = Path.Combine(App.Settings.DownloadPath, UtilityHelper.StripInvalidWindowsCharactersFromFileName(model.File.Name));
            var download = new SDK.Models.Presentation.Download(downloadClient, model.File, filePath);
            var param = new FilePreviewViewModelParameter(model.File, async () => await dataTransferManager.QueueUp(download), "Download");
            var filePreviewViewModel = new FilePreviewViewModel();
            filePreviewViewModel.Prepare(param);
            new FilePreviewWindow(filePreviewViewModel).Show();
        }

        private void Pager_Loaded(object sender, RoutedEventArgs e)
        {
            pager.PageIndexChanged += Pager_Changed;
            pager.PageSizeChanged += Pager_Changed;
        }

        private async void Pager_Changed(object sender, System.EventArgs e)
        {
            if (DataContext != null)
            {
                await (DataContext as SearchTabElementViewModel)?.Refresh();
            }
        }

        private void OpenFileWebGatewayReferenceWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var cellData = (EditGridCellData)((FrameworkElement)e.OriginalSource).DataContext;
            var model = (DownloadModel)cellData.RowData.Row;
            new FileWebGatewayReferenceWindow(model.File).Show();
        }

        private void SearchResultsTable_ColumnChooserStateChanged(object sender, RoutedEventArgs e)
        {
            ((SearchTabElementViewModel)DataContext).ColumnsIconModel.IsSelected = SearchResultsTable.IsColumnChooserVisible;
        }

        private void OpenPeersMap(object sender, RoutedEventArgs e)
        {
            var cellData = (EditGridCellData)((FrameworkElement)e.OriginalSource).DataContext;
            var model = (DownloadModel)cellData.RowData.Row;
            new PeersMapWindow(model.GeoPoints).Show();
        }

        private async void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var directoryViewModel = App.ServiceProvider.GetRequiredService<DirectoryViewModel>();
            var model = (DownloadModel)((FrameworkElement)e.OriginalSource).DataContext;
            await directoryViewModel.AddTab(model.File.NodeId);
            directoryViewModel.Navigate.Invoke();
            e.Handled = true;
        }

        private async void AddMergedDirectoryTab(object sender, RoutedEventArgs e)
        {
            var directoryViewModel = App.ServiceProvider.GetRequiredService<DirectoryViewModel>();
            var cellData = (EditGridCellData)((FrameworkElement)e.OriginalSource).DataContext;
            var model = (DownloadModel)cellData.RowData.Row;
            await directoryViewModel.AddMergedTab(model.File.Hash);
            directoryViewModel.Navigate.Invoke();
            e.Handled = true;
        }

        private async void ShareResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var dataContext = (SearchTabElementViewModel)((FrameworkElement)e.OriginalSource).DataContext;
            var fileModel = await dataContext.CreateResultsSnapshot();
            new ResultsSharingWindow(fileModel).Show();
        }

        private async void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            var dataContext = (DownloadModel)((FrameworkElement)e.OriginalSource).DataContext;
            var profileService = App.ServiceProvider.GetRequiredService<IProfileService>();
            var textBlock = sender as TextBlock;
            var textBlockTooltipElement = (FrameworkElement)textBlock.ToolTip;
            if (textBlockTooltipElement.DataContext is not User)
            {
                var user = await profileService.GetUser(dataContext.File.NodeId);
                textBlockTooltipElement.DataContext = user;
                textBlockTooltipElement.Visibility = Visibility.Visible;
            }
        }
    }
}