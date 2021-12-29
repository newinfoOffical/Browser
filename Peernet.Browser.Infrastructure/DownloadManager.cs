﻿using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Application.Download;
using Peernet.Browser.Application.Managers;
using Peernet.Browser.Application.Utilities;
using Peernet.Browser.Infrastructure.Clients;
using Peernet.Browser.Models.Domain.Download;
using Peernet.Browser.Models.Presentation.Footer;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Peernet.Browser.Infrastructure
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IDownloadClient downloadClient;
        private readonly ISettingsManager settingsManager;

        public DownloadManager(ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            Directory.CreateDirectory(settingsManager.DownloadPath);
            downloadClient = new DownloadClient(settingsManager);

            // Fire on the thread-pool and forget
            Task.Run(UpdateStatuses);
        }

        public event EventHandler downloadsChanged;

        public ObservableCollection<DownloadModel> ActiveFileDownloads { get; set; } = new();

        public async Task<ApiResponseDownloadStatus> CancelDownload(string id)
        {
            var download = ActiveFileDownloads.First(d => d.Id == id);
            var responseStatus = await ExecuteDownload(download, DownloadAction.Cancel);

            switch (responseStatus.DownloadStatus)
            {
                case DownloadStatus.DownloadCanceled:
                    ActiveFileDownloads.Remove(download);
                    NotifyChange($"{download.File.Name} downloading canceled!");
                    break;

                case DownloadStatus.DownloadFinished:
                    ActiveFileDownloads.Remove(download);
                    downloadsChanged?.Invoke(this, EventArgs.Empty);
                    break;
            }

            return responseStatus;
        }

        public void OpenFileLocation(string name)
        {
            var filePath = Path.Combine(settingsManager.DownloadPath, name);
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", "/select, " + filePath);
            }
        }

        public async Task<ApiResponseDownloadStatus> PauseDownload(string id)
        {
            var download = ActiveFileDownloads.First(d => d.Id == id);
            return await ExecuteDownload(download, DownloadAction.Pause);
        }

        public async Task QueueUpDownload(DownloadModel downloadModel)
        {
            var status = await downloadClient.Start($"{settingsManager.DownloadPath}/{downloadModel.File.Name}", downloadModel.File.Hash, downloadModel.File.NodeId);
            downloadModel.Id = status.Id;
            downloadModel.Status = status.DownloadStatus;

            if (status.APIStatus == APIStatus.DownloadResponseSuccess)
            {
                ActiveFileDownloads.Add(downloadModel);
            }
            else
            {
                GlobalContext.Notifications.Add(new Notification(
                    $"Failed to start file download. Status: {status.APIStatus}",
                    MessagingHelper.GetInOutSummary(downloadModel.File, status), Severity.Error));
            }

            downloadsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<ApiResponseDownloadStatus> ResumeDownload(string id)
        {
            var download = ActiveFileDownloads.First(d => d.Id == id);
            return await ExecuteDownload(download, DownloadAction.Resume);
        }

        private async Task<ApiResponseDownloadStatus> ExecuteDownload(DownloadModel download, DownloadAction action)
        {
            var responseStatus = await downloadClient.GetAction(download.Id, action);
            download.Status = responseStatus.DownloadStatus;
            downloadsChanged?.Invoke(this, EventArgs.Empty);

            if (responseStatus.APIStatus != APIStatus.DownloadResponseSuccess)
            {
                GlobalContext.Notifications.Add(new Notification(
                    $"Failed to {action} file download. Status: {responseStatus.APIStatus}",
                    MessagingHelper.GetInOutSummary(download.Id, responseStatus), Severity.Error));
            }

            return responseStatus;
        }

        private void NotifyChange(string message)
        {
            downloadsChanged?.Invoke(this, EventArgs.Empty);
            GlobalContext.Notifications.Add(new Notification(message));
        }

        private async Task UpdateStatuses()
        {
            while (true)
            {
                var copy = ActiveFileDownloads.ToList();
                foreach (var download in copy.Where(download => !download.IsCompleted))
                {
                    var status = await downloadClient.GetStatus(download.Id);
                    if (status == null)
                    {
                        continue;
                    }

                    download.Progress = status.Progress.Percentage;
                    download.Status = status.DownloadStatus;

                    if (status.DownloadStatus == DownloadStatus.DownloadFinished)
                    {
                        download.IsCompleted = true;
                        download.Progress = 100;
                        await GlobalContext.UiThreadDispatcher.ExecuteOnMainThreadAsync(() =>
                            NotifyChange($"{download.File.Name} downloading completed!"));
                    }
                }

                await Task.Delay(1000);
            }
        }
    }
}