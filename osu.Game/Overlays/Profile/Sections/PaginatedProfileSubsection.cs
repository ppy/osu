﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedProfileSubsection<TModel> : ProfileSubsection
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected FillFlowContainer ItemsContainer { get; private set; }

        private APIRequest<List<TModel>> retrievalRequest;
        private CancellationTokenSource loadCancellation;

        private ShowMoreButton moreButton;
        private OsuSpriteText missing;
        private readonly string missingText;

        protected PaginatedProfileSubsection(Bindable<User> user, string headerText = "", string missingText = "", CounterVisibilityState counterVisibilityState = CounterVisibilityState.AlwaysHidden)
            : base(user, headerText, counterVisibilityState)
        {
            this.missingText = missingText;
        }

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
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
                }
            }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(onUserChanged, true);
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

                if (!string.IsNullOrEmpty(missingText))
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

        protected virtual void OnItemsReceived(List<TModel> items)
        {
        }

        protected abstract APIRequest<List<TModel>> CreateRequest();

        protected abstract Drawable CreateDrawableItem(TModel model);

        protected override void Dispose(bool isDisposing)
        {
            retrievalRequest?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
