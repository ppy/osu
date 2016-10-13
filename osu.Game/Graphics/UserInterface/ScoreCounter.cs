//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class ScoreCounter : ULongCounter
    {
        /// <summary>
        /// How many leading zeroes the counter has.
        /// </summary>
        public uint LeadingZeroes
        {
            get;
            protected set;
        }

        /// <summary>
        /// Displays score.
        /// </summary>
        /// <param name="leading">How many leading zeroes the counter will have.</param>
        public ScoreCounter(uint leading = 0)
        {
            countSpriteText.FixedWidth = true;
            LeadingZeroes = leading;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
        }

        protected override string formatCount(ulong count)
        {
            return count.ToString("D" + LeadingZeroes);
        }
    }
}
