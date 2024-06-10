// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Settings
{
    public partial class SidebarIconButton : SidebarButton
    {
        private const float selection_indicator_height_active = 18;
        private const float selection_indicator_height_inactive = 4;

        private readonly ConstrainedIconContainer iconContainer;
        private readonly SpriteText headerText;
        private readonly CircularContainer selectionIndicator;
        private readonly Container textIconContent;

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
                    UpdateState();
            }
        }

        public SidebarIconButton()
        {
            RelativeSizeAxes = Axes.X;
            Height = 46;

            Padding = new MarginPadding(5);

            AddRange(new Drawable[]
            {
                textIconContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.6f),
                    Children = new Drawable[]
                    {
                        iconContainer = new ConstrainedIconContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(20),
                            Margin = new MarginPadding { Left = 25 }
                        },
                        headerText = new OsuSpriteText
                        {
                            Position = new Vector2(60, 0),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    }
                },
                selectionIndicator = new CircularContainer
                {
                    Alpha = 0,
                    Width = 4,
                    Height = selection_indicator_height_inactive,
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
            selectionIndicator.Colour = ColourProvider.Highlight1;
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            if (Selected)
            {
                textIconContent.FadeColour(ColourProvider.Content1, FADE_DURATION, Easing.OutQuint);

                selectionIndicator.FadeIn(FADE_DURATION, Easing.OutQuint);
                selectionIndicator.ResizeHeightTo(selection_indicator_height_active, FADE_DURATION, Easing.OutElasticHalf);
            }
            else
            {
                textIconContent.FadeColour(IsHovered ? ColourProvider.Light1 : ColourProvider.Light3, FADE_DURATION, Easing.OutQuint);

                selectionIndicator.FadeOut(FADE_DURATION, Easing.OutQuint);
                selectionIndicator.ResizeHeightTo(selection_indicator_height_inactive, FADE_DURATION, Easing.OutQuint);
            }
        }
    }
}
