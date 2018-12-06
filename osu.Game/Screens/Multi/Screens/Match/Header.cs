// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Header : Container
    {
        public const float HEIGHT = 200;

        public readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly Box tabStrip;

        public readonly PageTabControl<MatchHeaderPage> Tabs;

        public Action OnRequestSelectBeatmap;

        public Header()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            BeatmapSelectButton beatmapButton;
            UpdateableBeatmapBackgroundSprite background;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = background = new HeaderBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both }
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.5f)),
                },
                tabStrip = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 200,
                            Padding = new MarginPadding { Vertical = 5 },
                            Child = beatmapButton = new BeatmapSelectButton
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                        Tabs = new PageTabControl<MatchHeaderPage>
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            beatmapButton.Action = () => OnRequestSelectBeatmap?.Invoke();

            background.Beatmap.BindTo(Beatmap);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
        }

        private class BeatmapSelectButton : OsuClickableContainer
        {
            private const float corner_radius = 5;
            private const float bg_opacity = 0.5f;
            private const float transition_duration = 100;

            private readonly Box bg;
            private readonly Container border;

            public BeatmapSelectButton()
            {
                Masking = true;
                CornerRadius = corner_radius;

                Children = new Drawable[]
                {
                    bg = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = bg_opacity,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = @"Exo2.0-Bold",
                        Text = "Select Beatmap",
                    },
                    border = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = corner_radius,
                        BorderThickness = 4,
                        Alpha = 0,
                        Child = new Box // needs a child to show the border
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                border.BorderColour = colours.Yellow;
            }

            protected override bool OnHover(HoverEvent e)
            {
                border.FadeIn(transition_duration);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                border.FadeOut(transition_duration);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                bg.FadeTo(0.75f, 1000, Easing.Out);
                return base.OnMouseDown(e);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                bg.FadeTo(bg_opacity, transition_duration);
                return base.OnMouseUp(e);
            }
        }

        private class HeaderBeatmapBackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            protected override double FadeDuration => 0;
        }
    }

    public enum MatchHeaderPage
    {
        Settings,
        Room,
    }
}
