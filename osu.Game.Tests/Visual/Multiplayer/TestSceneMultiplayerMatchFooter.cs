// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerMatchFooter : MultiplayerTestScene
    {
        [Cached]
        private readonly OnlinePlayBeatmapAvailabilityTracker availablilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new MultiplayerMatchFooter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Height = 50
            };
        }
    }
}
