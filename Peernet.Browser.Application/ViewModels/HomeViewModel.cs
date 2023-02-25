﻿using AsyncAwaitBestPractices.MVVM;
using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Application.Download;
using Peernet.Browser.Application.Navigation;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Application.ViewModels.Parameters;
using Peernet.Browser.Application.Widgets;
using Peernet.SDK.Models.Extensions;
using Peernet.SDK.Models.Plugins;
using Peernet.SDK.Models.Presentation.Footer;
using Peernet.SDK.Models.Presentation.Home;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly IDownloadManager downloadManager;
        private readonly IModalNavigationService modalNavigationService;
        private readonly INavigationService navigationService;
        private readonly IEnumerable<IPlayButtonPlug> playButtonPlugs;
        private readonly ISearchService searchService;
        private readonly IWidgetsService widgetsService;
        private bool filtersActive;
        private string searchInput;
        private int selectedIndex = -1;

        public HomeViewModel(
            IWidgetsService widgetsService,
            ISearchService searchService,
            IDownloadManager downloadManager,
            INavigationService navigationService,
            IModalNavigationService modalNavigationService,
            IEnumerable<IPlayButtonPlug> playButtonPlugs)
        {
            this.widgetsService = widgetsService;
            this.searchService = searchService;
            this.downloadManager = downloadManager;
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.modalNavigationService = modalNavigationService ?? throw new ArgumentNullException(nameof(modalNavigationService));
            this.playButtonPlugs = playButtonPlugs;

            SearchCommand = new AsyncCommand(Search);
            Tabs.CollectionChanged += (o, s) =>
            {
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(IsNotVisible));
                OnPropertyChanged(nameof(Alignment));
                GlobalContext.IsLogoVisible = IsVisible;
            };
        }

        public AdvancedFilterModel AdvancedFilter { get; set; } = new();

        public Alignments Alignment => IsVisible ? Alignments.Stretch : Alignments.Center;

        public SearchTabElementViewModel Content => SelectedIndex < 0 ? null : Tabs[SelectedIndex];

        public bool FiltersActive
        {
            get => filtersActive;
            set
            {
                filtersActive = value;
                OnPropertyChanged(nameof(FiltersActive));
            }
        }

        public bool IsNotVisible => !IsVisible;

        public bool IsVisible => Tabs.Any();

        public IAsyncCommand OpenAdvancedOptionsCommand =>
            new AsyncCommand(async () =>
            {
                await modalNavigationService.Navigate<AdvancedSearchOptionsViewModel, AdvancedFilterModel>(AdvancedFilter);
            });

        public IAsyncCommand SearchCommand { get; }

        public string SearchInput
        {
            get => searchInput;
            set
            {
                searchInput = value;
                OnPropertyChanged(nameof(SearchInput));
            }
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
                OnPropertyChanged(nameof(Content));
            }
        }

        public ObservableCollection<SearchTabElementViewModel> Tabs { get; } = new ObservableCollection<SearchTabElementViewModel>();

        public SearchFilterResultModel CreateNewSearchFilter()
        {
            return new SearchFilterResultModel
            {
                InputText = SearchInput,
                AdvancedFilter = AdvancedFilter
            };
        }

        public async Task RemoveTab(SearchTabElementViewModel e)
        {
            await searchService.Terminate(e.Filters.UuId);
            Tabs.Remove(e);
            SelectedIndex = IsVisible ? 0 : -1;
        }

        private bool DoesSupportPlaying(DownloadModel model)
        {
            return playButtonPlugs.Any(plugin => plugin?.IsSupported(model.File) == true);
        }

        private async Task DownloadFile(DownloadModel model)
        {
            await downloadManager.QueueUpDownload(model);
        }

        private void ExecutePlayButtonPlug(DownloadModel model)
        {
            playButtonPlugs.Foreach(async plug =>
            {
                if (plug?.IsSupported(model.File) == true)
                {
                    await plug?.Execute(model.File);
                }
            });
        }

        private void OpenFile(DownloadModel model)
        {
            var param = new FilePreviewViewModelParameter(model.File, async () => await downloadManager.QueueUpDownload(model), "Download");
            navigationService.Navigate<FilePreviewViewModel, FilePreviewViewModelParameter>(param);
        }

        private Task Search()
        {
            var toAdd = new SearchTabElementViewModel(widgetsService, CreateNewSearchFilter(), RemoveTab, searchService.Search, DownloadFile, OpenFile, ExecutePlayButtonPlug, DoesSupportPlaying);

            Tabs.Add(toAdd);
            SelectedIndex = Tabs.Count - 1;
            SearchInput = string.Empty;

            return Task.CompletedTask;
        }
    }
}