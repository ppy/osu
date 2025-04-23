// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModPresetRow : FillFlowContainer
    {
        private readonly Mod mod;

        public ModPresetRow(Mod mod)
        {
            this.mod = mod;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(5);
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
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.Torus.With(size: 16f, weight: FontWeight.SemiBold),
                            Colour = colourProvider.Content1,
                            UseFullGlyphHeight = false,
                            Text = mod.Name,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 10f },
                    Alpha = mod.SettingDescription.Any() ? 1 : 0,
                    Children = new Drawable[]
                    {
                        new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold);
                        })
                        {
                            AutoSizeAxes = Axes.Both,
                            Colour = colourProvider.Content2,
                            Text = string.Join('\n', mod.SettingDescription.Select(svp => svp.setting)),
                        },
                        new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold);
                        })
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Both,
                            Colour = colourProvider.Content1,
                            TextAnchor = Anchor.TopRight,
                            Text = string.Join('\n', mod.SettingDescription.Select(svp => svp.value)),
                        },
                    }
                }
            };
        }
    }
}
