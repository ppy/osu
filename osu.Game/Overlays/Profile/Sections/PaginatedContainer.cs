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
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainer<TModel> : FillFlowContainer
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected readonly Bindable<User> User = new Bindable<User>();
        protected FillFlowContainer ItemsContainer;
        protected RulesetStore Rulesets;

        private APIRequest<List<TModel>> retrievalRequest;
        private CancellationTokenSource loadCancellation;

        private readonly string missingText;
        private ShowMoreButton moreButton;
        private OsuSpriteText missing;
        private PaginatedContainerHeader header;

        private readonly string headerText;
        private readonly CounterVisibilityState counterVisibilityState;

        protected PaginatedContainer(Bindable<User> user, string headerText = "", string missingText = "", CounterVisibilityState counterVisibilityState = CounterVisibilityState.AlwaysHidden)
        {
            this.headerText = headerText;
            this.missingText = missingText;
            this.counterVisibilityState = counterVisibilityState;
            User.BindTo(user);
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new Drawable[]
            {
                header = new PaginatedContainerHeader(headerText, counterVisibilityState)
                {
                    Alpha = string.IsNullOrEmpty(headerText) ? 0 : 1
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
                missing = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
                    Text = missingText,
                    Alpha = 0,
                },
            };

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
            {
                showMore();
                SetCount(GetCount(e.NewValue));
            }
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
            OnItemsReceived(items);

            if (!items.Any() && VisiblePages == 1)
            {
                moreButton.Hide();
                moreButton.IsLoading = false;

                if (!string.IsNullOrEmpty(missing.Text))
                    missing.Show();

                return;
            }

            LoadComponentsAsync(items.Select(CreateDrawableItem).Where(d => d != null), drawables =>
            {
                missing.Hide();
                moreButton.FadeTo(items.Count == ItemsPerPage ? 1 : 0);
                moreButton.IsLoading = false;

                ItemsContainer.AddRange(drawables);
            }, loadCancellation.Token);
        });

        protected virtual int GetCount(User user) => 0;

        protected void SetCount(int value) => header.Current.Value = value;

        protected virtual void OnItemsReceived(List<TModel> items)
        {
        }

        protected abstract APIRequest<List<TModel>> CreateRequest();

        protected abstract Drawable CreateDrawableItem(TModel model);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            retrievalRequest?.Cancel();
        }
    }
}
