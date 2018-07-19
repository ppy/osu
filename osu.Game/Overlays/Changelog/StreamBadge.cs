// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
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

        public Action OnActivation;

        private bool isActivated;

        private readonly Header.LineBadge lineBadge;
        private SampleChannel sampleHover;
        public readonly APIChangelog ChangelogEntry;

        public StreamBadge(APIChangelog changelogEntry)
        {
            ChangelogEntry = changelogEntry;
            Height = badge_height;
            Width = ChangelogEntry.IsFeatured ? badge_width * 2 : badge_width;
            Margin = new MarginPadding(5);
            isActivated = true;
            Children = new Drawable[]
            {
                new FillFlowContainer<SpriteText>
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
                                string.Join(" ", ChangelogEntry.Users.ToString("N0"), "users online"):
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

        public void Activate(bool withoutHeaderUpdate = false)
        {
            isActivated = true;
            this.FadeIn(transition_duration);
            lineBadge.IsCollapsed = false;
            if (!withoutHeaderUpdate) OnActivation?.Invoke();
        }

        public void Deactivate()
        {
            isActivated = false;
            if (!IsHovered)
            {
                this.FadeTo(0.5f, transition_duration);
                lineBadge.IsCollapsed = true;
            }
        }

        protected override bool OnClick(InputState state)
        {
            Activate();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            if (!isActivated) sampleHover?.Play();
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
            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
        }
    }
}
