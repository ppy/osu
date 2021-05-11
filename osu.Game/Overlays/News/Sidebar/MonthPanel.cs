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

namespace osu.Game.Overlays.News.Sidebar
{
    public class MonthPanel : CompositeDrawable
    {
        private const int header_height = 15;
        private const int animation_duration = 250;

        public readonly BindableBool IsOpen = new BindableBool();

        private readonly FillFlowContainer postsFlow;

        public MonthPanel(List<APINewsPost> posts)
        {
            Width = 160;
            Masking = true;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new DropdownButton(posts[0].PublishedAt)
                    {
                        IsOpen = { BindTarget = IsOpen }
                    },
                    postsFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = posts.Select(p => new PostButton(p)).ToArray()
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IsOpen.BindValueChanged(open =>
            {
                ClearTransforms();

                if (open.NewValue)
                {
                    AutoSizeAxes = Axes.Y;
                    postsFlow.FadeIn(animation_duration, Easing.OutQuint);
                }
                else
                {
                    AutoSizeAxes = Axes.None;
                    this.ResizeHeightTo(header_height, animation_duration, Easing.OutQuint);

                    postsFlow.FadeOut(animation_duration, Easing.OutQuint);
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

        private class DropdownButton : OsuClickableContainer
        {
            public readonly BindableBool IsOpen = new BindableBool();

            private readonly SpriteIcon icon;

            public DropdownButton(DateTimeOffset date)
            {
                Size = new Vector2(160, header_height);
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

            public PostButton(APINewsPost post)
            {
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
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
                Action = () => { }; // TODO
            }
        }
    }
}
