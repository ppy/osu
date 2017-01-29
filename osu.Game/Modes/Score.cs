//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes
{
    public class Score
    {
        public double TotalScore { get; set; }
        public double Accuracy { get; set; }
        public double Combo { get; set; }
        public double MaxCombo { get; set; }
        public double Health { get; set; }
    }
}
