// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OptionCheckbox : OptionItem<bool>
    {
        private OsuCheckbox checkbox;

        protected override Drawable CreateControl() => checkbox = new OsuCheckbox();

        public override string LabelText
        {
            get { return checkbox.LabelText; }
            set { checkbox.LabelText = value; }
        }
    }
}
