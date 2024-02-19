// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
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
    public partial class SettingsToolboxGroup : Container, IExpandable
    {
        private readonly string title;
        public const int CONTAINER_WIDTH = 270;

        private const float transition_duration = 250;
        private const int header_height = 30;
        private const int corner_radius = 5;

        private readonly Cached headerTextVisibilityCache = new Cached();

        protected override Container<Drawable> Content => content;

        private readonly FillFlowContainer content = new FillFlowContainer
        {
            Name = @"Content",
            Origin = Anchor.TopCentre,
            Anchor = Anchor.TopCentre,
            Direction = FillDirection.Vertical,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding { Horizontal = 10, Top = 5, Bottom = 10 },
            Spacing = new Vector2(0, 15),
        };

        public BindableBool Expanded { get; } = new BindableBool(true);

        private OsuSpriteText headerText = null!;

        private Container headerContent = null!;

        private Box background = null!;

        private IconButton expandButton = null!;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="title">The title to be displayed in the header of this group.</param>
        public SettingsToolboxGroup(string title)
        {
            this.title = title;

            AutoSizeAxes = Axes.Y;
            Width = CONTAINER_WIDTH;
            Masking = true;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider)
        {
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.1f,
                    Colour = colourProvider?.Background4 ?? Color4.Black,
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
                                expandButton = new IconButton
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
                        content
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => updateExpandedState(true));
            updateExpandedState(false);

            this.Delay(600).Schedule(updateFadeState);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateFadeState();
            updateExpandedState(true);
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateFadeState();
            updateExpandedState(true);
            base.OnHoverLost(e);
        }

        protected override void Update()
        {
            base.Update();

            if (!headerTextVisibilityCache.IsValid)
            {
                // These toolbox grouped may be contracted to only show icons.
                // For now, let's hide the header to avoid text truncation weirdness in such cases.
                headerText.FadeTo(headerText.DrawWidth < DrawWidth ? 1 : 0, 150, Easing.OutQuint);
                headerTextVisibilityCache.Validate();
            }
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            if (invalidation.HasFlagFast(Invalidation.DrawSize))
                headerTextVisibilityCache.Invalidate();

            return base.OnInvalidate(invalidation, source);
        }

        private void updateExpandedState(bool animate)
        {
            // clearing transforms is necessary to avoid a previous height transform
            // potentially continuing to get processed while content has changed to autosize.
            content.ClearTransforms();

            if (Expanded.Value || IsHovered)
            {
                content.AutoSizeAxes = Axes.Y;
                content.AutoSizeDuration = animate ? transition_duration : 0;
                content.AutoSizeEasing = Easing.OutQuint;
            }
            else
            {
                content.AutoSizeAxes = Axes.None;
                content.ResizeHeightTo(0, animate ? transition_duration : 0, Easing.OutQuint);
            }

            headerContent.FadeColour(Expanded.Value ? Color4.White : OsuColour.Gray(0.5f), 200, Easing.OutQuint);
        }

        private void updateFadeState()
        {
            const float fade_duration = 500;

            background.FadeTo(IsHovered ? 1 : 0.1f, fade_duration, Easing.OutQuint);
            expandButton.FadeTo(IsHovered ? 1 : 0, fade_duration, Easing.OutQuint);
        }
    }
}
