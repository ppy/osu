// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchHeader : OsuTestScene
    {
        private TestRoomContainer roomContainer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = roomContainer = new TestRoomContainer
            {
                Child = new Header()
            };
        });

        [Test]
        public void TestBasicRoom()
        {
            AddStep("set basic room", () =>
            {
                roomContainer.Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            Metadata = new BeatmapMetadata
                            {
                                Title = "Title",
                                Artist = "Artist",
                                AuthorString = "Author",
                            },
                            Version = "Version",
                            Ruleset = new OsuRuleset().RulesetInfo
                        }
                    },
                    RequiredMods =
                    {
                        new OsuModDoubleTime(),
                        new OsuModNoFail(),
                        new OsuModRelax(),
                    }
                });

                roomContainer.Room.Name.Value = "A very awesome room";
                roomContainer.Room.Host.Value = new User { Id = 2, Username = "peppy" };
            });
        }
    }
}
