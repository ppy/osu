// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Rulesets.Edit
{
    public class EditorToolboxGroup : SettingsToolboxGroup
    {
        public EditorToolboxGroup(string title)
            : base(title)
        {
            RelativeSizeAxes = Axes.X;
            Width = 1;
        }
    }
}
