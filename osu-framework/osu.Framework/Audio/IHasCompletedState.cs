//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.Audio
{
    public interface IHasCompletedState
    {
        /// <summary>
        /// Becomes true when we are out and done with this object (and pending clean-up).
        /// </summary>
        bool HasCompleted { get; }
    }
}
