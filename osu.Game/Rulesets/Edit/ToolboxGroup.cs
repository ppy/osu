// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
