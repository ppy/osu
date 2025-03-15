// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomFreestyleSelect : OnlinePlayFreestyleSelect
    {
        public new readonly Bindable<BeatmapInfo?> Beatmap = new Bindable<BeatmapInfo?>();
        public new readonly Bindable<RulesetInfo?> Ruleset = new Bindable<RulesetInfo?>();

        public PlaylistsRoomFreestyleSelect(Room room, PlaylistItem item)
            : base(room, item)
        {
        }

        protected override bool OnStart()
        {
            // Beatmaps without a valid online ID are filtered away; this is just a final safety.
            if (base.Beatmap.Value.BeatmapInfo.OnlineID < 0)
                return false;

            if (base.Ruleset.Value.OnlineID < 0)
                return false;

            Beatmap.Value = base.Beatmap.Value.BeatmapInfo;
            Ruleset.Value = base.Ruleset.Value;
            this.Exit();
            return true;
        }
    }
}
