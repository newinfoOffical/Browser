﻿using AsyncAwaitBestPractices.MVVM;
using Peernet.Browser.Application.Download;
using Peernet.Browser.Application.Managers;
using Peernet.Browser.Application.Navigation;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Application.ViewModels.Parameters;
using Peernet.SDK.Common;
using Peernet.SDK.Models.Domain.Download;
using Peernet.SDK.Models.Domain.Status;
using Peernet.SDK.Models.Presentation.Footer;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class FooterViewModel : ViewModelBase
    {
        private const int reconnectDelay = 2000;
        private readonly IStatusService statusService;
        private readonly IApplicationManager applicationManager;
        private readonly IBlockchainService blockchainService;
        private readonly CurrentUserDirectoryViewModel currentUserDirectoryViewModel;
        private readonly IModalNavigationService modalNavigationService;
        private readonly INavigationService navigationService;
        private readonly INotificationsManager notificationsManager;
        private readonly ISettingsManager settingsManager;
        private readonly IWarehouseService warehouseService;
        private bool areDownloadsCollapsed;
        private string commandLineInput;
        private string commandLineOutput;
        private ConnectionStatus connectionStatus = ConnectionStatus.Offline;
        private int peers;
        private ObservableCollection<PeerStatus> peerStatuses;

        public FooterViewModel(
            IStatusService apiService,
            INavigationService navigationService,
            IModalNavigationService modalNavigationService,
            IApplicationManager applicationManager,
            IDownloadManager downloadManager,
            IWarehouseService warehouseService,
            IBlockchainService blockchainService,
            ISettingsManager settingsManager,
            INotificationsManager notificationsManager,
            DirectoryViewModel directoryViewModel)
        {
            this.statusService = apiService;
            this.navigationService = navigationService;
            this.modalNavigationService = modalNavigationService;
            this.applicationManager = applicationManager;
            this.warehouseService = warehouseService;
            this.blockchainService = blockchainService;
            this.settingsManager = settingsManager;
            this.notificationsManager = notificationsManager;
            this.currentUserDirectoryViewModel = directoryViewModel.CurrentUserDirectoryViewModel;

            DownloadManager = downloadManager;
            DownloadManager.downloadsChanged += GetLastDownloadItem;
            UploadCommand = new AsyncCommand(UploadFiles);
            DownloadManager.downloadsChanged += CollapseWhenSingleItem;

            Task.Run(ConnectToPeernetAPI).ConfigureAwait(false).GetAwaiter().GetResult();

            Task.Run(UpdateStatuses);
        }

        public bool AreDownloadsCollapsed
        {
            get => areDownloadsCollapsed;
            set
            {
                areDownloadsCollapsed = value;
                OnPropertyChanged(nameof(AreDownloadsCollapsed));
            }
        }

        public IAsyncCommand<string> CancelDownloadCommand => new AsyncCommand<string>(
            async id =>
            {
                // Make API call and validate result
                await DownloadManager.CancelDownload(id);
            });

        public IAsyncCommand CollapseExpandDownloadsCommand => new AsyncCommand(
            () =>
            {
                AreDownloadsCollapsed ^= true;
                return Task.CompletedTask;
            });

        public string CommandLineInput
        {
            get => commandLineInput;
            set
            {
                commandLineInput = value;
                OnPropertyChanged(nameof(CommandLineInput));
            }
        }

        public string CommandLineOutput
        {
            get => commandLineOutput;
            set
            {
                commandLineOutput = value;
                OnPropertyChanged(nameof(CommandLineOutput));
            }
        }

        public ConnectionStatus ConnectionStatus
        {
            get => connectionStatus;
            set
            {
                connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public IDownloadManager DownloadManager { get; }

        public ObservableCollection<DownloadModel> ListedFileDownloads { get; set; } = new();

        public IAsyncCommand<DownloadModel> OpenFileCommand => new AsyncCommand<DownloadModel>(
            model =>
            {
                if (model.Status == DownloadStatus.DownloadFinished)
                {
                    var path = Path.Combine(settingsManager.DownloadPath, model.File.Name);
                    OpenWithDefaultProgram(path);
                }

                return Task.CompletedTask;
            });

        public IAsyncCommand<string> OpenFileLocationCommand => new AsyncCommand<string>(
            name =>
            {
                DownloadManager.OpenFileLocation(name);

                return Task.CompletedTask;
            });

        public IAsyncCommand<string> PauseDownloadCommand => new AsyncCommand<string>(
            async id =>
            {
                // Make API call and validate result
                await DownloadManager.PauseDownload(id);
            });

        public int Peers
        {
            get => peers;
            set
            {
                peers = value;
                OnPropertyChanged(nameof(Peers));
            }
        }

        public ObservableCollection<PeerStatus> PeerStatuses
        {
            get => peerStatuses;
            set
            {
                peerStatuses = value;
                OnPropertyChanged(nameof(PeerStatuses));
            }
        }

        public IAsyncCommand<string> ResumeDownloadCommand => new AsyncCommand<string>(
            async id =>
            {
                // Make API call and validate result
                await DownloadManager.ResumeDownload(id);
            });

        public IAsyncCommand UploadCommand { get; }

        private static void OpenWithDefaultProgram(string path)
        {
            using var process = new Process();

            process.StartInfo.FileName = "explorer";
            process.StartInfo.Arguments = "\"" + path + "\"";
            process.Start();
        }

        private void CollapseWhenSingleItem(object sender, EventArgs e)
        {
            if (DownloadManager.ActiveFileDownloads.Count == 1)
            {
                AreDownloadsCollapsed = true;
            }
        }

        private async Task ConnectToPeernetAPI()
        {
            var success = false;
            while (!success)
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                success = await GetPeernetStatus();

                if (!success)
                {
                    await Task.Delay(reconnectDelay);
                }
            }
        }

        private void GetLastDownloadItem(object sender, EventArgs e)
        {
            var copy = ListedFileDownloads.ToList();
            copy.ForEach(i => ListedFileDownloads.Remove(i));
            ListedFileDownloads.Add(DownloadManager.ActiveFileDownloads.LastOrDefault());
        }

        private async Task<bool> GetPeernetStatus()
        {
            var status = await statusService.GetStatus();

            if (status == null)
            {
                return false;
            }

            ConnectionStatus = status.IsConnected ? ConnectionStatus.Online : ConnectionStatus.Offline;
            Peers = status.CountPeerList;
            return status.IsConnected;
        }

        private async Task UpdateStatuses()
        {
            while (true)
            {
                var status = await statusService.GetStatus();
                if (status != null)
                {
                    Peers = status.CountPeerList;
                    PeerStatuses = new (await statusService.GetPeersStatus());
                }

                await Task.Delay(3000);
            }
        }

        private async Task UploadFiles()
        {
            var fileModels = applicationManager.OpenFileDialog().Select(f => new FileModel(f)).ToList();

            if (fileModels.Count != 0)
            {
                var parameter = new ShareFileViewModelParameter(warehouseService, blockchainService, navigationService, notificationsManager, currentUserDirectoryViewModel)
                {
                    FileModels = fileModels
                };

                await modalNavigationService.Navigate<ShareFileViewModel, ShareFileViewModelParameter>(parameter);
            }
        }
    }
}