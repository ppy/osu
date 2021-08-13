// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsFilterControl : OsuTestScene
    {
        public TestScenePlaylistsFilterControl()
        {
            Child = new PlaylistsFilterControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Width = 0.7f,
                Height = 80,
            };
        }
    }
}
