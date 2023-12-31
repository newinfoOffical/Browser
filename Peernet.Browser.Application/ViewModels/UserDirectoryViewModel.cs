﻿using AsyncAwaitBestPractices.MVVM;
using Peernet.Browser.Application.Interfaces;
using Peernet.Browser.Application.VirtualFileSystem;
using Peernet.SDK.Models.Domain.Search;
using Peernet.SDK.Models.Plugins;
using Peernet.SDK.Models.Presentation.Footer;
using Peernet.SDK.Models.Presentation.Profile;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class UserDirectoryViewModel : DirectoryTabViewModel, IShareableContent
    {
        private readonly string title;
        private Func<string, SearchResult, Task<FileModel>> createResultsSnapshot;
        private SearchResult searchResult;

        public UserDirectoryViewModel(
            User? user,
            string nodeId,
            SearchResult searchResult,
            Func<string, SearchResult, Task<FileModel>> createResultsSnapshot,
            Func<DirectoryTabViewModel, Task> removeTabAction,
            IVirtualFileSystemFactory virtualFileSystemFactory,
            IEnumerable<IPlayButtonPlug> playButtonPlugs)
            : base(
                  user,
                  nodeId,
                  virtualFileSystemFactory,
                  playButtonPlugs)
        {
            this.title = user?.Name ?? nodeId;
            this.searchResult = searchResult;
            this.createResultsSnapshot = createResultsSnapshot;

            Files = searchResult.Files;
            DeleteCommand = new AsyncCommand(async () => await removeTabAction(this));
            Initialize();
            InitializePath(VirtualFileSystem?.Home);
            OpenCommand.Execute(VirtualFileSystem?.Home);
        }

        public IAsyncCommand DeleteCommand { get; }

        public override bool IsReadOnly => true;

        public Task<FileModel> CreateResultsSnapshot() => createResultsSnapshot(title, searchResult);

        private void Initialize()
        {
            IsLoaded = false;
            CreateVirtualFileSystem();
            IsLoaded = true;
        }
    }
}