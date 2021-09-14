﻿using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Peernet.Browser.Application.Models;
using Peernet.Browser.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class DirectoryViewModel : MvxViewModel, ISearchable
    {
        private readonly IBlockchainService blockchainService;
        private bool _showSearchBox;
        private IReadOnlyCollection<ApiBlockRecordFile> sharedFiles;
        private string searchInput;
        private List<ApiBlockRecordFile> activeSearchResults;
        private bool showHint;

        public DirectoryViewModel(IBlockchainService blockchainService)
        {
            this.blockchainService = blockchainService;
        }

        public IMvxAsyncCommand<ApiBlockRecordFile> DeleteCommand =>
            new MvxAsyncCommand<ApiBlockRecordFile>(
                apiBlockRecordFile =>
            {
                blockchainService.DeleteSelfFile(apiBlockRecordFile);

                return Task.CompletedTask;
            });

        public IMvxAsyncCommand<ApiBlockRecordFile> EditCommand =>
            new MvxAsyncCommand<ApiBlockRecordFile>(
                apiBlockRecordFile =>
        {
            // Logic to be implemented

            return Task.CompletedTask;
        });

        public IMvxCommand RemoveHint
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (ShowHint)
                    {
                        ShowHint = false;
                        ShowSearchBox = true;
                    }
                });
            }
        }

        public IMvxCommand Search
        {
            get
            {
                return new MvxCommand(() =>
                {
                    if (!string.IsNullOrEmpty(SearchInput))
                    {
                        ActiveSearchResults = sharedFiles.Where(f => f.Name.Contains(SearchInput, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        ActiveSearchResults = sharedFiles.ToList();
                    }
                });
            }
        }

        public string SearchInput
        {
            get => searchInput;
            set
            {
                searchInput = value;
                RaisePropertyChanged(nameof(SearchInput));
            }
        }

        public IMvxAsyncCommand<ApiBlockRecordFile> ShareCommand =>
                                    new MvxAsyncCommand<ApiBlockRecordFile>(
                apiBlockRecordFile =>
        {
            // Logic to be implemented

            return Task.CompletedTask;
        });

        public List<ApiBlockRecordFile> ActiveSearchResults
        {
            get => activeSearchResults;
            set
            {
                activeSearchResults = value;
                RaisePropertyChanged(nameof(ActiveSearchResults));
            }
        }

        public bool ShowHint
        {
            get => showHint;
            set
            {
                SetProperty(ref showHint, value);
            }
        }

        public bool ShowSearchBox
        {
            get => _showSearchBox;
            set
            {
                SetProperty(ref _showSearchBox, value);
            }
        }

        public override Task Initialize()
        {
            var header = blockchainService.GetSelfHeader();
            if (header.Height > 0)
            {
                sharedFiles = blockchainService.GetSelfList().Files;
                ActiveSearchResults = sharedFiles.ToList();
            }

            return base.Initialize();
        }
    }
}