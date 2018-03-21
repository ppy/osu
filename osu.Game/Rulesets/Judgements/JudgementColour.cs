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
