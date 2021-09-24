﻿using System.Collections.Generic;
using Peernet.Browser.Application.Models;

namespace Peernet.Browser.Application.VirtualFileSystem
{
    public class VirtualFileSystemTier : VirtualFileSystemEntity
    {
        public VirtualFileSystemTier(string name, VirtualFileSystemEntityType type, int depth)
            : base(name, type, new List<ApiBlockRecordFile>())
        {
            Depth = depth;
        }

        public int Depth { get; }

        public List<VirtualFileSystemTier> VirtualFileSystemTiers { get; set; } = new();

        public override void ResetSelection()
        {
            VirtualFileSystemTiers.ForEach(t => t.ResetSelection());

            base.ResetSelection();
        }
    }
}
