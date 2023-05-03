﻿using Peernet.Browser.Application.Managers;
using Peernet.Browser.Application.Navigation;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Application.VirtualFileSystem;
using Peernet.SDK.Models.Domain.Common;
using Peernet.SDK.Models.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peernet.Browser.Application.ViewModels
{
    public class CurrentUserDirectoryViewModel : DirectoryTabViewModel
    {
        public CurrentUserDirectoryViewModel(
            IBlockchainService blockchainService,
            IVirtualFileSystemFactory virtualFileSystemFactory,
            IModalNavigationService modalNavigationService,
            INotificationsManager notificationsManager,
            IEnumerable<IPlayButtonPlug> playButtonPlugs)
            : base("My Directory", async () => await GetFiles(blockchainService), blockchainService, virtualFileSystemFactory, modalNavigationService, notificationsManager, playButtonPlugs)
        {
        }

        private async static Task<List<ApiFile>> GetFiles(IBlockchainService blockchainService)
        {
            var header = await blockchainService.GetHeader();
            if (header == null)
            {
                return new();
            }

            return await blockchainService.GetList();
        }
    }
}