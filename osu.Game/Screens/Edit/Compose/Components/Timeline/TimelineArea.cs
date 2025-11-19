// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineArea : CompositeDrawable
    {
        public Timeline Timeline = null!;

        private readonly Drawable userContent;

        private Box timelineBackground = null!;
        private readonly Bindable<bool> composerFocusMode = new Bindable<bool>();

        public TimelineArea(Drawable? content = null)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            userContent = content ?? Empty();
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours, Editor? editor)
        {
            const float padding = 10;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Width = 35 + HitObjectComposer.TOOLBOX_CONTRACTED_SIZE_RIGHT,
                    RelativeSizeAxes = Axes.Y,
                    Colour = colourProvider.Background4
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 35),
                        new Dimension(GridSizeMode.Absolute, HitObjectComposer.TOOLBOX_CONTRACTED_SIZE_RIGHT),
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
                                    timelineBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Depth = float.MaxValue,
                                        Colour = colourProvider.Background5
                                    },
                                    Timeline = new Timeline(userContent),
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Name = @"Zoom controls",
                                Padding = new MarginPadding { Right = padding },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background2,
                                    },
                                    new Container<TimelineButton>
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new[]
                                        {
                                            new TimelineButton
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Size = new Vector2(1, 0.5f),
                                                Icon = FontAwesome.Solid.SearchPlus,
                                                Action = () => Timeline.AdjustZoomRelatively(1)
                                            },
                                            new TimelineButton
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                RelativeSizeAxes = Axes.Both,
                                                Size = new Vector2(1, 0.5f),
                                                Icon = FontAwesome.Solid.SearchMinus,
                                                Action = () => Timeline.AdjustZoomRelatively(-1)
                                            },
                                        }
                                    }
                                }
                            },
                            new BeatDivisorControl { RelativeSizeAxes = Axes.Both }
                        },
                    },
                }
            };

            if (editor != null)
                composerFocusMode.BindTo(editor.ComposerFocusMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            composerFocusMode.BindValueChanged(_ =>
            {
                // Transforms should be kept in sync with other usages of composer focus mode.
                if (!composerFocusMode.Value)
                    timelineBackground.FadeIn(750, Easing.OutQuint);
                else
                    timelineBackground.Delay(600).FadeTo(0.5f, 4000, Easing.OutQuint);
            }, true);
        }
    }
}
