// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class UpdateStreamBadge : TabItem<APIUpdateStream>
    {
        private const float badge_height = 66.5f;
        private const float badge_width = 100;
        private const float transition_duration = 100;

        private readonly ExpandingBar expandingBar;
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        private readonly FillFlowContainer<SpriteText> text;

        public readonly Bindable<APIUpdateStream> SelectedTab = new Bindable<APIUpdateStream>();

        private readonly Container fadeContainer;

        public UpdateStreamBadge(APIUpdateStream stream)
            : base(stream)
        {
            Size = new Vector2(stream.IsFeatured ? badge_width * 2 : badge_width, badge_height);
            Padding = new MarginPadding(5);

            Child = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
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
                                Text = stream.LatestBuild.Users > 0 ? $"{stream.LatestBuild.Users:N0} {"user".Pluralize(stream.LatestBuild.Users == 1)} online" : null,
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
                        IsCollapsed = true
                    },
                }
            };

            SelectedTab.BindValueChanged(_ => updateState(), true);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Samples.Get(@"UI/generic-select-soft");
            sampleHover = audio.Samples.Get(@"UI/generic-hover-soft");
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        protected override bool OnClick(ClickEvent e)
        {
            sampleClick?.Play();
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            sampleHover?.Play();
            updateState();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            // Expand based on the local state
            bool shouldExpand = Active.Value || IsHovered;

            // Expand based on whether no build is selected and the badge area is hovered
            shouldExpand |= SelectedTab.Value == null && !externalDimRequested;

            if (shouldExpand)
            {
                expandingBar.Expand();
                fadeContainer.FadeTo(1, transition_duration);
            }
            else
            {
                expandingBar.Collapse();
                fadeContainer.FadeTo(0.5f, transition_duration);
            }

            text.FadeTo(externalDimRequested && !IsHovered ? 0.5f : 1, transition_duration);
        }

        private bool externalDimRequested;

        public void EnableDim()
        {
            externalDimRequested = true;
            updateState();
        }

        public void DisableDim()
        {
            externalDimRequested = false;
            updateState();
        }
    }
}
