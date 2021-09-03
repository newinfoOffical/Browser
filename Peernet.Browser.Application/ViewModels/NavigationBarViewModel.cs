﻿using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class NavigationBarViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService navigationService;
        private bool isProfileMenuVisible;

        public bool IsProfileMenuVisible
        {
            get => isProfileMenuVisible;
            set
            {
                isProfileMenuVisible = value;
                RaisePropertyChanged(nameof(IsProfileMenuVisible));
            }
        }

        public List<MenuItemViewModel> Items { get; set; }

        public NavigationBarViewModel(IMvxNavigationService navigationService)
        {
            this.navigationService = navigationService;
            Items = new List<MenuItemViewModel>
            {
                new MenuItemViewModel("About"),
                new MenuItemViewModel("FAQ (Help)"),
                new MenuItemViewModel("Backup to a file")
            };
        }

        public IMvxAsyncCommand NavigateHomeCommand
        {
            get
            {
                return new MvxAsyncCommand(async () =>
                {
                    await navigationService.Navigate<HomeViewModel>();
                });
            }
        }

        public IMvxAsyncCommand NavigateExploreCommand
        {
            get
            {
                return new MvxAsyncCommand(async () =>
                {
                    await navigationService.Navigate<ExploreViewModel>();
                });
            }
        }

        public IMvxAsyncCommand NavigateDirectoryCommand
        {
            get
            {
                return new MvxAsyncCommand(async () =>
                {
                    await navigationService.Navigate<DirectoryViewModel>();
                });
            }
        }

        public IMvxAsyncCommand OpenCloseProfileMenuCommand
        {
            get
            {
                return new MvxAsyncCommand(() =>
                {
                    IsProfileMenuVisible ^= true;

                    return Task.CompletedTask;
                });
            }
        }
    }
}