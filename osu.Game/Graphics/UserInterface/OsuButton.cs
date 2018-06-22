// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with added default sound effects.
    /// </summary>
    public class OsuButton : Button
    {
        public OsuButton()
        {
            Add(new HoverClickSounds(HoverSampleSet.Loud));
        }
    }
}
