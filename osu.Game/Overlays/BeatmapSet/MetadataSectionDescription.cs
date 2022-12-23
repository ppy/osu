// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class MetadataSectionDescription : MetadataSection
    {
        public MetadataSectionDescription(Action<string>? searchAction = null)
            : base(MetadataType.Description, searchAction)
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            loaded.AddText(metadata);
        }
    }
}
