// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePair : ClickableContainer
    {
        protected SpriteText text;
        protected LineBadge lineBadge;
        protected bool startCollapsed;

        public Action OnActivation;
        public Action OnDeactivation;

        public void SetTextColour(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            text.FadeColour(newColour, duration, easing);
        }

        public void SetBadgeColour(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            lineBadge.FadeColour(newColour, duration, easing);
        }

        public void HideText(double duration = 0, Easing easing = Easing.InOutCubic)
        {
            lineBadge.IsCollapsed = true;
            text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing);
        }

        public void ShowText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            lineBadge.IsCollapsed = false;
            if (!string.IsNullOrEmpty(displayText)) text.Text = displayText;
            text.MoveToY(0, duration, easing)
                .FadeIn(duration, easing);
        }

        /// <param name="duration">
        /// The duration of popping in and popping out not combined.
        /// Full change takes double this time.</param>
        public void ChangeText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            lineBadge.IsCollapsed = true;
            text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing)
                .Then()
                .MoveToY(0, duration, easing)
                .FadeIn(duration, easing);

            // since using .finally/.oncomplete after first fadeout made the badge
            // not hide sometimes in visual tests(because FinishTransforms()/CancelTransforms()
            // didn't apply to transforms that come after the .finally), I'm using a scheduler here
            Scheduler.AddDelayed(() =>
            {
                if (!string.IsNullOrEmpty(displayText)) text.Text = displayText;
                lineBadge.IsCollapsed = false;
            }, duration);
        }

        public TextBadgePair(ColourInfo badgeColour, string displayText = "Listing", bool startBadgeCollapsed = true)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new SpriteText
                {
                    TextSize = 21, // web is 16, but here it looks too small?
                    Text = displayText,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding()
                    {
                        Top = 5,
                        Bottom = 15,
                    }
                },
                lineBadge = new LineBadge(startCollapsed)
                {
                    Width = 1,
                    Colour = badgeColour,
                    RelativeSizeAxes = Axes.X,
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
