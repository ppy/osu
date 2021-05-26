// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.News.Displays
{
    /// <summary>
    /// Lists articles in a vertical flow for a specified year.
    /// </summary>
    public class ArticleListing : CompositeDrawable
    {
        public Action RequestMorePosts;

        private readonly BindableList<APINewsPost> posts = new BindableList<APINewsPost>();
        private bool showMoreButtonIsVisible;

        private FillFlowContainer content;
        private ShowMoreButton showMore;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Vertical = 20,
                Left = 30,
                Right = 50
            };
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    content = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10)
                    },
                    showMore = new ShowMoreButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding
                        {
                            Top = 15
                        },
                        Action = RequestMorePosts,
                        Alpha = 0
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            posts.BindCollectionChanged((sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        addPosts(args.NewItems.Cast<APINewsPost>());
                        break;

                    default:
                        throw new NotSupportedException(@"You can only add items to this list. Other actions are not supported.");
                }
            }, true);
        }

        public void AddPosts(IEnumerable<APINewsPost> posts, bool showMoreButtonIsVisible)
        {
            this.showMoreButtonIsVisible = showMoreButtonIsVisible;
            this.posts.AddRange(posts);
        }

        private CancellationTokenSource cancellationToken;

        private void addPosts(IEnumerable<APINewsPost> posts)
        {
            LoadComponentsAsync(posts.Select(p => new NewsCard(p)).ToList(), loaded =>
            {
                content.AddRange(loaded);
                showMore.IsLoading = false;
                showMore.Alpha = showMoreButtonIsVisible ? 1 : 0;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
