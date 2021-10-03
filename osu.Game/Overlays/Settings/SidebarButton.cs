// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Settings
{
    public class SidebarButton : OsuButton
    {
        private const double fade_duration = 50;

        private readonly ConstrainedIconContainer iconContainer;
        private readonly SpriteText headerText;
        private readonly CircularContainer selectionIndicator;
        private readonly Container text;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        // always consider as part of flow, even when not visible (for the sake of the initial animation).
        public override bool IsPresent => true;

        private SettingsSection section;

        public SettingsSection Section
        {
            get => section;
            set
            {
                section = value;
                headerText.Text = value.Header;
                iconContainer.Icon = value.CreateIcon();
            }
        }

        private bool selected;

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;

                if (IsLoaded)
                    updateState();
            }
        }

        public SidebarButton()
        {
            RelativeSizeAxes = Axes.X;
            Height = 46;

            AddRange(new Drawable[]
            {
                text = new Container
                {
                    Width = Sidebar.DEFAULT_WIDTH,
                    RelativeSizeAxes = Axes.Y,
                    Colour = OsuColour.Gray(0.6f),
                    Children = new Drawable[]
                    {
                        headerText = new OsuSpriteText
                        {
                            Position = new Vector2(Sidebar.DEFAULT_WIDTH + 10, 0),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        iconContainer = new ConstrainedIconContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(20),
                        },
                    }
                },
                selectionIndicator = new CircularContainer
                {
                    Alpha = 0,
                    Width = 3,
                    Height = 18,
                    Masking = true,
                    CornerRadius = 1.5f,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding
                    {
                        Left = 9,
                    },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White
                    }
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = colourProvider.Background5;
            selectionIndicator.Colour = colourProvider.Highlight1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e) => updateState();

        private void updateState()
        {
            if (Selected)
            {
                text.FadeColour(colourProvider.Content1, fade_duration, Easing.OutQuint);
                selectionIndicator.FadeIn(fade_duration, Easing.OutQuint);
                return;
            }

            text.FadeColour(IsHovered ? colourProvider.Light1 : colourProvider.Light3, fade_duration, Easing.OutQuint);
            selectionIndicator.FadeOut(fade_duration, Easing.OutQuint);
        }
    }
}
