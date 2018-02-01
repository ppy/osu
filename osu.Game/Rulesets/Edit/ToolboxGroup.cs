// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Edit
{
    public class ToolboxGroup : PlayerSettingsGroup
    {
        protected override string Title => "toolbox";

        public ToolboxGroup()
        {
            RelativeSizeAxes = Axes.X;
            Width = 1;
        }
    }
}
