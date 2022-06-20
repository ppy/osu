// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneHitCircleHidden : TestSceneHitCircle
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = new[] { new OsuModHidden() };
        });
    }
}
