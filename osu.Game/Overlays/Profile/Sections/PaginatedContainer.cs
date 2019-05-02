// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections
{
    public class PaginatedContainer : FillFlowContainer
    {
        protected readonly FillFlowContainer ItemsContainer;
        protected readonly OsuHoverContainer ShowMoreButton;
        protected readonly LoadingAnimation ShowMoreLoading;
        protected readonly OsuSpriteText MissingText;

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected readonly Bindable<User> User = new Bindable<User>();

        protected IAPIProvider Api;
        protected APIRequest RetrievalRequest;
        protected RulesetStore Rulesets;

        public PaginatedContainer(Bindable<User> user, string header, string missing)
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
                    Font = OsuFont.GetFont(size: 15, weight: FontWeight.Regular, italics: true),
                    Margin = new MarginPadding { Top = 10, Bottom = 10 },
                },
                ItemsContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding { Bottom = 10 }
                },
                ShowMoreButton = new OsuHoverContainer
                {
                    Alpha = 0,
                    Action = ShowMore,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14),
                        Text = "show more",
                        Padding = new MarginPadding { Vertical = 10, Horizontal = 15 },
                    }
                },
                ShowMoreLoading = new LoadingAnimation
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(14),
                },
                MissingText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 14),
                    Text = missing,
                    Alpha = 0,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, RulesetStore rulesets)
        {
            Api = api;
            Rulesets = rulesets;

            User.ValueChanged += onUserChanged;
            User.TriggerChange();
        }

        private void onUserChanged(ValueChangedEvent<User> e)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();
            ShowMoreButton.Hide();

            if (e.NewValue != null)
                ShowMore();
        }

        protected virtual void ShowMore()
        {
            ShowMoreLoading.Show();
            ShowMoreButton.Hide();
        }
    }
}
