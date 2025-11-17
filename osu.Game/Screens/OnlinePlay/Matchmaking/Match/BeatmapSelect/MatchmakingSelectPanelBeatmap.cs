// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelBeatmap : MatchmakingSelectPanel
    {
        private readonly APIBeatmap beatmap;
        private readonly Mod[] mods;

        public MatchmakingSelectPanelBeatmap(MatchmakingPlaylistItem item)
            : base(item.PlaylistItem)
        {
            beatmap = item.Beatmap;
            mods = item.Mods;
        }

        private CardContent content = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(content = new CardContentBeatmap(beatmap, mods));
        }

        public override void AddUser(APIUser user)
        {
            content.SelectionOverlay.AddUser(user);
        }

        public override void RemoveUser(APIUser user)
        {
            content.SelectionOverlay.RemoveUser(user.Id);
        }
    }
}
