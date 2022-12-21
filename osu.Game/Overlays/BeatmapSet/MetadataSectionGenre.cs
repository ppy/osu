// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class MetadataSectionGenre : MetadataSection
    {
        public MetadataSectionGenre(Action<string>? searchAction = null)
            : base(MetadataType.Genre, searchAction)
        {
        }

        protected override void AddMetadata(string text, LinkFlowContainer loaded)
        {
            loaded.AddLink(text.DehumanizeTo<SearchGenre>().GetLocalisableDescription(), LinkAction.FilterBeatmapSetGenre, text);
        }
    }
}
