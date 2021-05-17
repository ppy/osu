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
using osu.Framework.Platform;

namespace osu.Game.Overlays.News.Sidebar
{
    public class MonthSection : CompositeDrawable
    {
        private const int animation_duration = 250;

        public readonly BindableBool IsOpen = new BindableBool();

        public MonthSection(int month, int year, IEnumerable<APINewsPost> posts)
        {
            Debug.Assert(posts.All(p => p.PublishedAt.Month == month && p.PublishedAt.Year == year));

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
                        IsOpen = { BindTarget = IsOpen }
                    },
                    new PostsContainer
                    {
                        IsOpen = { BindTarget = IsOpen },
                        Children = posts.Select(p => new PostButton(p)).ToArray()
                    }
                }
            };
        }

        private class DropdownHeader : OsuClickableContainer
        {
            public readonly BindableBool IsOpen = new BindableBool();

            private readonly SpriteIcon icon;

            public DropdownHeader(int month, int year)
            {
                var date = new DateTime(year, month, 1);

                RelativeSizeAxes = Axes.X;
                Height = 15;
                Action = IsOpen.Toggle;
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

                IsOpen.BindValueChanged(open =>
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
            private void load(OverlayColourProvider colourProvider, GameHost host)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;

                TooltipText = "view in browser";
                Action = () => host.OpenUrlExternally("https://osu.ppy.sh/home/news/" + post.Slug);
            }
        }

        private class PostsContainer : Container
        {
            public readonly BindableBool IsOpen = new BindableBool();

            protected override Container<Drawable> Content => content;

            private readonly FillFlowContainer content;

            public PostsContainer()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChild = content = new FillFlowContainer
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5)
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                IsOpen.BindValueChanged(open =>
                {
                    ClearTransforms(true);

                    if (open.NewValue)
                    {
                        AutoSizeAxes = Axes.Y;
                        content.FadeIn(animation_duration, Easing.OutQuint);
                    }
                    else
                    {
                        AutoSizeAxes = Axes.None;
                        this.ResizeHeightTo(0, animation_duration, Easing.OutQuint);

                        content.FadeOut(animation_duration, Easing.OutQuint);
                    }
                }, true);

                // First state change should be instant.
                FinishTransforms(true);
            }

            private bool shouldUpdateAutosize = true;

            // Workaround to allow the dropdown to be opened immediately since FinishTransforms doesn't work for AutosizeDuration.
            protected override void UpdateAfterAutoSize()
            {
                base.UpdateAfterAutoSize();

                if (shouldUpdateAutosize)
                {
                    AutoSizeDuration = animation_duration;
                    AutoSizeEasing = Easing.OutQuint;

                    shouldUpdateAutosize = false;
                }
            }
        }
    }
}
