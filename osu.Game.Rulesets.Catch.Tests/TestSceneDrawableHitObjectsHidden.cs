// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Mods;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneDrawableHitObjectsHidden : TestSceneDrawableHitObjects
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = new[] { new CatchModHidden() };
        });
    }
}
