// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests
{
    public abstract class LadderTestScene : OsuTestScene
    {
        [Resolved]
        protected LadderInfo Ladder { get; private set; }
    }
}
