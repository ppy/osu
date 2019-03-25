// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestCaseSkipOverlay : OsuTestCase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new SkipOverlay(Clock.CurrentTime + 5000));
        }
    }
}
