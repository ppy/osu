// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer.Queueing;

namespace osu.Game.Tests.Visual.Multiplayer.QueueingModes
{
    public class TestSceneFreeForAllQueueMode : QueueModeTestScene
    {
        protected override QueueModes Mode => QueueModes.FreeForAll;
    }
}
