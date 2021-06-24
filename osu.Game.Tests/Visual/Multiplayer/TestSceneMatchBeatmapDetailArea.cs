// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchBeatmapDetailArea : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        private TestRoomContainer roomContainer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = roomContainer = new TestRoomContainer
            {
                Child = new MatchBeatmapDetailArea
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500),
                    CreateNewItem = createNewItem
                }
            };
        });

        private void createNewItem()
        {
            roomContainer.Room.Playlist.Add(new PlaylistItem
            {
                ID = roomContainer.Room.Playlist.Count,
                Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                Ruleset = { Value = new OsuRuleset().RulesetInfo },
                RequiredMods =
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime(),
                    new OsuModAutoplay()
                }
            });
        }
    }
}
