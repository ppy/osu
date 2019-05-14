// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class DrawableProfileRow : Container
    {
        private const int fade_duration = 200;

        private Box underscoreLine;
        private readonly Box coloredBackground;
        private readonly Container background;

        /// <summary>
        /// A visual element displayed to the left of <see cref="LeftFlowContainer"/> content.
        /// </summary>
        protected abstract Drawable CreateLeftVisual();

        protected FillFlowContainer LeftFlowContainer { get; private set; }
        protected FillFlowContainer RightFlowContainer { get; private set; }

        protected override Container<Drawable> Content { get; }

        protected DrawableProfileRow()
        {
            RelativeSizeAxes = Axes.X;
            Height = 60;
            InternalChildren = new Drawable[]
            {
                background = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 3,
                    Alpha = 0,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0f, 1f),
                        Radius = 1f,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Child = coloredBackground = new Box { RelativeSizeAxes = Axes.Both }
                },
                Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 0.97f,
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colour)
        {
            AddRange(new Drawable[]
            {
                underscoreLine = new Box
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new[]
                    {
                        CreateLeftVisual(),
                        LeftFlowContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = 10 },
                            Direction = FillDirection.Vertical,
                        },
                    }
                },
                RightFlowContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Vertical,
                },
            });

            coloredBackground.Colour = underscoreLine.Colour = colour.Gray4;
        }

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeIn(fade_duration, Easing.OutQuint);
            underscoreLine.FadeOut(fade_duration, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeOut(fade_duration, Easing.OutQuint);
            underscoreLine.FadeIn(fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
