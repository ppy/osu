// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class OverlinedPlaylistHeader : OverlinedHeader
    {
        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public OverlinedPlaylistHeader()
            : base("Playlist")
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged((_, _) => Details.Value = Playlist.GetTotalDuration(rulesets), true);
        }
    }
}
