// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class PlayerLoaderDisclaimer : CompositeDrawable
    {
        private readonly LocalisableString title;
        private readonly LocalisableString content;

        private Box background = null!;

        public bool IsImportant { get; init; }

        public Action? Action { get; set; }

        public PlayerLoaderDisclaimer(LocalisableString title, LocalisableString content)
        {
            this.title = title;
            this.content = content;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 5;

            if (IsImportant)
            {
                BorderColour = colours.Red2;
                BorderThickness = 1.5f;
            }

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = IsImportant ? colours.Red3 : colourProvider.Background4,
                    Alpha = 0.1f,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Width = 7,
                            Height = 15,
                            Margin = new MarginPadding { Top = 2 },
                            Colour = IsImportant ? colours.Red1 : colours.Orange1,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Left = 12 },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 2),
                            Children = new[]
                            {
                                new TextFlowContainer(t => t.Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Text = title,
                                },
                                new TextFlowContainer(t => t.Font = OsuFont.GetFont(size: 16))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Text = content,
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (IsImportant)
            {
                background.FadeTo(0.6f, 600, Easing.Out)
                          .Then()
                          .FadeTo(0.1f, 300, Easing.Out)
                          .Loop();
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateFadeState();

            // handle hover so that users can hover the disclaimer to delay load if they want to read it.
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateFadeState();
            base.OnHoverLost(e);
        }

        private void updateFadeState()
        {
            // Matches SettingsToolboxGroup
            background.FadeTo(IsHovered ? 1 : 0.1f, 500, Easing.OutQuint);
        }
    }
}
