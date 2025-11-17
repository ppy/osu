// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelRandom : MatchmakingSelectPanel
    {
        public MatchmakingSelectPanelRandom(MultiplayerPlaylistItem item)
            : base(item)
        {
        }

        private CardContent content = null!;
        private readonly List<APIUser> users = new List<APIUser>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(content = new CardContentRandom());
        }

        public void RevealBeatmap(APIBeatmap beatmap, Mod[] mods)
        {
            content.Expire();

            var flashLayer = new Box { RelativeSizeAxes = Axes.Both };

            AddRange(new Drawable[]
            {
                content = new CardContentBeatmap(beatmap, mods),
                flashLayer,
            });

            foreach (var user in users)
                content.SelectionOverlay.AddUser(user);

            flashLayer.FadeOutFromOne(1000, Easing.In);
        }

        public override void AddUser(APIUser user)
        {
            users.Add(user);
            content.SelectionOverlay.AddUser(user);
        }

        public override void RemoveUser(APIUser user)
        {
            users.Remove(user);
            content.SelectionOverlay.RemoveUser(user.Id);
        }
    }
}
