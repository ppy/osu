// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePair : Container
    {
        protected SpriteText Text;
        protected LineBadge LineBadge;
        public bool IsActivated { get; protected set; }

        public delegate void ActivatedEventHandler(object source, EventArgs args);

        public event ActivatedEventHandler Activated;

        private SampleChannel sampleHover;
        private SampleChannel sampleActivate;

        public TextBadgePair(ColourInfo badgeColour, string displayText = "Listing", bool startCollapsed = true)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                Text = new SpriteText
                {
                    TextSize = 21, // web: 16,
                    Text = displayText,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Top = 5, Bottom = 15 },
                },
                LineBadge = new LineBadge(startCollapsed)
                {
                    CollapsedSize = 2,
                    UncollapsedSize = 10,
                    Colour = badgeColour,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                }
            };
        }

        /// <param name="duration">
        /// The duration of popping in and popping out not combined.
        /// Full change takes double this time.</param>
        public void ChangeText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            LineBadge.Collapse();
            Text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing)
                .Then()
                .MoveToY(0, duration, easing)
                .FadeIn(duration, easing);

            // since using .finally/.oncomplete after first fadeout made the badge not hide
            // sometimes in visual tests (https://streamable.com/0qssq), I'm using a scheduler here
            Scheduler.AddDelayed(() =>
            {
                if (!string.IsNullOrEmpty(displayText))
                    Text.Text = displayText;
                LineBadge.Uncollapse();
            }, duration);
        }

        public virtual void Deactivate()
        {
            IsActivated = false;
            LineBadge.Collapse();
            Text.Font = "Exo2.0-Regular";
        }

        public virtual void Activate()
        {
            IsActivated = true;
            LineBadge.Uncollapse();
            Text.Font = "Exo2.0-Bold";
        }

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
            LineBadge.Collapse();
            Text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing);
        }

        public void ShowText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            LineBadge.Uncollapse();
            if (!string.IsNullOrEmpty(displayText))
                Text.Text = displayText;
            Text.MoveToY(0, duration, easing)
                .FadeIn(duration, easing);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!IsActivated)
                sampleHover?.Play();
            return base.OnHover(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            OnActivated();
            sampleActivate?.Play();
            return base.OnClick(e);
        }

        protected virtual void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
            sampleActivate = audio.Sample.Get(@"UI/generic-select-soft");
        }
    }
}
