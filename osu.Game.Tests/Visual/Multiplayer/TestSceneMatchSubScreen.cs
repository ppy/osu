// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Match;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSubScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Screens.Multi.Multiplayer),
            typeof(MatchSubScreen),
            typeof(Header),
            typeof(Footer)
        };

        [Cached]
        private readonly Bindable<Room> currentRoom = new Bindable<Room>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        public TestSceneMatchSubScreen()
        {
            currentRoom.Value = new Room();
        }

        [Test]
        public void TestShowRoom()
        {
            AddStep(@"show", () =>
            {
                currentRoom.Value.RoomID.Value = 1;
                currentRoom.Value.Availability.Value = RoomAvailability.Public;
                currentRoom.Value.Duration.Value = TimeSpan.FromHours(24);
                currentRoom.Value.Host.Value = new User { Username = "peppy", Id = 2 };
                currentRoom.Value.Name.Value = "super secret room";
                currentRoom.Value.Participants.AddRange(new[]
                {
                    new User { Username = "peppy", Id = 2 },
                    new User { Username = "smoogipoo", Id = 1040328 }
                });
                currentRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = beatmaps.GetAllUsableBeatmapSets()[0].Beatmaps[0] },
                    Ruleset = { Value = rulesets.GetRuleset(2) },
                });

                LoadScreen(new MatchSubScreen(currentRoom.Value));
            });
        }

        [Test]
        public void TestShowSettings()
        {
            AddStep(@"show", () =>
            {
                currentRoom.Value.RoomID.Value = null;
                LoadScreen(new MatchSubScreen(currentRoom.Value));
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(currentRoom);
            return dependencies;
        }
    }
}
