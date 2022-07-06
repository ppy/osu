// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Mods
{
    public class ModSettingsTest
    {
        [Test]
        public void TestModSettingsUnboundWhenCopied()
        {
            var original = new OsuModDoubleTime();
            var copy = (OsuModDoubleTime)original.DeepClone();

            original.SpeedChange.Value = 2;

            Assert.That(original.SpeedChange.Value, Is.EqualTo(2.0));
            Assert.That(copy.SpeedChange.Value, Is.EqualTo(1.5));
        }

        [Test]
        public void TestMultiModSettingsUnboundWhenCopied()
        {
            var original = new MultiMod(new OsuModDoubleTime());
            var copy = (MultiMod)original.DeepClone();

            ((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value = 2;

            Assert.That(((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value, Is.EqualTo(2.0));
            Assert.That(((OsuModDoubleTime)copy.Mods[0]).SpeedChange.Value, Is.EqualTo(1.5));
        }
    }
}
