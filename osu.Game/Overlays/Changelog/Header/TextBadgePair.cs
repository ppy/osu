// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Overlays.Changelog;
using System;

namespace osu.Game.Overlays.Changelog.Header
{

    public class TextBadgePair : ClickableContainer
    {
        // When in listing, "Listing" is white and doesn't change on mouseover
        // when release stream is chosen, "Listing" turns purple, and lighter font
        // on mouseover, the badge scales up
        // Version name steals "Listing"'s styling

        public SpriteText text;
        public LineBadge lineBadge;

        public Action OnActivation;
        public Action OnDeactivation;

        public void SetTextColor(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            text.FadeColour(newColour, duration, easing);
        }

        public void SetBadgeColor(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            lineBadge.FadeColour(newColour, duration, easing);
        }

        public void HideText(double duration = 0, Easing easing = Easing.InOutCubic)
        {
            lineBadge.IsCollapsed = true;
            text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing);
        }

        public void ShowText(double duration = 0, Easing easing = Easing.InOutCubic)
        {
            lineBadge.IsCollapsed = false;
            text.MoveToY(0, duration, easing)
                .FadeIn(duration, easing)
                .Finally(d => lineBadge.ResizeWidthTo(text.DrawWidth, 250));
        }

        public void ChangeText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            lineBadge.IsCollapsed = true;
            text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing)
                .Finally(d =>
                {
                    lineBadge.ResizeWidthTo(0);
                    if (!string.IsNullOrEmpty(displayText)) text.Text = displayText;
                    text.MoveToY(0, duration, easing)
                        .FadeIn(duration, easing)
                        .OnComplete(dd => {
                            lineBadge.ResizeWidthTo(text.DrawWidth);
                            lineBadge.IsCollapsed = false;
                        });
                });
        }

        public TextBadgePair(ColourInfo badgeColour, string displayText = "Listing")
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new SpriteText
                {
                    TextSize = 20,
                    Text = displayText,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AlwaysPresent = true,
                    Margin = new MarginPadding()
                    {
                        Top = 5,
                        Bottom = 15,
                        Left = 10,
                        Right = 10,
                    }
                },
                lineBadge = new LineBadge
                {
                    Colour = badgeColour,
                }
            };
        }

        public virtual void Deactivate()
        {
            lineBadge.IsCollapsed = true;
            text.Font = "Exo2.0-Regular";
        }

        public virtual void Activate()
        {
            lineBadge.IsCollapsed = false;
            text.Font = "Exo2.0-Bold";
        }
    }
}
