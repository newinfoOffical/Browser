﻿using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Models;
using Peernet.Browser.Models.Presentation;

namespace Peernet.Browser.Application.ViewModels
{
    public class FiltersViewModel : MvxViewModel<FiltersModel>
    {
        private FiltersModel filters;

        public FiltersModel Filters
        {
            get => filters;
            set => SetProperty(ref filters, value);
        }

        public override void Prepare(FiltersModel p)
        {
            Filters = p;
            p.CloseAction = Hide;
        }

        private readonly IMvxNavigationService mvxNavigationService;

        public FiltersViewModel(IMvxNavigationService mvxNavigationService)
        {
            this.mvxNavigationService = mvxNavigationService;
        }

        private void Hide()
        {
            GlobalContext.IsMainWindowActive = true;
            mvxNavigationService.Close(this);
        }
    }
}