// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.Judgements
{
    public class JudgementColour : Attribute
    {
        public Color4 colour { get; }

        public JudgementColour(string colourHex)
        {
            this.colour = OsuColour.FromHex(colourHex);
        }
    }
}
