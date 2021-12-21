﻿using System;
using System.IO;
using System.Threading.Tasks;
using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Application.Managers;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Infrastructure.Clients;
using Peernet.Browser.Models.Domain.Common;
using Peernet.Browser.Models.Domain.Warehouse;
using Peernet.Browser.Models.Presentation.Footer;

namespace Peernet.Browser.Infrastructure.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseClient warehouseClient;
        private readonly ISettingsManager settingsManager;

        public WarehouseService(ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            warehouseClient = new WarehouseClient(settingsManager);
        }

        public async Task<WarehouseResult> Create(FileModel file)
        {
            var content = await File.ReadAllBytesAsync(file.FullPath, default);
            
            return await warehouseClient.Create(content);
        }

        public async Task<WarehouseResult> ReadPath(ApiFile file)
        {
            var fullPath = Path.Combine(Environment.ExpandEnvironmentVariables(settingsManager.DownloadPath),
                file.Name);
            var result = await warehouseClient.ReadPath(file.Hash, fullPath);
            GlobalContext.Notifications.Add(result.Status != WarehouseStatus.StatusOK
                ? new Notification($"Failed to save file to {fullPath}. Status: {result.Status}", severity: Severity.Error)
                : new Notification($"File saved to {fullPath}"));

            return result;
        }
    }
}