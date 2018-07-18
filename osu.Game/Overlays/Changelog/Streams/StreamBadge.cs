// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using System;

namespace osu.Game.Overlays.Changelog.Streams
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
        public readonly string Name;
        public readonly string DisplayVersion;
        public readonly bool IsFeatured;
        public readonly float Users;

        public StreamBadge(ColourInfo colour, string streamName, string streamBuild, float onlineUsers = 0, bool isFeatured = false)
        {
            Name = streamName;
            DisplayVersion = streamBuild;
            IsFeatured = isFeatured;
            Users = onlineUsers;
            Height = badge_height;
            Width = isFeatured ? badge_width * 2 : badge_width;
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
                            Text = streamName,
                            Font = @"Exo2.0-Bold",
                            TextSize = 16,
                            Margin = new MarginPadding
                            {
                                Top = 7,
                            }
                        },
                        new SpriteText
                        {
                            Text = streamBuild,
                            Font = @"Exo2.0-Light",
                            TextSize = 21,
                        },
                        new SpriteText
                        {
                            Text = onlineUsers > 0 ?
                                string.Join(" ", onlineUsers.ToString("N0"), "users online"):
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
                    Width = 1,
                    Colour = colour,
                    RelativeSizeAxes = Axes.X,
                    TransitionDuration = 600,
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
