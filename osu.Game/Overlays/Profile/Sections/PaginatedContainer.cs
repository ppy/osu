// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        protected APIAccess Api;
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
                    TextSize = 15,
                    Text = header,
                    Font = "Exo2.0-RegularItalic",
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
                        TextSize = 14,
                        Text = "show more",
                        Padding = new MarginPadding {Vertical = 10, Horizontal = 15 },
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
                    TextSize = 14,
                    Text = missing,
                    Alpha = 0,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, RulesetStore rulesets)
        {
            Api = api;
            Rulesets = rulesets;

            User.ValueChanged += onUserChanged;
            User.TriggerChange();
        }

        private void onUserChanged(User newUser)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();
            ShowMoreButton.Hide();

            if (newUser != null)
                ShowMore();
        }

        protected virtual void ShowMore()
        {
            ShowMoreLoading.Show();
            ShowMoreButton.Hide();
        }
    }
}
