// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerSpmCounter : Container
    {
        private readonly OsuSpriteText spmText;
        public SpinnerSpmCounter()
        {
            Children = new Drawable[]
            {
                spmText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"0",
                    Font = @"Venera",
                    TextSize = 24
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"SPINS PER MINUTE",
                    Font = @"Venera",
                    TextSize = 12,
                    Y = 30
                }
            };
        }
    }
}
