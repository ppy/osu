// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using System;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Changelog.Header
{
    public abstract class Breadcrumb : Container
    {
        protected SpriteText Text;
        protected LineBadge LineBadge;

        public bool IsActivated { get; protected set; }

        public Action Action;

        private SampleChannel sampleHover;
        private SampleChannel sampleActivate;

        protected Breadcrumb(ColourInfo badgeColour, string displayText = "Listing", bool startCollapsed = true)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                Text = new SpriteText
                {
                    Font = OsuFont.GetFont(size: 21), // web: 16,
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

        public virtual void Deactivate()
        {
            if (!IsActivated)
                return;

            IsActivated = false;
            LineBadge.Collapse();
            Text.Font = Text.Font.With(weight: FontWeight.Regular);
        }

        public virtual void Activate()
        {
            if (IsActivated)
                return;

            IsActivated = true;
            LineBadge.Uncollapse();
            Text.Font = Text.Font.With(weight: FontWeight.Bold);
        }

        public void SetTextColour(ColourInfo newColour, double duration = 0, Easing easing = Easing.None)
        {
            Text.FadeColour(newColour, duration, easing);
        }

        public void HideText(double duration = 0, Easing easing = Easing.InOutCubic)
        {
            LineBadge.Collapse();
            Text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing);
        }

        public void ShowText(double duration = 0, string displayText = null, Easing easing = Easing.InOutCubic)
        {
            LineBadge.Collapse();
            Text.MoveToY(20, duration, easing)
                .FadeOut(duration, easing)
                .Then()
                .MoveToY(0, duration, easing)
                .FadeIn(duration, easing);

            Scheduler.AddDelayed(() =>
            {
                Text.Text = displayText;
                LineBadge.Uncollapse();
            }, duration);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!IsActivated)
                sampleHover?.Play();
            return base.OnHover(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            Activate();
            sampleActivate?.Play();

            return true;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
            sampleActivate = audio.Sample.Get(@"UI/generic-select-soft");
        }
    }
}
