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
using System.Threading;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainer<TModel> : FillFlowContainer
    {
        private readonly ProfileShowMoreButton moreButton;
        private readonly OsuSpriteText missingText;
        private APIRequest<List<TModel>> retrievalRequest;
        private CancellationTokenSource loadCancellation;

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
                moreButton = new ProfileShowMoreButton
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
            loadCancellation?.Cancel();
            retrievalRequest?.Cancel();

            VisiblePages = 0;
            ItemsContainer.Clear();

            if (e.NewValue != null)
                showMore();
        }

        private void showMore()
        {
            loadCancellation = new CancellationTokenSource();

            retrievalRequest = CreateRequest();
            retrievalRequest.Success += UpdateItems;

            api.Queue(retrievalRequest);
        }

        protected virtual void UpdateItems(List<TModel> items) => Schedule(() =>
        {
            if (!items.Any() && VisiblePages == 1)
            {
                moreButton.Hide();
                moreButton.IsLoading = false;
                missingText.Show();
                return;
            }

            LoadComponentsAsync(items.Select(CreateDrawableItem).Where(d => d != null), drawables =>
            {
                missingText.Hide();
                moreButton.FadeTo(items.Count == ItemsPerPage ? 1 : 0);
                moreButton.IsLoading = false;

                ItemsContainer.AddRange(drawables);
            }, loadCancellation.Token);
        });

        protected abstract APIRequest<List<TModel>> CreateRequest();

        protected abstract Drawable CreateDrawableItem(TModel model);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            retrievalRequest?.Cancel();
        }
    }
}
