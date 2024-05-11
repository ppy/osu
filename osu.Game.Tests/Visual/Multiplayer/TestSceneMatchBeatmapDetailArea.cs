// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create area", () =>
            {
                SelectedRoom.Value = new Room();

                Child = new MatchBeatmapDetailArea
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
            SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
            {
                ID = SelectedRoom.Value.Playlist.Count,
                RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                RequiredMods = new[]
                {
                    new APIMod(new OsuModHardRock()),
                    new APIMod(new OsuModDoubleTime()),
                    new APIMod(new OsuModAutoplay())
                }
            });
        }
    }
}
