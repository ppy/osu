// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using osu.Game.Online.API;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Graphics.UserInterface;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;
using osu.Game.Configuration;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;
using osu.Framework.Utils;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public partial class PaginatedScoreContainer : PaginatedProfileSubsection<SoloScoreInfo>
    {
        private readonly ScoreType type;

        public Action? OnScorePinChanged { get; set; }

        private readonly HashSet<long> pinnedScoreIds;

        public PaginatedScoreContainer(ScoreType type, Bindable<UserProfileData?> user, LocalisableString headerText, HashSet<long>? pinnedScoreIds = null)
            : base(user, headerText)
        {
            this.type = type;
            this.pinnedScoreIds = pinnedScoreIds ?? new HashSet<long>();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override int GetCount(APIUser user)
        {
            switch (type)
            {
                case ScoreType.Best:
                    return user.ScoresBestCount;

                case ScoreType.Firsts:
                    return user.ScoresFirstCount;

                case ScoreType.Recent:
                    return user.ScoresRecentCount;

                case ScoreType.Pinned:
                    return user.ScoresPinnedCount;

                default:
                    return 0;
            }
        }

        protected override void OnItemsReceived(List<SoloScoreInfo> items)
        {
            if (CurrentPage == null || CurrentPage?.Offset == 0)
                drawableItemIndex = 0;

            if (type == ScoreType.Pinned)
            {
                foreach (var item in items)
                {
                    item.IsPinned = true;
                    pinnedScoreIds.Add(item.OnlineID);
                }
            }
            else
            {
                foreach (var item in items)
                {
                    if (pinnedScoreIds.Contains(item.OnlineID))
                    {
                        item.IsPinned = true;
                    }
                }
            }

            base.OnItemsReceived(items);
        }

        protected override APIRequest<List<SoloScoreInfo>> CreateRequest(UserProfileData user, PaginationParameters pagination)
        {
            return new GetUserScoresRequest(user.User.Id, type, pagination, user.Ruleset);
        }

        private int drawableItemIndex;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        protected override Drawable? CreateDrawableItem(SoloScoreInfo model)
        {
            switch (type)
            {
                default:
                    return new DrawableProfileScoreRow(new DrawableProfileScore(model), model, OnScorePinChanged);

                case ScoreType.Best:
                    double weight = Math.Pow(0.95, drawableItemIndex++);
                    return new DrawableProfileScoreRow(new DrawableProfileWeightedScore(model, weight), model, OnScorePinChanged);
            }
        }
    }

    public partial class DrawableProfileScoreRow : CompositeDrawable
    {
        private readonly Container menuContainer;
        private OsuMenu? currentMenu;
        private ScoreActionsContainer? currentActionsContainer;

        public DrawableProfileScoreRow(Drawable scoreDrawable, SoloScoreInfo score, Action? onScorePinChanged = null)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            var actionsContainer = new ScoreActionsContainer(score)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            actionsContainer.OnMenuRequested = menu =>
            {
                showMenu(menu, actionsContainer);
            };

            actionsContainer.OnPinStatusChanged = () =>
            {
                hideMenu();
                onScorePinChanged?.Invoke();
            };

            var mainContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    scoreDrawable,
                    new Container
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        X = 5,
                        Padding = new MarginPadding { Left = 2 },
                        Child = actionsContainer
                    }
                }
            };

            InternalChildren = new Drawable[]
            {
                mainContainer,
                menuContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Depth = -1,
                    RelativePositionAxes = Axes.Y,
                    Y = 1f,
                    Padding = new MarginPadding { Right = -20 }
                }
            };
        }

        private void showMenu(OsuMenu menu, ScoreActionsContainer button)
        {
            hideMenu();

            currentMenu = menu;
            currentActionsContainer = button;

            menuContainer.Add(menu);

            menu.Position = new Vector2(-100, 0);
            menu.Anchor = Anchor.TopRight;
            menu.Origin = Anchor.TopRight;
            menu.Width = 100f;

            menu.Open();

            menu.StateChanged += state =>
            {
                if (state == MenuState.Closed)
                {
                    Scheduler.AddOnce(hideMenu);
                }
            };
        }

        private void hideMenu()
        {
            currentMenu?.Expire();
            currentMenu = null;
            currentActionsContainer = null;
            menuContainer.Clear();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (currentMenu?.State == MenuState.Open)
            {
                var menuBounds = currentMenu.ScreenSpaceDrawQuad;
                var buttonBounds = currentActionsContainer?.ScreenSpaceDrawQuad ?? new Quad();

                bool clickInMenu = menuBounds.Contains(e.ScreenSpaceMousePosition);
                bool clickInButton = buttonBounds.Contains(e.ScreenSpaceMousePosition);

                if (!clickInMenu && !clickInButton)
                {
                    hideMenu();
                    return true;
                }
            }

            return base.OnMouseDown(e);
        }

        protected override void Update()
        {
            base.Update();

            if (currentMenu?.State == MenuState.Open)
            {
                var inputManager = GetContainingInputManager();
                if (inputManager != null && inputManager.CurrentState.Mouse.IsPressed(MouseButton.Left))
                {
                    var menuBounds = currentMenu.ScreenSpaceDrawQuad;
                    var buttonBounds = currentActionsContainer?.ScreenSpaceDrawQuad ?? new Quad();
                    var mousePos = inputManager.CurrentState.Mouse.Position;

                    bool mouseInMenu = menuBounds.Contains(mousePos);
                    bool mouseInButton = buttonBounds.Contains(mousePos);

                    if (!mouseInMenu && !mouseInButton && !wasMousePressed)
                    {
                        hideMenu();
                    }
                }

                wasMousePressed = inputManager?.CurrentState.Mouse.IsPressed(MouseButton.Left) ?? false;
            }
        }

        private bool wasMousePressed;
    }
}
