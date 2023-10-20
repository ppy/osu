// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModPresetRow : FillFlowContainer
    {
        public ModPresetRow(Mod mod)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(4);
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7),
                    Children = new Drawable[]
                    {
                        new ModSwitchTiny(mod)
                        {
                            Active = { Value = true },
                            Scale = new Vector2(0.6f),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        },
                        new OsuSpriteText
                        {
                            Text = mod.Name,
                            Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Margin = new MarginPadding { Bottom = 2 }
                        }
                    }
                }
            };

            if (!string.IsNullOrEmpty(mod.SettingDescription))
            {
                AddInternal(new OsuTextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = 14 },
                    Text = mod.SettingDescription
                });
            }
        }
    }
}
