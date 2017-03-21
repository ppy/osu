// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    class OptionCheckbox : OsuCheckbox, IFilterable
    {
        public string[] Keywords => new[] { LabelText };
        public bool FilteredByParent
        {
            set
            {
                if (value)
                    //FadeIn(250);
                    ScaleTo(new Vector2(1, 1), 250);
                else
                    ScaleTo(new Vector2(1, 0), 250);
            }
        }
    }
}
