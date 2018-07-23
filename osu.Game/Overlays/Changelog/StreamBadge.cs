// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class StreamBadge : ClickableContainer
    {
        private const float badge_height = 56.5f;
        private const float badge_width = 100;
        private const float transition_duration = 100;

        public delegate void SelectedHandler(StreamBadge source, EventArgs args);

        public event SelectedHandler Selected;

        private bool isActivated;

        private readonly Header.LineBadge lineBadge;
        private SampleChannel sampleHover;
        public readonly APIChangelog ChangelogEntry;
        private readonly FillFlowContainer<SpriteText> text;

        public StreamBadge(APIChangelog changelogEntry)
        {
            ChangelogEntry = changelogEntry;
            Height = badge_height;
            Width = ChangelogEntry.IsFeatured ? badge_width * 2 : badge_width;
            Margin = new MarginPadding(5);
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
                            Text = ChangelogEntry.UpdateStream.DisplayName,
                            Font = @"Exo2.0-Bold",
                            TextSize = 16,
                            Margin = new MarginPadding
                            {
                                Top = 7,
                            }
                        },
                        new SpriteText
                        {
                            Text = ChangelogEntry.DisplayVersion,
                            Font = @"Exo2.0-Light",
                            TextSize = 21,
                        },
                        new SpriteText
                        {
                            Text = ChangelogEntry.Users > 0 ?
                                $"{ChangelogEntry.Users:N0} users online" :
                                null,
                            TextSize = 12,
                            Font = @"Exo2.0-Regular",
                            Colour = new Color4(203, 164, 218, 255),
                        },
                    }
                },
                lineBadge = new Header.LineBadge(false, 2, 4)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Colour = StreamColour.FromStreamName(ChangelogEntry.UpdateStream.Name),
                    RelativeSizeAxes = Axes.X,
                },
            };
        }

        public void Activate(bool withoutFiringUpdates = true)
        {
            isActivated = true;
            this.FadeIn(transition_duration);
            lineBadge.IsCollapsed = false;
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
                lineBadge.IsCollapsed = true;
            }
        }

        protected override bool OnClick(InputState state)
        {
            Activate(false);
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            sampleHover?.Play();
            DisableDim();
            this.FadeIn(transition_duration);
            lineBadge.IsCollapsed = false;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!isActivated)
            {
                this.FadeTo(0.5f, transition_duration);
                lineBadge.IsCollapsed = true;
            }
            else
                EnableDim();
            base.OnHoverLost(state);
        }

        public void EnableDim() => text.FadeTo(0.5f, transition_duration);

        public void DisableDim() => text.FadeIn(transition_duration);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
        }
    }
}
