// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Users;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainer<T> : FillFlowContainer
    {
        private readonly ShowMoreButton moreButton;
        private readonly OsuSpriteText missingText;
        private APIRequest<List<T>> retrievalRequest;

        [Resolved]
        private IAPIProvider api { get; set; }

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected readonly Bindable<User> User = new Bindable<User>();
        protected readonly FillFlowContainer ItemsContainer;
        protected RulesetStore Rulesets;

        protected PaginatedContainer(Bindable<User> user, string header, string missing)
        {
            User.BindTo(user);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = header,
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                    Margin = new MarginPadding { Top = 10, Bottom = 10 },
                },
                ItemsContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(0, 2),
                },
                moreButton = new ShowMoreButton
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Alpha = 0,
                    Margin = new MarginPadding { Top = 10 },
                    Action = showMore,
                },
                missingText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
                    Text = missing,
                    Alpha = 0,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Rulesets = rulesets;

            User.ValueChanged += onUserChanged;
            User.TriggerChange();
        }

        private void onUserChanged(ValueChangedEvent<User> e)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();

            if (e.NewValue != null)
                showMore();
        }

        private void showMore()
        {
            retrievalRequest = CreateRequest();
            retrievalRequest.Success += items => UpdateItems(items);

            api.Queue(retrievalRequest);
        }

        protected virtual void UpdateItems(List<T> items)
        {
            Schedule(() =>
            {
                moreButton.FadeTo(items.Count == ItemsPerPage ? 1 : 0);
                moreButton.IsLoading = false;

                if (!items.Any() && VisiblePages == 1)
                {
                    moreButton.Hide();
                    moreButton.IsLoading = false;
                    missingText.Show();
                    return;
                }

                LoadComponentsAsync(items.Select(item => CreateDrawableItem(item)), i =>
                {
                    missingText.Hide();
                    moreButton.FadeTo(items.Count == ItemsPerPage ? 1 : 0);
                    moreButton.IsLoading = false;

                    ItemsContainer.AddRange(i);
                });
            });
        }

        protected abstract APIRequest<List<T>> CreateRequest();

        protected abstract Drawable CreateDrawableItem(T item);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            retrievalRequest?.Cancel();
        }
    }
}
