// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;

namespace osu.Game.Modes.UI
{
    public class StandardHudOverlay : HudOverlay
    {
        protected override PercentageCounter CreateAccuracyCounter() => new PercentageCounter
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Position = new Vector2(0, 65),
            TextSize = 20,
            Margin = new MarginPadding { Right = 5 },
        };

        protected override ComboCounter CreateComboCounter() => new StandardComboCounter
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
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
            Margin = new MarginPadding { Right = 5 },
        };
    }
}
