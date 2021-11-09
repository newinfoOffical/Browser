﻿using System;
using Peernet.Browser.Application.Managers;
using Peernet.Browser.Application.Services;
using Peernet.Browser.Infrastructure.Clients;
using Peernet.Browser.Models.Domain.Blockchain;
using Peernet.Browser.Models.Domain.Common;
using Peernet.Browser.Models.Presentation.Footer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peernet.Browser.Infrastructure.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly IBlockchainClient blockchainClient;

        public BlockchainService(ISettingsManager settingsManager)
        {
            blockchainClient = new BlockchainClient(settingsManager);
        }

        // todo: it should consume some presentation model
        public async Task DeleteFile(ApiFile apiFile)
        {
            await blockchainClient.DeleteFile(apiFile);
        }

        public async Task<ApiBlockchainBlockStatus> UpdateFile(FileModel fileModel)
        {
            var apiFile = new ApiFile
            {
                Id = fileModel.Id,
                Hash = fileModel.Hash,
                MetaData = fileModel.Metadata,
                NodeId = fileModel.NodeId,
                Name = fileModel.FileName,
                Description = fileModel.Description,
                Folder = fileModel.Directory,
                Date = fileModel.CreateDate,
                Type = fileModel.Type,
                Format = fileModel.Format
            };

            return await blockchainClient.UpdateFile(apiFile);
        }

        public async Task<ApiBlockchainHeader> GetHeader()
        {
            return await blockchainClient.GetHeader();
        }

        public async Task<List<ApiFile>> GetList()
        {
            return (await blockchainClient.GetList()).Files;
        }

        public async Task AddFiles(IEnumerable<FileModel> files)
        {
            var data = files
                .Select(x =>
                    new ApiFile
                    {
                        Description = x.Description ?? string.Empty,
                        Name = x.FileName,
                        Folder = x.Directory,
                        Date = DateTime.Now,
                        Hash = x.Hash,
                        MetaData = new List<ApiFileMetadata>()
                    })
                .ToList();

            await blockchainClient.AddFiles(new ApiBlockchainAddFiles { Files = data });
        }
    }
}