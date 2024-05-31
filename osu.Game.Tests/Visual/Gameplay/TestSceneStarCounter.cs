// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        private readonly StarCounter twentyStarCounter;
        private readonly OsuSpriteText starsLabel;

        public TestSceneStarCounter()
        {
            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    starCounter = new StarCounter(),
                    twentyStarCounter = new StarCounter(20),
                    starsLabel = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Scale = new Vector2(2),
                    },
                }
            });

            setStars(5);

            AddRepeatStep("random value", () => setStars(RNG.NextSingle() * (twentyStarCounter.StarCount + 1)), 10);
            AddSliderStep("exact value", 0f, 20f, 5f, setStars);
            AddStep("stop animation", () =>
            {
                starCounter.StopAnimation();
                twentyStarCounter.StopAnimation();
            });
            AddStep("reset", () => setStars(0));
        }

        private void setStars(float stars)
        {
            starCounter.Current = stars;
            twentyStarCounter.Current = stars;
            starsLabel.Text = starCounter.Current.ToString("0.00");
        }
    }
}
