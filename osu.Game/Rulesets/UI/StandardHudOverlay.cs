// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.UI
{
    public class StandardHudOverlay : HudOverlay
    {
        protected override RollingCounter<double> CreateAccuracyCounter() => new PercentageCounter
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopRight,
            Position = new Vector2(0, 35),
            TextSize = 20,
            Margin = new MarginPadding { Right = 140 },
        };

        protected override RollingCounter<int> CreateComboCounter() => new SimpleComboCounter
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopLeft,
            Position = new Vector2(0, 35),
            Margin = new MarginPadding { Left = 140 },
            TextSize = 20,
        };

        protected override HealthDisplay CreateHealthDisplay() => new StandardHealthDisplay
        {
            Size = new Vector2(1, 5),
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = 20 }
        };

        protected override KeyCounterCollection CreateKeyCounter() => new KeyCounterCollection
        {
            IsCounting = true,
            FadeTime = 50,
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            Margin = new MarginPadding(10),
            Y = - TwoLayerButton.SIZE_RETRACTED.Y,
        };

        protected override ScoreCounter CreateScoreCounter() => new ScoreCounter(6)
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            TextSize = 40,
            Position = new Vector2(0, 30),
        };

        protected override SongProgress CreateProgress() => new SongProgress
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            RelativeSizeAxes = Axes.X,
        };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ComboCounter.AccentColour = colours.BlueLighter;
            AccuracyCounter.AccentColour = colours.BlueLighter;
            ScoreCounter.AccentColour = colours.BlueLighter;

            var shd = HealthDisplay as StandardHealthDisplay;
            if (shd != null)
            {
                shd.AccentColour = colours.BlueLighter;
                shd.GlowColour = colours.BlueDarker;
            }
        }

        public override void BindProcessor(ScoreProcessor processor)
        {
            base.BindProcessor(processor);

            var shd = HealthDisplay as StandardHealthDisplay;
            if (shd != null)
                processor.NewJudgement += shd.Flash;
        }
    }
}
