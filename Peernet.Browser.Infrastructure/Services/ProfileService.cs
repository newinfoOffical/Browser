﻿using Peernet.Browser.Application.Services;
using Peernet.SDK.Client.Clients;
using Peernet.SDK.Models.Domain.Blockchain;
using Peernet.SDK.Models.Domain.Profile;
using Peernet.SDK.Models.Presentation.Profile;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Peernet.Browser.Infrastructure.Services
{
    internal class ProfileService : IProfileService
    {
        private readonly IProfileClient profileClient;

        public ProfileService(IProfileClient profileClient)
        {
            this.profileClient = profileClient;
        }

        public async Task DeleteUserImage()
        {
            await profileClient.DeleteUserImage();
        }

        public async Task<User> GetUser(byte[]? node = null)
        {
            ApiProfileData profileData = null;
            try
            {
                profileData = await profileClient.GetProfileData(node);
            }
            catch (Exception e)
            {
            }
            var name = profileData?.Fields?.FirstOrDefault(f => f.Type == ProfileField.ProfileFieldName)?.Text;
            var image = profileData?.Fields?.FirstOrDefault(f => f.Type == ProfileField.ProfilePicture)?.Blob;

            return new User
            {
                Name = string.IsNullOrEmpty(name) ? null : name,
                Image = image?.Length == 0 ? null : image
            };
        }

        public async Task<ApiBlockchainBlockStatus> UpdateUser(string name, byte[] image)
        {
            return await profileClient.UpdateUser(name, image);
        }
    }
}