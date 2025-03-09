// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public interface IMultiplayerMatchScreen
    {
        Room Room { get; }

        bool IsCurrentScreen();

        void Push(IScreen screen);
    }
}
