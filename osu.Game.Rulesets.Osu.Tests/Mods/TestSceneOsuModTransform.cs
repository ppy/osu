// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModTransform : OsuModTestScene
    {
        [Test]
        public void TestRotateSetting()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModTransform
                {
                    Move = { Value = TransformMovementType.Rotate }
                },
                PassCondition = () => true
            });
        }

        [Test]
        public void TestRadiateSetting()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModTransform
                {
                    Move = { Value = TransformMovementType.Radiate }
                },
                PassCondition = () => true
            });
        }
    }
}
