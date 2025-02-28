// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMatchBeatmapDetailArea : OnlinePlayTestScene
    {
        private Room room = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create area", () =>
            {
                Child = new MatchBeatmapDetailArea(room = new Room())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500),
                    CreateNewItem = createNewItem
                };
            });
        }

        private void createNewItem()
        {
            room.Playlist = room.Playlist.Append(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
            {
                ID = room.Playlist.Count,
                RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                RequiredMods = new[]
                {
                    new APIMod(new OsuModHardRock()),
                    new APIMod(new OsuModDoubleTime()),
                    new APIMod(new OsuModAutoplay())
                }
            }).ToArray();
        }
    }
}
