// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestBarLineApplication : HitObjectApplicationTestScene
    {
        [Test]
        public void TestApplyNewBarLine()
        {
            DrawableBarLine barLine = null;

            AddHitObject(() => barLine = new DrawableBarLine(PrepareObject(new BarLine
            {
                StartTime = 400,
                Major = true
            })));

            RemoveHitObject(() => barLine);

            AddStep("apply new bar line", () => barLine.Apply(PrepareObject(new BarLine
            {
                StartTime = 200,
                Major = false
            }), null));

            AddHitObject(() => barLine);
        }
    }
}
