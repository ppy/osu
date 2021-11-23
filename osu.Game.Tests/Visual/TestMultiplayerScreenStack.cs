// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// An <see cref="OsuScreen"/> loadable into <see cref="ScreenTestScene"/>s via <see cref="ScreenTestScene.LoadScreen"/>,
    /// which provides dependencies for and loads an isolated <see cref="Screens.OnlinePlay.Multiplayer.Multiplayer"/> screen.
    /// <p>
    /// This screen:
    /// <list type="bullet">
    /// <item>Provides a <see cref="TestMultiplayerClient"/> to be resolved as a dependency in the <see cref="Screens.OnlinePlay.Multiplayer.Multiplayer"/> screen,
    /// which is typically a part of <see cref="OsuGameBase"/>.</item>
    /// <item>Rebinds the <see cref="DummyAPIAccess"/> to handle requests via a <see cref="TestRoomRequestsHandler"/>.</item>
    /// <item>Provides a <see cref="TestMultiplayerRoomManager"/> for the <see cref="Screens.OnlinePlay.Multiplayer.Multiplayer"/> screen.</item>
    /// </list>
    /// </p>
    /// </summary>
    public class TestMultiplayerScreenStack : OsuScreen
    {
        public Screens.OnlinePlay.Multiplayer.Multiplayer MultiplayerScreen => multiplayerScreen;

        public TestMultiplayerRoomManager RoomManager => multiplayerScreen.RoomManager;

        public IScreen CurrentScreen => screenStack.CurrentScreen;

        public new bool IsLoaded => base.IsLoaded && MultiplayerScreen.IsLoaded;

        [Cached(typeof(MultiplayerClient))]
        public readonly TestMultiplayerClient Client;

        private readonly OsuScreenStack screenStack;
        private readonly TestMultiplayer multiplayerScreen;

        public TestMultiplayerScreenStack()
        {
            multiplayerScreen = new TestMultiplayer();

            InternalChildren = new Drawable[]
            {
                Client = new TestMultiplayerClient(RoomManager),
                screenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both }
            };

            screenStack.Push(multiplayerScreen);
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuGameBase game)
        {
            ((DummyAPIAccess)api).HandleRequest = request => multiplayerScreen.RequestsHandler.HandleRequest(request, api.LocalUser.Value, game);
        }

        public override bool OnBackButton() => multiplayerScreen.OnBackButton();

        public override bool OnExiting(IScreen next)
        {
            if (screenStack.CurrentScreen == null)
                return base.OnExiting(next);

            screenStack.Exit();
            return true;
        }

        private class TestMultiplayer : Screens.OnlinePlay.Multiplayer.Multiplayer
        {
            public new TestMultiplayerRoomManager RoomManager { get; private set; }
            public TestRoomRequestsHandler RequestsHandler { get; private set; }

            protected override RoomManager CreateRoomManager() => RoomManager = new TestMultiplayerRoomManager(RequestsHandler = new TestRoomRequestsHandler());
        }
    }
}
