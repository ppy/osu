// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    public class ModSwitchTiny : CompositeDrawable
    {
        public BindableBool Active { get; } = new BindableBool();

        public const float DEFAULT_HEIGHT = 30;

        private readonly IMod mod;

        private readonly Box background;
        private readonly OsuSpriteText acronymText;

        private Color4 activeForegroundColour;
        private Color4 inactiveForegroundColour;

        private Color4 activeBackgroundColour;
        private Color4 inactiveBackgroundColour;

        public ModSwitchTiny(IMod mod)
        {
            this.mod = mod;
            Size = new Vector2(73, DEFAULT_HEIGHT);

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    acronymText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Shadow = false,
                        Font = OsuFont.Numeric.With(size: 24, weight: FontWeight.Black),
                        Text = mod.Acronym,
                        Margin = new MarginPadding
                        {
                            Top = 4
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OverlayColourProvider? colourProvider)
        {
            inactiveBackgroundColour = colourProvider?.Background5 ?? colours.Gray3;
            activeBackgroundColour = colours.ForModType(mod.Type);

            inactiveForegroundColour = colourProvider?.Background2 ?? colours.Gray5;
            activeForegroundColour = Interpolation.ValueAt<Colour4>(0.1f, Colour4.Black, activeForegroundColour, 0, 1);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Active.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            acronymText.FadeColour(Active.Value ? activeForegroundColour : inactiveForegroundColour, 200, Easing.OutQuint);
            background.FadeColour(Active.Value ? activeBackgroundColour : inactiveBackgroundColour, 200, Easing.OutQuint);
        }
    }
}
