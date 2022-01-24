// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Graphics.Containers;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using System.Diagnostics;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.News.Sidebar
{
    public class MonthSection : CompositeDrawable
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public readonly BindableBool Expanded = new BindableBool();

        private const int animation_duration = 250;
        private Sample sampleOpen;
        private Sample sampleClose;

        public MonthSection(int month, int year, IEnumerable<APINewsPost> posts)
        {
            Debug.Assert(posts.All(p => p.PublishedAt.Month == month && p.PublishedAt.Year == year));

            Year = year;
            Month = month;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new DropdownHeader(month, year)
                    {
                        Expanded = { BindTarget = Expanded }
                    },
                    new PostsContainer
                    {
                        Expanded = { BindTarget = Expanded },
                        Children = posts.Select(p => new PostButton(p)).ToArray()
                    }
                }
            };

            Expanded.ValueChanged += expanded =>
            {
                if (expanded.NewValue)
                    sampleOpen?.Play();
                else
                    sampleClose?.Play();
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOpen = audio.Samples.Get(@"UI/dropdown-open");
            sampleClose = audio.Samples.Get(@"UI/dropdown-close");
        }

        private class DropdownHeader : OsuClickableContainer
        {
            public readonly BindableBool Expanded = new BindableBool();

            private readonly SpriteIcon icon;

            public DropdownHeader(int month, int year)
            {
                var date = new DateTime(year, month, 1);

                RelativeSizeAxes = Axes.X;
                Height = 15;
                Action = Expanded.Toggle;
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                        Text = date.ToString("MMM yyyy")
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(10),
                        Icon = FontAwesome.Solid.ChevronDown
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Expanded.BindValueChanged(open =>
                {
                    icon.Scale = new Vector2(1, open.NewValue ? -1 : 1);
                }, true);
            }
        }

        private class PostButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { text };

            private readonly TextFlowContainer text;
            private readonly APINewsPost post;

            public PostButton(APINewsPost post)
                : base(HoverSampleSet.Submit)
            {
                this.post = post;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Child = text = new TextFlowContainer(t => t.Font = OsuFont.GetFont(size: 12))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = post.Title
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider overlayColours, GameHost host)
            {
                IdleColour = overlayColours.Light2;
                HoverColour = overlayColours.Light1;

                TooltipText = "view in browser";
                Action = () => host.OpenUrlExternally("https://osu.ppy.sh/home/news/" + post.Slug);
            }
        }

        private class PostsContainer : Container
        {
            public readonly BindableBool Expanded = new BindableBool();

            protected override Container<Drawable> Content { get; }

            public PostsContainer()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                AutoSizeDuration = animation_duration;
                AutoSizeEasing = Easing.Out;
                InternalChild = Content = new FillFlowContainer
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Alpha = 0
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Expanded.BindValueChanged(updateState, true);
            }

            private void updateState(ValueChangedEvent<bool> expanded)
            {
                ClearTransforms(true);

                if (expanded.NewValue)
                {
                    AutoSizeAxes = Axes.Y;
                    Content.FadeIn(animation_duration, Easing.OutQuint);
                }
                else
                {
                    AutoSizeAxes = Axes.None;
                    this.ResizeHeightTo(0, animation_duration, Easing.OutQuint);

                    Content.FadeOut(animation_duration, Easing.OutQuint);
                }
            }
        }
    }
}
