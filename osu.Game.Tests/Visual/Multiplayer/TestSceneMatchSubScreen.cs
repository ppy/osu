// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Match;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK.Input;
using Header = osu.Game.Screens.Multi.Match.Components.Header;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSubScreen : MultiplayerTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Screens.Multi.Multiplayer),
            typeof(MatchSubScreen),
            typeof(Header),
            typeof(Footer)
        };

        [Cached(typeof(IRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private TestMatchSubScreen match;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room.CopyFrom(new Room());
        });

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load match", () => LoadScreen(match = new TestMatchSubScreen(Room)));
            AddUntilStep("wait for load", () => match.IsCurrentScreen());
        }

        [Test]
        public void TestPlaylistItemSelectedOnCreate()
        {
            AddStep("set room properties", () =>
            {
                Room.Name.Value = "my awesome room";
                Room.Host.Value = new User { Id = 2, Username = "peppy" };
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("move mouse to create button", () =>
            {
                var footer = match.ChildrenOfType<Footer>().Single();
                InputManager.MoveMouseTo(footer.ChildrenOfType<OsuButton>().Single());
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("first playlist item selected", () => match.SelectedItem.Value == Room.Playlist[0]);
        }

        private class TestMatchSubScreen : MatchSubScreen
        {
            public new Bindable<PlaylistItem> SelectedItem => base.SelectedItem;

            public TestMatchSubScreen(Room room)
                : base(room)
            {
            }
        }

        private class TestRoomManager : IRoomManager
        {
            public event Action RoomsUpdated
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            public IBindableList<Room> Rooms { get; } = new BindableList<Room>();

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            {
                room.RoomID.Value = 1;
                onSuccess?.Invoke(room);
            }

            public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => onSuccess?.Invoke(room);

            public void PartRoom()
            {
            }
        }
    }
}
