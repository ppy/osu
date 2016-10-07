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

        protected override string formatCount(ulong count)
        {
            return count.ToString("D" + LeadingZeroes);
        }
    }
}
