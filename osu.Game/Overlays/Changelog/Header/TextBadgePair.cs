// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using System;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePair : Container
    {
        protected SpriteText Text;
        protected LineBadge LineBadge;
        public bool IsActivated { get; protected set; }

        public Action OnActivation;
        public Action OnDeactivation;
        private SampleChannel sampleHover;
        protected SampleChannel SampleActivate;

        public void SetTextColour(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            Text.FadeColour(newColour, duration, easing);
        }

        public void SetBadgeColour(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            LineBadge.FadeColour(newColour, duration, easing);
        }

        public void HideText(double duration = 0, Easing easing = Easing.InOutCubic)
        {
            LineBadge.IsCollapsed = true;
            Text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing);
        }

        public void ShowText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            LineBadge.IsCollapsed = false;
            if (!string.IsNullOrEmpty(displayText)) Text.Text = displayText;
            Text.MoveToY(0, duration, easing)
                .FadeIn(duration, easing);
        }

        /// <param name="duration">
        /// The duration of popping in and popping out not combined.
        /// Full change takes double this time.</param>
        public void ChangeText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            LineBadge.IsCollapsed = true;
            Text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing)
                .Then()
                .MoveToY(0, duration, easing)
                .FadeIn(duration, easing);

            // since using .finally/.oncomplete after first fadeout made the badge
            // not hide sometimes in visual tests(because FinishTransforms()/CancelTransforms()
            // didn't apply to transforms that come after the .finally), I'm using a scheduler here
            Scheduler.AddDelayed(() =>
            {
                if (!string.IsNullOrEmpty(displayText)) Text.Text = displayText;
                LineBadge.IsCollapsed = false;
            }, duration);
        }

        public TextBadgePair(ColourInfo badgeColour, string displayText = "Listing", bool startCollapsed = true)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                Text = new SpriteText
                {
                    TextSize = 21, // web is 16, but here it looks too small?
                    Text = displayText,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding
                    {
                        Top = 5,
                        Bottom = 15,
                    }
                },
                LineBadge = new LineBadge(startCollapsed)
                {
                    Width = 1,
                    Colour = badgeColour,
                    RelativeSizeAxes = Axes.X,
                }
            };
        }

        public virtual void Deactivate()
        {
            IsActivated = false;
            LineBadge.IsCollapsed = true;
            Text.Font = "Exo2.0-Regular";
        }

        public virtual void Activate()
        {
            IsActivated = true;
            LineBadge.IsCollapsed = false;
            Text.Font = "Exo2.0-Bold";
            SampleActivate?.Play();
        }

        protected override bool OnHover(InputState state)
        {
            if (!IsActivated) sampleHover?.Play();
            return base.OnHover(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
            SampleActivate = audio.Sample.Get(@"UI/generic-select-soft");
        }
    }
}
