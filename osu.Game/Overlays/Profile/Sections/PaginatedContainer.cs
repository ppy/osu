// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainer : ProfileSubSection
    {
        protected readonly FillFlowContainer ItemsContainer;
        protected readonly ShowMoreButton MoreButton;

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected IAPIProvider Api;
        protected APIRequest RetrievalRequest;
        protected RulesetStore Rulesets;

        protected PaginatedContainer(Bindable<User> user)
            : base(user)
        {
            Add(new FillFlowContainer
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
                    MoreButton = new ShowMoreButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Alpha = 0,
                        Margin = new MarginPadding { Top = 10 },
                        Action = ShowMore,
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, RulesetStore rulesets)
        {
            Api = api;
            Rulesets = rulesets;
        }

        protected override void OnUserChanged(ValueChangedEvent<User> user)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();

            if (user.NewValue != null)
                ShowMore();
        }

        protected abstract void ShowMore();
    }
}
