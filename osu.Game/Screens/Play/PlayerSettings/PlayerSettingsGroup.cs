// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public abstract class PlayerSettingsGroup : Container
    {
        /// <summary>
        /// The title to be displayed in the header of this group.
        /// </summary>
        protected abstract string Title { get; }

        private const float transition_duration = 250;
        private const int container_width = 270;
        private const int border_thickness = 2;
        private const int header_height = 30;
        private const int corner_radius = 5;

        private readonly FillFlowContainer content;
        private readonly IconButton button;

        private bool expanded = true;

        public bool Expanded
        {
            get => expanded;
            set
            {
                if (expanded == value) return;

                expanded = value;

                content.ClearTransforms();

                if (expanded)
                    content.AutoSizeAxes = Axes.Y;
                else
                {
                    content.AutoSizeAxes = Axes.None;
                    content.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
                }

                updateExpanded();
            }
        }

        private Color4 expandedColour;

        protected PlayerSettingsGroup()
        {
            AutoSizeAxes = Axes.Y;
            Width = container_width;
            Masking = true;
            CornerRadius = corner_radius;
            BorderColour = Color4.Black;
            BorderThickness = border_thickness;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = @"Header",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = header_height,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    Text = Title.ToUpperInvariant(),
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17),
                                    Margin = new MarginPadding { Left = 10 },
                                },
                                button = new IconButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-15, 0),
                                    Icon = FontAwesome.Solid.Bars,
                                    Scale = new Vector2(0.75f),
                                    Action = () => Expanded = !Expanded,
                                },
                            }
                        },
                        content = new FillFlowContainer
                        {
                            Name = @"Content",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeDuration = transition_duration,
                            AutoSizeEasing = Easing.OutQuint,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(15),
                            Spacing = new Vector2(0, 15),
                        }
                    }
                },
            };
        }

        private const float fade_duration = 800;
        private const float inactive_alpha = 0.5f;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.Delay(600).FadeTo(inactive_alpha, fade_duration, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.FadeIn(fade_duration, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.FadeTo(inactive_alpha, fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            expandedColour = colours.Yellow;

            updateExpanded();
        }

        private void updateExpanded() => button.FadeColour(expanded ? expandedColour : Color4.White, 200, Easing.InOutQuint);

        protected override Container<Drawable> Content => content;

        protected override bool OnMouseDown(MouseDownEvent e) => true;
    }
}
