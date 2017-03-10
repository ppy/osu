// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.UI
{
    internal abstract class HealthDisplay : Container
    {
        public readonly BindableDouble Current = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 1
        };

        public HealthDisplay()
        {
            Current.ValueChanged += (s, e) => setHP((float)Current);
        }

        protected abstract void setHP(float value);
    }
}
