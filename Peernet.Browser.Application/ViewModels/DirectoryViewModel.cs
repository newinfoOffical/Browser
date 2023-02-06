﻿using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Application.Managers;
using Peernet.Browser.Application.Navigation;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Application.VirtualFileSystem;
using Peernet.SDK.Models.Plugins;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class DirectoryViewModel : ViewModelBase
    {
        public ObservableCollection<DirectoryTabViewModel> DirectoryTabs { get; set; }

        private readonly IBlockchainService blockchainService;
        private readonly IModalNavigationService modalNavigationService;
        private readonly INotificationsManager notificationsManager;
        private readonly IEnumerable<IPlayButtonPlug> playButtonPlugs;
        private readonly IVirtualFileSystemFactory virtualFileSystemFactory;
        private int selectedIndex = 0;

        public CurrentUserDirectoryViewModel CurrentUserDirectoryViewModel { get; private set; }

        public DirectoryTabViewModel Content => SelectedIndex < 0 ? null : DirectoryTabs[SelectedIndex];

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

        public DirectoryViewModel(
            IBlockchainService blockchainService,
            IVirtualFileSystemFactory virtualFileSystemFactory,
            IModalNavigationService modalNavigationService,
            INotificationsManager notificationsManager,
            IEnumerable<IPlayButtonPlug> playButtonPlugs)
        {
            this.blockchainService = blockchainService;
            this.virtualFileSystemFactory = virtualFileSystemFactory;
            this.modalNavigationService = modalNavigationService;
            this.notificationsManager = notificationsManager;
            this.playButtonPlugs = playButtonPlugs;

            CurrentUserDirectoryViewModel = new CurrentUserDirectoryViewModel(blockchainService, CloseTab, virtualFileSystemFactory, modalNavigationService, notificationsManager, playButtonPlugs);
            DirectoryTabs = new ObservableCollection<DirectoryTabViewModel>(new List<DirectoryTabViewModel> { CurrentUserDirectoryViewModel });
        }

        public async Task AddTab(string nodeId)
        {
            UserDirectoryViewModel tab = new(nodeId, blockchainService, CloseTab, virtualFileSystemFactory, modalNavigationService, notificationsManager, playButtonPlugs);
            DirectoryTabs.Add(tab);
        }

        public Task CloseTab(DirectoryTabViewModel tab)
        {
            DirectoryTabs.Remove(tab);

            return Task.CompletedTask;
        }
    }
}