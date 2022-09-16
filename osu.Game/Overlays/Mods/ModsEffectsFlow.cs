// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public sealed class ModsEffectsFlow : FillFlowContainer<ModsEffectDisplay>
    {
        public ModsEffectsFlow()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            AutoSizeAxes = Axes.X;
            Direction = FillDirection.Horizontal;
            Height = ModsEffectDisplay.HEIGHT;
            Margin = new MarginPadding { Horizontal = 100 };
            Spacing = new Vector2(5, 0);
            Children = new ModsEffectDisplay[]
            {
                new BpmDisplay(),
                new DifficultyMultiplierDisplay()
            };
        }
    }
}
