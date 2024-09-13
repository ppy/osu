// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsSidebar : ExpandingContainer
    {
        public const float CONTRACTED_WIDTH = 70;
        public const int EXPANDED_WIDTH = 170;

        public Action? BackButtonAction;

        protected override bool ExpandOnHover => false;

        private readonly bool showBackButton;

        public SettingsSidebar(bool showBackButton)
            : base(CONTRACTED_WIDTH, EXPANDED_WIDTH)
        {
            this.showBackButton = showBackButton;
            Expanded.Value = true;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AddInternal(new Box
            {
                Colour = colourProvider.Background5,
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            if (showBackButton)
            {
                AddInternal(new BackButton
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Action = () => BackButtonAction?.Invoke(),
                });
            }
        }

        public partial class BackButton : SidebarButton
        {
            private Drawable content = null!;

            public BackButton()
                : base(HoverSampleSet.Default)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(SettingsSidebar.EXPANDED_WIDTH);

                Padding = new MarginPadding(40);

                AddRange(new[]
                {
                    content = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(30),
                                Shadow = true,
                                Icon = FontAwesome.Solid.ChevronLeft
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular),
                                Text = @"back",
                            },
                        }
                    }
                });
            }

            protected override void UpdateState()
            {
                base.UpdateState();

                content.FadeColour(IsHovered ? ColourProvider.Light1 : ColourProvider.Light3, FADE_DURATION, Easing.OutQuint);
            }
        }
    }
}
