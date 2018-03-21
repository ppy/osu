// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Graphics;
using System;

namespace osu.Game.Rulesets.Judgements
{
    public class JudgementColour : Attribute
    {
        public Color4 Colour { get; }

        public JudgementColour(string colourHex)
        {
            Colour = OsuColour.FromHex(colourHex);
        }
    }
}
