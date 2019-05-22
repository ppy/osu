// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class StreamBadge : ClickableContainer
    {
        private const float badge_height = 66.5f;
        private const float badge_width = 100;
        private const float transition_duration = 100;

        public event Action Selected;

        private bool isActivated;

        private readonly ExpandingBar expandingBar;
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        public readonly APIUpdateStream Stream;

        private readonly FillFlowContainer<SpriteText> text;

        public StreamBadge(APIUpdateStream stream)
        {
            Stream = stream;

            Height = badge_height;
            Width = stream.IsFeatured ? badge_width * 2 : badge_width;
            Padding = new MarginPadding(5);
            isActivated = true;
            Children = new Drawable[]
            {
                text = new FillFlowContainer<SpriteText>
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = stream.DisplayName,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12),
                            Margin = new MarginPadding { Top = 6 },
                        },
                        new OsuSpriteText
                        {
                            Text = stream.LatestBuild.DisplayVersion,
                            Font = OsuFont.GetFont(weight: FontWeight.Light, size: 16),
                        },
                        new OsuSpriteText
                        {
                            Text = stream.LatestBuild.Users > 0 ? $"{stream.LatestBuild.Users:N0} users online" : null,
                            Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 10),
                            Colour = new Color4(203, 164, 218, 255),
                        },
                    }
                },
                expandingBar = new ExpandingBar
                {
                    Anchor = Anchor.TopCentre,
                    Colour = stream.Colour,
                    ExpandedSize = 4,
                    CollapsedSize = 2,
                },
            };
        }

        /// <param name="withoutFiringUpdates">In case we don't want to
        /// fire the <see cref="Selected"/> event.</param>
        public void Activate(bool withoutFiringUpdates = true)
        {
            isActivated = true;
            this.FadeIn(transition_duration);
            expandingBar.Expand();
            if (!withoutFiringUpdates)
                Selected?.Invoke();
        }

        public void Deactivate()
        {
            isActivated = false;
            DisableDim();

            if (!IsHovered)
            {
                this.FadeTo(0.5f, transition_duration);
                expandingBar.Collapse();
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            Activate(false);
            sampleClick?.Play();
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            sampleHover?.Play();
            DisableDim();
            this.FadeIn(transition_duration);
            expandingBar.Expand();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!isActivated)
            {
                this.FadeTo(0.5f, transition_duration);
                expandingBar.Collapse();
            }
            else
                EnableDim();

            base.OnHoverLost(e);
        }

        public void EnableDim() => text.FadeTo(0.5f, transition_duration);

        public void DisableDim() => text.FadeIn(transition_duration);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get(@"UI/generic-select-soft");
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
        }
    }
}
