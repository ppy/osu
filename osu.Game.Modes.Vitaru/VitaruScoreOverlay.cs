using System;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes.UI;
using osu.Game.Screens.Play;
using OpenTK.Input;
using osu.Game.Modes.Vitaru.UI;
using osu.Game.Modes.Osu.UI;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics;
using OpenTK;

namespace osu.Game.Modes.Vitaru
{
    internal class VitaruScoreOverlay : ScoreOverlay
    {
        protected override ScoreCounter CreateScoreCounter() => new ScoreCounter(6)
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            TextSize = 40,
            Position = new Vector2(0, 30),
            Margin = new MarginPadding { Right = 5 },
        };

        protected override PercentageCounter CreateAccuracyCounter() => new EnergyMeter()
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Position = new Vector2(0, 65),
            TextSize = 20,
            Margin = new MarginPadding { Right = 5 },
        };

        protected override ComboCounter CreateComboCounter() => new VitaruComboCounter()
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
        };

        protected override KeyCounterCollection CreateKeyCounter() => new KeyCounterCollection
        {
            IsCounting = true,
            FadeTime = 50,
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            Margin = new MarginPadding(10),
            Children = new KeyCounter[]
            {
                new KeyCounterKeyboard(Key.W),
                new KeyCounterKeyboard(Key.A),
                new KeyCounterKeyboard(Key.S),
                new KeyCounterKeyboard(Key.D),
            }
        };
    }
}