// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    class OptionCheckbox : OsuCheckbox, ISearchable
    {
        public string[] Keywords => new[] { LabelText };
        public bool Matching
        {
            set
            {
                if (value)
                    FadeIn(250);
                else
                    FadeOut(250);
            }
        }
    }
}
