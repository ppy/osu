﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Input;
using osu.Game.Beatmaps.Objects;

namespace osu.Game.GameModes.Play.Osu
{
    class ScoreOverlayOsu : ScoreOverlay
    {
        protected override PercentageCounter CreateAccuracyCounter() => new PercentageCounter()
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Position = new Vector2(0, 45)
        };

        protected override ScoreCounter CreateScoreCounter() => new ScoreCounter()
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            TextSize = 60
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
            Position = new Vector2(10),
            Counters = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(@"Z", Key.Z),
                        new KeyCounterKeyboard(@"X", Key.X),
                        new KeyCounterMouse(@"M1", MouseButton.Left),
                        new KeyCounterMouse(@"M2", MouseButton.Right),
                    }
        };
    }
}
