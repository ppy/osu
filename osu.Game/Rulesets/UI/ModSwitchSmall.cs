// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    public partial class ModSwitchSmall : CompositeDrawable
    {
        public BindableBool Active { get; } = new BindableBool();

        public const float DEFAULT_SIZE = 60;

        private readonly IMod mod;

        private Drawable background = null!;
        private SpriteIcon? modIcon;

        private Color4 activeForegroundColour;
        private Color4 inactiveForegroundColour;

        private Color4 activeBackgroundColour;
        private Color4 inactiveBackgroundColour;

        public ModSwitchSmall(IMod mod)
        {
            this.mod = mod;

            Size = new Vector2(DEFAULT_SIZE);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, OsuColour colours, OverlayColourProvider? colourProvider)
        {
            FillFlowContainer contentFlow;
            ModSwitchTiny tinySwitch;

            InternalChildren = new[]
            {
                background = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = textures.Get("Icons/BeatmapDetails/mod-icon"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                contentFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(0, 4),
                    Direction = FillDirection.Vertical,
                    Child = tinySwitch = new ModSwitchTiny(mod)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Scale = new Vector2(0.6f),
                        Active = { BindTarget = Active }
                    }
                }
            };

            if (mod.Icon != null)
            {
                contentFlow.Insert(-1, modIcon = new SpriteIcon
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(21),
                    Icon = mod.Icon.Value
                });
                tinySwitch.Scale = new Vector2(0.3f);
            }

            var modTypeColour = colours.ForModType(mod.Type);

            inactiveForegroundColour = colourProvider?.Background5 ?? colours.Gray3;
            activeForegroundColour = modTypeColour;

            inactiveBackgroundColour = colourProvider?.Background2 ?? colours.Gray5;
            activeBackgroundColour = Interpolation.ValueAt<Colour4>(0.1f, Colour4.Black, modTypeColour, 0, 1);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Active.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            modIcon?.FadeColour(Active.Value ? activeForegroundColour : inactiveForegroundColour, 200, Easing.OutQuint);
            background.FadeColour(Active.Value ? activeBackgroundColour : inactiveBackgroundColour, 200, Easing.OutQuint);
        }
    }
}
