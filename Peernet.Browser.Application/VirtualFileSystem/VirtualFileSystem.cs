﻿using Peernet.Browser.Models.Domain.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Peernet.Browser.Application.VirtualFileSystem
{
    public class VirtualFileSystem
    {
        private readonly IFilesToCategoryBinder binder;

        public VirtualFileSystem(IEnumerable<ApiFile> sharedFiles, IFilesToCategoryBinder binder, bool isCurrentSelection = true)
        {
            this.binder = binder;

            CreateRootCoreTier(sharedFiles, isCurrentSelection);
        }

        public VirtualFileSystemCoreTier Root => VirtualFileSystemTiers.First(t => t.Name == "Root");

        public ObservableCollection<VirtualFileSystemCoreCategory> VirtualFileSystemCategories { get; set; } = new();

        public ObservableCollection<VirtualFileSystemCoreTier> VirtualFileSystemTiers { get; set; } = new();

        public void ResetSelection()
        {
            VirtualFileSystemTiers.Foreach(e => e.ResetSelection());
            VirtualFileSystemCategories.Foreach(e => e.ResetSelection());
        }

        public VirtualFileSystemCoreEntity GetCurrentlySelected()
        {
            IEnumerable<VirtualFileSystemCoreEntity> allEntities =
                new List<VirtualFileSystemCoreEntity>(VirtualFileSystemTiers).Concat(VirtualFileSystemCategories);
            VirtualFileSystemCoreEntity selected = null;
            foreach (var tier in allEntities)
            {
                selected = tier.GetSelected();
                if (selected != null)
                {
                    return selected;
                }
            }

            return selected;
        }

        private void AddFileToTheSystem(VirtualFileSystemEntity candidateEntity, List<VirtualFileSystemEntity> sameLevelFileSystemTiers)
        {
            var candidateCoreTier = candidateEntity as VirtualFileSystemCoreTier;
            if (candidateCoreTier is null)
            {
                sameLevelFileSystemTiers.Add(candidateEntity);
                return;
            }
            var matchingTierThatIsAlreadyInTheFileSystem = sameLevelFileSystemTiers.FirstOrDefault(t => t.Name == candidateCoreTier.Name) as VirtualFileSystemCoreTier;
            if (matchingTierThatIsAlreadyInTheFileSystem == null)
            {
                sameLevelFileSystemTiers.Add(candidateCoreTier);
            }
            else
            {
                var candidateTierDescendant =
                    candidateCoreTier.VirtualFileSystemEntities.FirstOrDefault(e => e is VirtualFileSystemCoreTier);
                if (candidateTierDescendant == null)
                {
                    matchingTierThatIsAlreadyInTheFileSystem.VirtualFileSystemEntities.Add(candidateCoreTier.VirtualFileSystemEntities.First());
                    return;
                }

                AddFileToTheSystem(candidateTierDescendant as VirtualFileSystemCoreTier, matchingTierThatIsAlreadyInTheFileSystem.VirtualFileSystemEntities);
            }
        }

        private void CreateRootCoreTier(IEnumerable<ApiFile> sharedFiles, bool isCurrentSelection)
        {
            // materialize
            var sharedFilesList = sharedFiles.ToList();
            var rootTier = new VirtualFileSystemCoreTier(nameof(Root), VirtualFileSystemEntityType.Directory, nameof(Root))
            {
                IsSelected = isCurrentSelection
            };

            foreach (var coreTier in sharedFilesList.Select(StructureTheFile))
            {
                AddFileToTheSystem(coreTier, rootTier.VirtualFileSystemEntities);
            }

            VirtualFileSystemTiers.Add(rootTier);

            VirtualFileSystemCategories = new ObservableCollection<VirtualFileSystemCoreCategory>(binder.Bind(sharedFilesList));
        }

        private VirtualFileSystemEntity StructureTheFile(ApiFile file)
        {
            if (file.Folder.IsNullOrEmpty())
            {
                return new VirtualFileSystemEntity(file);
            }

            var directories = file.Folder.Split('/', '\\').ToList();
            directories.RemoveAll(string.IsNullOrEmpty);
            var totalDepth = directories.Count;

            VirtualFileSystemCoreTier coreTier = null;
            VirtualFileSystemCoreTier higherTier = null;
            for (int i = 0; i < totalDepth; i++)
            {
                var absolutePath = Path.Combine(directories.Take(i + 1).ToArray());
                var tier = new VirtualFileSystemCoreTier(directories[i], VirtualFileSystemEntityType.Directory, absolutePath);

                if (coreTier == null)
                {
                    coreTier = tier;
                    higherTier = coreTier;
                }
                else
                {
                    higherTier.VirtualFileSystemEntities.Add(tier);
                    higherTier = tier;
                }

                if (i == totalDepth - 1)
                {
                    var entity = new VirtualFileSystemEntity(file);
                    tier.VirtualFileSystemEntities.Add(entity);
                }
            }

            return coreTier;
        }

        public VirtualFileSystemCoreEntity Find(VirtualFileSystemCoreEntity selected, IEnumerable<VirtualFileSystemEntity> entities)
        {
            VirtualFileSystemCoreEntity matchingEntity = null;
            if (selected != null)
            {
                foreach (var tier in entities)
                {
                    var coreEntity = tier as VirtualFileSystemCoreEntity;
                    if (coreEntity == null)
                    {
                        continue;
                    }

                    if (coreEntity.AbsolutePath == selected.AbsolutePath)
                    {
                        matchingEntity = coreEntity;
                        break;
                    }

                    if (!coreEntity.VirtualFileSystemEntities.IsNullOrEmpty())
                    {
                        matchingEntity = Find(selected, coreEntity.VirtualFileSystemEntities);
                        if (matchingEntity != null)
                        {
                            return matchingEntity;
                        }
                    }
                }
            }

            return matchingEntity;
        }
    }
}