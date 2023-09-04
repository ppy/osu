// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneStarCounter : OsuTestScene
    {
        private readonly StarCounter starCounter;
        private readonly OsuSpriteText starsLabel;

        public TestSceneStarCounter()
        {
            starCounter = new StarCounter
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            };

            Add(starCounter);

            starsLabel = new OsuSpriteText
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Scale = new Vector2(2),
                Y = 50,
            };

            Add(starsLabel);

            setStars(5);

            AddRepeatStep("random value", () => setStars(RNG.NextSingle() * (starCounter.StarCount + 1)), 10);
            AddSliderStep("exact value", 0f, 10f, 5f, setStars);
            AddStep("stop animation", () => starCounter.StopAnimation());
            AddStep("reset", () => setStars(0));
        }

        private void setStars(float stars)
        {
            starCounter.Current = stars;
            starsLabel.Text = starCounter.Current.ToString("0.00");
        }
    }
}
