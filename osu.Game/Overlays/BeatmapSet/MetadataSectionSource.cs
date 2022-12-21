// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class MetadataSectionSource : MetadataSection
    {
        public MetadataSectionSource(Action<string>? searchAction = null)
            : base(MetadataType.Source, searchAction)
        {
        }

        protected override void AddMetadata(string text, LinkFlowContainer loaded)
        {
            if (SearchAction != null)
                loaded.AddLink(text, () => SearchAction(text));
            else
                loaded.AddLink(text, LinkAction.SearchBeatmapSet, text);
        }
    }
}
