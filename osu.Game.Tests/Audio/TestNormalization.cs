// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Audio.Effects;

namespace osu.Game.Tests.Audio
{
    [TestFixture]
    public class TestNormalization
    {
        [Test]
        public void Main()
        {
            AudioNormalization audioNormalization = new AudioNormalization("/Users/smallketchup/Downloads/Origami Angel - Doctor Whomst.mp3");
        }
    }
}
