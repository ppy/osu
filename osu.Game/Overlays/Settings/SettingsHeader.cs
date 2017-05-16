// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsHeader : Container
    {
        public SearchTextBox SearchTextBox;

        private Box background;

        private readonly Func<float> currentScrollOffset;

        public Action Exit;

        /// <param name="currentScrollOffset">A reference to the current scroll position of the ScrollContainer we are contained within.</param>
        public SettingsHeader(Func<float> currentScrollOffset)
        {
            this.currentScrollOffset = currentScrollOffset;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                background = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "settings",
                            TextSize = 40,
                            Margin = new MarginPadding {
                                Left = SettingsOverlay.CONTENT_MARGINS,
                                Top = Toolbar.Toolbar.TOOLTIP_HEIGHT
                            },
                        },
                        new OsuSpriteText
                        {
                            Colour = colours.Pink,
                            Text = "Change the way osu! behaves",
                            TextSize = 18,
                            Margin = new MarginPadding {
                                Left = SettingsOverlay.CONTENT_MARGINS,
                                Bottom = 30
                            },
                        },
                        SearchTextBox = new SearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Width = 0.95f,
                            Margin = new MarginPadding {
                                Top = 20,
                                Bottom = 20
                            },
                            Exit = () => Exit(),
                        },
                    }
                }
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // the point at which we will start anchoring to the top.
            float anchorOffset = SearchTextBox.Y;

            float scrollPosition = currentScrollOffset();

            // we want to anchor the search field to the top of the screen when scrolling.
            Margin = new MarginPadding { Top = Math.Max(0, scrollPosition - anchorOffset) };

            // we don't want the header to scroll when scrolling beyond the upper extent.
            Y = Math.Min(0, scrollPosition);

            // we get darker as scroll progresses
            background.Alpha = Math.Min(1, scrollPosition / anchorOffset) * 0.5f;
        }
    }
}