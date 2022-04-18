// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    [Cached]
    public class FirstRunSetupOverlay : OsuFocusedOverlayContainer
    {
        protected override bool StartHidden => true;

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private OsuGame osuGame { get; set; }

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private ScreenWelcome welcomeScreen;

        public FirstRunSetupOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.95f),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                Colour = colourProvider.Background6,
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Margin = new MarginPadding(10),
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = "First run setup",
                                                        Font = OsuFont.Default.With(size: 32),
                                                        Colour = colourProvider.Content2,
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                    },
                                                    new OsuTextFlowContainer
                                                    {
                                                        Text = "Setup osu! to suit you",
                                                        Colour = colourProvider.Content1,
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        AutoSizeAxes = Axes.Both,
                                                    },
                                                }
                                            },
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    new ScreenStack(welcomeScreen = new ScreenWelcome())
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                }
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (osuGame != null)
            {
                // if we are valid for display, only do so after reaching the main menu.
                osuGame.PerformFromScreen(_ =>
                {
                    Show();
                }, new[] { typeof(MainMenu) });
            }
            else
            {
                Show();
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (dialogOverlay?.CurrentDialog == null)
            {
                dialogOverlay?.Push(new ConfirmDialog("Are you sure you want to exit the setup process?",
                    Hide,
                    () => { }));
            }

            return base.OnClick(e);
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(400, Easing.OutQuint);

            if (welcomeScreen.GetChildScreen() != null)
                welcomeScreen.MakeCurrent();
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(100);
        }
    }
}
