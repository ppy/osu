// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace Osu.Game.Tests.Rulesets.Osu.Edit
{
    [TestFixture]
    public class OsuSelectionScaleHandlerTests
    {
        [Test]
        public void TestMoveSelectionInBounds_AllInBounds()
        {
            // Arrange
            var handler = new OsuSelectionScaleHandler();
            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle { Position = new Vector2(50, 50) }, // Example with HitCircle
                new Slider { Position = new Vector2(100, 100) }    // Example with Slider
            };
            handler.Begin();
            handler.GetType().GetField("objectsInScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(handler, hitObjects.ToDictionary(ho => ho, ho => new OsuSelectionScaleHandler.OriginalHitObjectState(ho)));

            // Act
            handler.GetType().GetMethod("moveSelectionInBounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(handler, null);

            // Assert
            Assert.IsTrue(hitObjects[0].Position.X >= 0 && hitObjects[0].Position.Y >= 0);
            Assert.IsTrue(hitObjects[1].Position.X >= 0 && hitObjects[1].Position.Y >= 0);
        }

        [Test]
        public void TestMoveSelectionInBounds_OutOfBounds()
        {
            // Arrange
            var handler = new OsuSelectionScaleHandler();
            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle { Position = new Vector2(-50, -50) }, // Example with HitCircle
                new Slider { Position = new Vector2(200, 200) }     // Example with Slider
            };
            handler.Begin();
            handler.GetType().GetField("objectsInScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(handler, hitObjects.ToDictionary(ho => ho, ho => new OsuSelectionScaleHandler.OriginalHitObjectState(ho)));

            // Act
            handler.GetType().GetMethod("moveSelectionInBounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(handler, null);

            // Assert
            Assert.IsTrue(hitObjects[0].Position.X >= 0 && hitObjects[0].Position.Y >= 0);
            Assert.IsTrue(hitObjects[1].Position.X < OsuPlayfield.BASE_SIZE.X && hitObjects[1].Position.Y < OsuPlayfield.BASE_SIZE.Y);
        }
    }
}
