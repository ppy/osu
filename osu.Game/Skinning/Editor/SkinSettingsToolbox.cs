// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    internal class SkinSettingsToolbox : ScrollingToolboxGroup
    {
        public const float WIDTH = 200;

        public SkinSettingsToolbox()
            : base("Settings", 600)
        {
            RelativeSizeAxes = Axes.None;
            Width = WIDTH;

            FillFlow.Spacing = new Vector2(10);
        }
    }
}
