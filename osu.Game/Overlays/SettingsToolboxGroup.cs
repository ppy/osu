// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class SettingsToolboxGroup : Container, IExpandable
    {
        public const int CONTAINER_WIDTH = 270;

        private const float transition_duration = 250;
        private const int border_thickness = 2;
        private const int header_height = 30;
        private const int corner_radius = 5;

        private const float fade_duration = 800;
        private const float inactive_alpha = 0.5f;

        private readonly Cached headerTextVisibilityCache = new Cached();

        private readonly FillFlowContainer content;

        public BindableBool Expanded { get; } = new BindableBool(true);

        private readonly OsuSpriteText headerText;

        private readonly Container headerContent;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="title">The title to be displayed in the header of this group.</param>
        public SettingsToolboxGroup(string title)
        {
            AutoSizeAxes = Axes.Y;
            Width = CONTAINER_WIDTH;
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
                        headerContent = new Container
                        {
                            Name = @"Header",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = header_height,
                            Children = new Drawable[]
                            {
                                headerText = new OsuSpriteText
                                {
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    Text = title.ToUpperInvariant(),
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17),
                                    Padding = new MarginPadding { Left = 10, Right = 30 },
                                },
                                new IconButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-15, 0),
                                    Icon = FontAwesome.Solid.Bars,
                                    Scale = new Vector2(0.75f),
                                    Action = () => Expanded.Toggle(),
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(updateExpandedState, true);

            this.Delay(600).Schedule(updateFadeState);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateFadeState();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateFadeState();
            base.OnHoverLost(e);
        }

        protected override void Update()
        {
            base.Update();

            if (!headerTextVisibilityCache.IsValid)
                // These toolbox grouped may be contracted to only show icons.
                // For now, let's hide the header to avoid text truncation weirdness in such cases.
                headerText.FadeTo(headerText.DrawWidth < DrawWidth ? 1 : 0, 150, Easing.OutQuint);
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            if (invalidation.HasFlagFast(Invalidation.DrawSize))
                headerTextVisibilityCache.Invalidate();

            return base.OnInvalidate(invalidation, source);
        }

        private void updateExpandedState(ValueChangedEvent<bool> expanded)
        {
            // clearing transforms is necessary to avoid a previous height transform
            // potentially continuing to get processed while content has changed to autosize.
            content.ClearTransforms();

            if (expanded.NewValue)
                content.AutoSizeAxes = Axes.Y;
            else
            {
                content.AutoSizeAxes = Axes.None;
                content.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            }

            headerContent.FadeColour(expanded.NewValue ? Color4.White : OsuColour.Gray(0.5f), 200, Easing.OutQuint);
        }

        private void updateFadeState()
        {
            this.FadeTo(IsHovered ? 1 : inactive_alpha, fade_duration, Easing.OutQuint);
        }

        protected override Container<Drawable> Content => content;
    }
}
