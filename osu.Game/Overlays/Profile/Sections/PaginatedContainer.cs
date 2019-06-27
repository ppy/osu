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

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainer : FillFlowContainer
    {
        protected readonly FillFlowContainer ItemsContainer;
        protected readonly FillFlowContainer HeaderContainer;
        protected readonly ShowMoreButton MoreButton;
        protected readonly OsuSpriteText MissingText;

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected readonly Bindable<User> User = new Bindable<User>();

        protected IAPIProvider Api;
        protected APIRequest RetrievalRequest;
        protected RulesetStore Rulesets;

        protected PaginatedContainer(Bindable<User> user, string header, string missing)
        {
            User.BindTo(user);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new Drawable[]
            {
                HeaderContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Margin = new MarginPadding { Top = 10, Bottom = 10 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = header,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                        },
                    },
                },
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
                },
                MissingText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
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

            User.ValueChanged += OnUserChanged;
            User.TriggerChange();
        }

        protected virtual void OnUserChanged(ValueChangedEvent<User> e)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();

            if (e.NewValue != null)
                ShowMore();
        }

        protected abstract void ShowMore();
    }
}
