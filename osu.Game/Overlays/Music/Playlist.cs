// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Music
{
    public partial class Playlist : VirtualisedListContainer<Live<BeatmapSetInfo>, PlaylistItem>
    {
        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public Playlist()
            : base(20, 50)
        {
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();
    }
}
