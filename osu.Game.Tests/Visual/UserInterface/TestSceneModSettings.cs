// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModSettings : OsuManualInputManagerTestScene
    {
        [Test]
        public void TestModSettingsUnboundWhenCopied()
        {
            OsuModDoubleTime original = null;
            OsuModDoubleTime copy = null;

            AddStep("create mods", () =>
            {
                original = new OsuModDoubleTime();
                copy = (OsuModDoubleTime)original.DeepClone();
            });

            AddStep("change property", () => original.SpeedChange.Value = 2);

            AddAssert("original has new value", () => Precision.AlmostEquals(2.0, original.SpeedChange.Value));
            AddAssert("copy has original value", () => Precision.AlmostEquals(1.5, copy.SpeedChange.Value));
        }

        [Test]
        public void TestMultiModSettingsUnboundWhenCopied()
        {
            MultiMod original = null;
            MultiMod copy = null;

            AddStep("create mods", () =>
            {
                original = new MultiMod(new OsuModDoubleTime());
                copy = (MultiMod)original.DeepClone();
            });

            AddStep("change property", () => ((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value = 2);

            AddAssert("original has new value", () => Precision.AlmostEquals(2.0, ((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value));
            AddAssert("copy has original value", () => Precision.AlmostEquals(1.5, ((OsuModDoubleTime)copy.Mods[0]).SpeedChange.Value));
        }
    }
}
