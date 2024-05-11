// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonModsV2 : FooterButtonV2, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        // todo: see https://github.com/ppy/osu-framework/issues/3271
        private const float torus_scale_factor = 1.2f;
        private const float bar_shear_width = 7f;
        private const float bar_height = 37f;
        private const float mod_display_portion = 0.65f;

        private static readonly Vector2 bar_shear = new Vector2(bar_shear_width / bar_height, 0);

        private readonly BindableWithCurrent<IReadOnlyList<Mod>> current = new BindableWithCurrent<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private Container modDisplayBar = null!;

        private Drawable unrankedBadge = null!;

        private ModDisplay modDisplay = null!;
        private OsuSpriteText modCountText = null!;

        protected OsuSpriteText MultiplierText { get; private set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Mods";
            Icon = FontAwesome.Solid.ExchangeAlt;
            AccentColour = colours.Lime1;

            AddRange(new[]
            {
                unrankedBadge = new UnrankedBadge(),
                modDisplayBar = new Container
                {
                    Y = -5f,
                    Depth = float.MaxValue,
                    Origin = Anchor.BottomLeft,
                    Shear = bar_shear,
                    CornerRadius = CORNER_RADIUS,
                    Size = new Vector2(BUTTON_WIDTH, bar_height),
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 4,
                        // Figma says 50% opacity, but it does not match up visually if taken at face value, and looks bad.
                        Colour = Colour4.Black.Opacity(0.25f),
                        Offset = new Vector2(0, 2),
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background4,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 1f - mod_display_portion,
                            Masking = true,
                            Child = MultiplierText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = -bar_shear,
                                UseFullGlyphHeight = false,
                                Font = OsuFont.Torus.With(size: 14 * torus_scale_factor, weight: FontWeight.Bold)
                            }
                        },
                        new Container
                        {
                            CornerRadius = CORNER_RADIUS,
                            RelativeSizeAxes = Axes.Both,
                            Width = mod_display_portion,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background3,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                modDisplay = new ModDisplay(showExtendedInformation: false)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shear = -bar_shear,
                                    Scale = new Vector2(0.6f),
                                    Current = { BindTarget = Current },
                                    ExpansionMode = ExpansionMode.AlwaysContracted,
                                },
                                modCountText = new ModCountText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shear = -bar_shear,
                                    Font = OsuFont.Torus.With(size: 14 * torus_scale_factor, weight: FontWeight.Bold),
                                    Mods = { BindTarget = Current },
                                }
                            }
                        },
                    }
                },
            });
        }

        private ModSettingChangeTracker? modSettingChangeTracker;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(m =>
            {
                modSettingChangeTracker?.Dispose();

                updateDisplay();

                if (m.NewValue != null)
                {
                    modSettingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                    modSettingChangeTracker.SettingChanged += _ => updateDisplay();
                }
            }, true);

            FinishTransforms(true);
        }

        private const double duration = 240;
        private const Easing easing = Easing.OutQuint;

        private void updateDisplay()
        {
            if (Current.Value.Count == 0)
            {
                modDisplayBar.MoveToY(20, duration, easing);
                modDisplayBar.FadeOut(duration, easing);
                modDisplay.FadeOut(duration, easing);
                modCountText.FadeOut(duration, easing);

                unrankedBadge.MoveToY(20, duration, easing);
                unrankedBadge.FadeOut(duration, easing);

                // add delay to let unranked indicator hide first before resizing the button back to its original width.
                this.Delay(duration).ResizeWidthTo(BUTTON_WIDTH, duration, easing);
            }
            else
            {
                modDisplay.Hide();
                modCountText.Hide();

                if (Current.Value.Count >= 5)
                    modCountText.Show();
                else
                    modDisplay.Show();

                if (Current.Value.Any(m => !m.Ranked))
                {
                    unrankedBadge.MoveToX(0, duration, easing);
                    unrankedBadge.FadeIn(duration, easing);

                    this.ResizeWidthTo(BUTTON_WIDTH + 5 + unrankedBadge.DrawWidth, duration, easing);
                }
                else
                {
                    unrankedBadge.MoveToX(-unrankedBadge.DrawWidth, duration, easing);
                    unrankedBadge.FadeOut(duration, easing);

                    this.ResizeWidthTo(BUTTON_WIDTH, duration, easing);
                }

                modDisplayBar.MoveToY(-5, duration, Easing.OutQuint);
                unrankedBadge.MoveToY(-5, duration, easing);
                modDisplayBar.FadeIn(duration, easing);
            }

            double multiplier = Current.Value?.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier) ?? 1;
            MultiplierText.Text = ModUtils.FormatScoreMultiplier(multiplier);

            if (multiplier > 1)
                MultiplierText.FadeColour(colours.Red1, duration, easing);
            else if (multiplier < 1)
                MultiplierText.FadeColour(colours.Lime1, duration, easing);
            else
                MultiplierText.FadeColour(Color4.White, duration, easing);
        }

        private partial class ModCountText : OsuSpriteText, IHasCustomTooltip<IReadOnlyList<Mod>>
        {
            public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>();

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Mods.BindValueChanged(v => Text = ModSelectOverlayStrings.Mods(v.NewValue.Count).ToUpper(), true);
            }

            public ITooltip<IReadOnlyList<Mod>> GetCustomTooltip() => new ModTooltip(colourProvider);

            public IReadOnlyList<Mod>? TooltipContent => Mods.Value;

            public partial class ModTooltip : VisibilityContainer, ITooltip<IReadOnlyList<Mod>>
            {
                private ModDisplay extendedModDisplay = null!;

                [Cached]
                private OverlayColourProvider colourProvider;

                public ModTooltip(OverlayColourProvider colourProvider)
                {
                    this.colourProvider = colourProvider;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    AutoSizeAxes = Axes.Both;
                    CornerRadius = CORNER_RADIUS;
                    Masking = true;

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        extendedModDisplay = new ModDisplay
                        {
                            Margin = new MarginPadding { Vertical = 2f, Horizontal = 10f },
                            Scale = new Vector2(0.6f),
                            ExpansionMode = ExpansionMode.AlwaysExpanded,
                        },
                    };
                }

                public void SetContent(IReadOnlyList<Mod> content)
                {
                    extendedModDisplay.Current.Value = content;
                }

                public void Move(Vector2 pos) => Position = pos;

                protected override void PopIn() => this.FadeIn(240, Easing.OutQuint);
                protected override void PopOut() => this.FadeOut(240, Easing.OutQuint);
            }
        }

        internal partial class UnrankedBadge : CompositeDrawable, IHasTooltip
        {
            public LocalisableString TooltipText { get; }

            public UnrankedBadge()
            {
                Margin = new MarginPadding { Left = BUTTON_WIDTH + 5f };
                Y = -5f;
                Depth = float.MaxValue;
                Origin = Anchor.BottomLeft;
                Shear = bar_shear;
                CornerRadius = CORNER_RADIUS;
                AutoSizeAxes = Axes.X;
                Height = bar_height;
                Masking = true;
                BorderColour = Color4.White;
                BorderThickness = 2f;
                TooltipText = ModSelectOverlayStrings.UnrankedExplanation;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Orange2,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Shear = -bar_shear,
                        Text = ModSelectOverlayStrings.Unranked.ToUpper(),
                        Margin = new MarginPadding { Horizontal = 15 },
                        UseFullGlyphHeight = false,
                        Font = OsuFont.Torus.With(size: 14 * torus_scale_factor, weight: FontWeight.Bold),
                        Colour = Color4.Black,
                    }
                };
            }
        }
    }
}
