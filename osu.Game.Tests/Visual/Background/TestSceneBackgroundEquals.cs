// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;

namespace osu.Game.Tests.Visual.Background
{
    [TestFixture]
    public partial class TestSceneBackgroundEquals : OsuTestScene
    {
        [Test]
        public void TestEqualsNull()
        {
            var bg1 = new osu.Game.Graphics.Backgrounds.Background("texture1");
            Assert.IsFalse(bg1.Equals(null), "TestEqualsNull failed");
        }

        [Test]
        public void TestEqualsSameInstance()
        {
            var bg1 = new osu.Game.Graphics.Backgrounds.Background("texture1");
            Assert.IsTrue(bg1.Equals(bg1), "TestEqualsSameInstance failed");
        }

        [Test]
        public void TestEqualsDifferentType()
        {
            var bg1 = new osu.Game.Graphics.Backgrounds.Background("texture1");
            var differentTypeObject = new DifferentBackgroundType();
            Assert.IsFalse(bg1.Equals(differentTypeObject), "TestEqualsDifferentType failed");
        }

        [Test]
        public void TestEqualsSameTypeDifferentTexture()
        {
            var bg1 = new osu.Game.Graphics.Backgrounds.Background("texture1");
            var bg2 = new osu.Game.Graphics.Backgrounds.Background("texture2");
            Assert.IsFalse(bg1.Equals(bg2), "TestEqualsSameTypeDifferentTexture failed");
        }

        [Test]
        public void TestEqualsSameTypeSameTexture()
        {
            var bg1 = new osu.Game.Graphics.Backgrounds.Background("texture1");
            var bg2 = new osu.Game.Graphics.Backgrounds.Background("texture1");
            Assert.IsTrue(bg1.Equals(bg2), "TestEqualsSameTypeSameTexture failed");
        }
    }

    public partial class DifferentBackgroundType : osu.Game.Graphics.Backgrounds.Background
    {
        public DifferentBackgroundType() : base("texture")
        {
        }
    }
}
