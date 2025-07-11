// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Edit
{
    public partial class EditorToolboxGroup : Container, IExpandable
    {
        private readonly string title;

        private const float transition_duration = 250;
        private const int header_height = 24;
        private const int corner_radius = 4;

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
            Spacing = new Vector2(0, 8),
        };

        public MarginPadding ContentPadding
        {
            get => content.Padding;
            set => content.Padding = value;
        }

        public BindableBool Expanded { get; } = new BindableBool(true);

        private OsuSpriteText headerText = null!;

        private Container headerContent = null!;

        private Box background = null!;

        private IconButton expandButton = null!;

        private InputManager inputManager = null!;

        private Drawable? draggedChild;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="title">The title to be displayed in the header of this group.</param>
        public EditorToolboxGroup(string title)
        {
            this.title = title;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
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
                                    Font = Editor.Fonts.Heading,
                                    Padding = new MarginPadding { Left = ContentPadding.Left, Right = 24 },
                                },
                                expandButton = new IconButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-(ContentPadding.Right + 5), 0),
                                    Icon = FontAwesome.Solid.Bars,
                                    Scale = new Vector2(0.65f),
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

            inputManager = GetContainingInputManager()!;

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

            // These toolbox grouped may be contracted to only show icons.
            // For now, let's hide the header to avoid text truncation weirdness in such cases.
            headerText.Alpha = (float)Interpolation.DampContinuously(headerText.Alpha, headerText.DrawWidth < DrawWidth ? 1 : 0, 40, Time.Elapsed);

            // Dragged child finished its drag operation.
            if (draggedChild != null && inputManager.DraggedDrawable != draggedChild)
            {
                draggedChild = null;
                updateExpandedState(true);
            }
        }

        private void updateExpandedState(bool animate)
        {
            // before we collapse down, let's double check the user is not dragging a UI control contained within us.
            if (inputManager.DraggedDrawable.IsRootedAt(this))
            {
                draggedChild = inputManager.DraggedDrawable;
            }

            // clearing transforms is necessary to avoid a previous height transform
            // potentially continuing to get processed while content has changed to autosize.
            content.ClearTransforms();

            if (Expanded.Value || IsHovered || draggedChild != null)
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

            headerContent.FadeColour(Expanded.Value ? Color4.White : OsuColour.Gray(0.7f), 200, Easing.OutQuint);
        }

        private void updateFadeState()
        {
            const float fade_duration = 500;

            background.FadeTo(IsHovered ? 1 : 0.1f, fade_duration, Easing.OutQuint);
            expandButton.FadeTo(IsHovered ? 1 : 0, fade_duration, Easing.OutQuint);
        }
    }
}
