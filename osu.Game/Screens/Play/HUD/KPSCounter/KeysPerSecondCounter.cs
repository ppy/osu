// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD.KPSCounter
{
    public class KeysPerSecondCounter : RollingCounter<int>, ISkinnableDrawable
    {
        private const float alpha_when_invalid = 0.3f;

        private readonly Bindable<bool> valid = new Bindable<bool>();
        private GameplayClock gameplayClock;

        [Resolved(canBeNull: true)]
        private DrawableRuleset drawableRuleset { get; set; }

        private KeysPerSecondCalculator calculator => KeysPerSecondCalculator.GetInstance(gameplayClock, drawableRuleset);

        [SettingSource("Smoothing time", "How smooth the counter should change\nThe more it is smooth, the less it's accurate.")]
        public BindableNumber<double> SmoothingTime { get; } = new BindableNumber<double>(350)
        {
            MaxValue = 1000,
            MinValue = 0
        };

        protected override double RollingDuration => SmoothingTime.Value;

        public bool UsesFixedAnchor { get; set; }

        public KeysPerSecondCounter()
        {
            Current.Value = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameplayClock clock, DrawableRuleset ruleset)
        {
            gameplayClock = clock;
            Colour = colours.BlueLighter;
            valid.BindValueChanged(e =>
                DrawableCount.FadeTo(e.NewValue ? 1 : alpha_when_invalid, 1000, Easing.OutQuint));
        }

        protected override void Update()
        {
            base.Update();

            valid.Value = calculator.Ready;
            Current.Value = calculator.Value;
        }

        protected override IHasText CreateText() => new TextComponent
        {
            Alpha = alpha_when_invalid
        };

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 8, fixedWidth: true),
                            Text = @"KPS",
                            Padding = new MarginPadding { Bottom = 1.5f }, // align baseline better
                        }
                    }
                };
            }
        }
    }
}
