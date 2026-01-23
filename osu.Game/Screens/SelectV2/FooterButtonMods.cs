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
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Play.HUD;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    public partial class FooterButtonMods : ScreenFooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        public Action? RequestDeselectAllMods { get; init; }

        private const float bar_height = 30f;
        private const float mod_display_portion = 0.65f;

        private readonly BindableWithCurrent<IReadOnlyList<Mod>> current = new BindableWithCurrent<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private Container modDisplayBar = null!;

        private Drawable unrankedBadge = null!;

        private ModDisplay modDisplay = null!;

        private OsuSpriteText multiplierText { get; set; } = null!;

        private Container modContainer = null!;

        private ModCountText overflowModCountDisplay = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        private IBindable<Language> currentLanguage = null!;

        public FooterButtonMods(ModSelectOverlay overlay)
            : base(overlay)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = SongSelectStrings.Mods;
            Icon = FontAwesome.Solid.ExchangeAlt;
            AccentColour = colours.Lime1;

            AddRange(new[]
            {
                unrankedBadge = new UnrankedBadge(),
                modDisplayBar = new InputBlockingContainer
                {
                    Y = -5f,
                    Depth = float.MaxValue,
                    Origin = Anchor.BottomLeft,
                    Shear = OsuGame.SHEAR,
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
                            Child = multiplierText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = -OsuGame.SHEAR,
                                UseFullGlyphHeight = false,
                                Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.Bold)
                            }
                        },
                        modContainer = new Container
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
                                modDisplay = new ModDisplay(showExtendedInformation: true)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shear = -OsuGame.SHEAR,
                                    Scale = new Vector2(0.5f),
                                    Current = { BindTarget = Current },
                                    ExpansionMode = ExpansionMode.AlwaysContracted,
                                },
                                overflowModCountDisplay = new ModCountText { Mods = { BindTarget = Current }, },
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

            currentLanguage = game.CurrentLanguage.GetBoundCopy();
            currentLanguage.BindValueChanged(_ => ScheduleAfterChildren(updateDisplay));

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

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // should probably be OnClick but right mouse button clicks isn't setup well.
            if (e.Button == MouseButton.Right)
            {
                RequestDeselectAllMods?.Invoke();
                return true;
            }

            return base.OnMouseDown(e);
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
                overflowModCountDisplay.FadeOut(duration, easing);

                unrankedBadge.MoveToY(20, duration, easing);
                unrankedBadge.FadeOut(duration, easing);

                // add delay to let unranked indicator hide first before resizing the button back to its original width.
                this.Delay(duration).ResizeWidthTo(BUTTON_WIDTH, duration, easing);
            }
            else
            {
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
                modDisplay.FadeIn(duration, easing);
            }

            double multiplier = Current.Value?.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier) ?? 1;
            multiplierText.Text = ModUtils.FormatScoreMultiplier(multiplier);

            if (multiplier > 1)
                multiplierText.FadeColour(colours.Red1, duration, easing);
            else if (multiplier < 1)
                multiplierText.FadeColour(colours.Lime1, duration, easing);
            else
                multiplierText.FadeColour(Color4.White, duration, easing);
        }

        protected override void Update()
        {
            base.Update();

            if (Current.Value.Count == 0)
                return;

            if (modDisplay.DrawWidth * modDisplay.Scale.X > modContainer.DrawWidth)
                overflowModCountDisplay.Show();
            else
                overflowModCountDisplay.Hide();
        }

        private partial class ModCountText : CompositeDrawable, IHasCustomTooltip<IReadOnlyList<Mod>>
        {
            public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>();

            private OsuSpriteText text = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background3,
                        Alpha = 0.8f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.Bold),
                        Shear = -OsuGame.SHEAR,
                    }
                };

                Mods.BindValueChanged(v => text.Text = ModSelectOverlayStrings.Mods(v.NewValue.Count).ToUpper(), true);
            }

            public ITooltip<IReadOnlyList<Mod>> GetCustomTooltip() => new ModOverflowTooltip(colourProvider);

            public IReadOnlyList<Mod>? TooltipContent => Mods.Value;

            public partial class ModOverflowTooltip : VisibilityContainer, ITooltip<IReadOnlyList<Mod>>
            {
                private ModDisplay extendedModDisplay = null!;

                [Cached]
                private OverlayColourProvider colourProvider;

                public ModOverflowTooltip(OverlayColourProvider colourProvider)
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

        internal partial class UnrankedBadge : InputBlockingContainer, IHasTooltip
        {
            public LocalisableString TooltipText { get; }

            public UnrankedBadge()
            {
                Margin = new MarginPadding { Left = BUTTON_WIDTH + 5f };
                Y = -5f;
                Depth = float.MaxValue;
                Origin = Anchor.BottomLeft;
                Shear = OsuGame.SHEAR;
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
                        Shear = -OsuGame.SHEAR,
                        Text = ModSelectOverlayStrings.Unranked.ToUpper(),
                        Margin = new MarginPadding { Horizontal = 15 },
                        UseFullGlyphHeight = false,
                        Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.Bold),
                        Colour = Color4.Black,
                    }
                };
            }
        }
    }
}
