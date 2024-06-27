// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    [TestFixture]
    public class OnChangeDirectionTests
    {
        private DefaultHitExplosion hitExplosion;

        [SetUp]
        public void SetUp()
        {
            hitExplosion = new DefaultHitExplosion();
        }

        [Test]
        public void TestOnDirectionChanged_Up()
        {
            // Arrange
            var directionBindable = new Bindable<ScrollingDirection>();
            directionBindable.Value = ScrollingDirection.Up;

            // Act
            hitExplosion.onDirectionChanged(new ValueChangedEvent<ScrollingDirection>(ScrollingDirection.Down, ScrollingDirection.Up));

            // Assert
            Assert.AreEqual(Anchor.TopCentre, hitExplosion.Anchor);
            Assert.AreEqual(DefaultNotePiece.NOTE_HEIGHT / 2, hitExplosion.Y);
        }

        [Test]
        public void TestOnDirectionChanged_Down()
        {
            // Arrange
            var directionBindable = new Bindable<ScrollingDirection>();
            directionBindable.Value = ScrollingDirection.Down;

            // Act
            hitExplosion.onDirectionChanged(new ValueChangedEvent<ScrollingDirection>(ScrollingDirection.Up, ScrollingDirection.Down));

            // Assert
            Assert.AreEqual(Anchor.BottomCentre, hitExplosion.Anchor);
            Assert.AreEqual(-DefaultNotePiece.NOTE_HEIGHT / 2, hitExplosion.Y);
        }
    }
}
