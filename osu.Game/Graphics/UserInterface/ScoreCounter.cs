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
        /// How many leading zeroes the counter will have.
        /// </summary>
        public uint LeadingZeroes = 0;

        public override void Load(BaseGame game)
        {
            base.Load(game);

            countSpriteText.FixedWidth = true;
        }

        protected override string formatCount(ulong count)
        {
            return count.ToString("D" + LeadingZeroes);
        }
    }
}
