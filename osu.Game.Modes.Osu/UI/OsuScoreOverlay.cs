// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Graphics.Primitives;
using osu.Game.Screens.Play;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuScoreOverlay : ScoreOverlay
    {
        protected override ScoreCounter CreateScoreCounter() => new ScoreCounter(6)
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            TextSize = 40,
            Position = new Vector2(0, 30),
            Margin = new MarginPadding { Right = 5 },
        };

        protected override PercentageCounter CreateAccuracyCounter() => new PercentageCounter()
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Position = new Vector2(0, 65),
            TextSize = 20,
            Margin = new MarginPadding { Right = 5 },
        };

        protected override ComboCounter CreateComboCounter() => new OsuComboCounter()
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
                new KeyCounterKeyboard(@"Z", Key.Z),
                new KeyCounterKeyboard(@"X", Key.X),
                new KeyCounterMouse(@"M1", MouseButton.Left),
                new KeyCounterMouse(@"M2", MouseButton.Right),
            }
        };
    }
}
