// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class StreamBadge : ClickableContainer
    {
        private const float badge_height = 66.5f;
        private const float badge_width = 100;
        private const float transition_duration = 100;

        public delegate void SelectedHandler(StreamBadge source, EventArgs args);

        public event SelectedHandler Selected;

        private bool isActivated;

        private readonly LineBadge lineBadge;
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        public readonly APIChangelog LatestBuild;
        private readonly FillFlowContainer<SpriteText> text;

        public StreamBadge(APIChangelog latestBuild)
        {
            LatestBuild = latestBuild;
            Height = badge_height;
            Width = LatestBuild.IsFeatured ? badge_width * 2 : badge_width;
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
                        new SpriteText
                        {
                            Text = LatestBuild.UpdateStream.DisplayName,
                            Font = @"Exo2.0-Bold",
                            TextSize = 14, // web: 12,
                            Margin = new MarginPadding { Top = 6 },
                        },
                        new SpriteText
                        {
                            Text = LatestBuild.DisplayVersion,
                            Font = @"Exo2.0-Light",
                            TextSize = 20, // web: 16,
                        },
                        new SpriteText
                        {
                            Text = LatestBuild.Users > 0 ?
                                $"{LatestBuild.Users:N0} users online" :
                                null,
                            TextSize = 12, // web: 10,
                            Font = @"Exo2.0-Regular",
                            Colour = new Color4(203, 164, 218, 255),
                        },
                    }
                },
                lineBadge = new LineBadge(false)
                {
                    Anchor = Anchor.TopCentre,
                    Colour = StreamColour.FromStreamName(LatestBuild.UpdateStream.Name),
                    UncollapsedSize = 4,
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
            lineBadge.Uncollapse();
            if (!withoutFiringUpdates)
                Selected?.Invoke(this, EventArgs.Empty);
        }

        public void Deactivate()
        {
            isActivated = false;
            DisableDim();
            if (!IsHovered)
            {
                this.FadeTo(0.5f, transition_duration);
                lineBadge.Collapse(200);
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
            lineBadge.Uncollapse();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!isActivated)
            {
                this.FadeTo(0.5f, transition_duration);
                lineBadge.Collapse(200);
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
