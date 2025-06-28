// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API.Requests;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public partial class ScoreActionsContainer : OsuClickableContainer
    {
        private readonly SoloScoreInfo score;
        private readonly ScoreType scoreType;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private INotificationOverlay? notifications { get; set; }

        private SpriteIcon icon = null!;

        public Action<OsuMenu>? OnMenuRequested;
        public Action? OnPinStatusChanged;

        public ScoreActionsContainer(SoloScoreInfo score, ScoreType scoreType = ScoreType.Best)
        {
            this.score = score;
            this.scoreType = scoreType;

            Size = new Vector2(26);  // Increased from 24 to give more room

            Action = () =>
            {
                // Create menu fresh each time to ensure it has the latest state
                var menu = createMenu();
                OnMenuRequested?.Invoke(menu);
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = icon = new SpriteIcon
            {
                Icon = FontAwesome.Solid.EllipsisV,
                Size = new Vector2(14),  // Reduced from 16 to prevent clipping
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = colourProvider.Light1,
            };
        }

        private OsuMenu createMenu()
        {
            var menu = new OsuMenu(Direction.Vertical, true)
            {
                Items = createMenuItems()
            };

            return menu;
        }

        private MenuItem[] createMenuItems()
        {
            var items = new List<MenuItem>();

            if (score.OnlineID == -1)
            {
                items.Add(new OsuMenuItem("Cannot pin score (invalid ID)", MenuItemType.Standard)
                {
                    Action = { Disabled = true }
                });
            }
            else
            {
                // For scores in the Pinned section, they are definitely pinned
                // For scores in other sections, check the IsPinned property
                bool isPinned = scoreType == ScoreType.Pinned || score.IsPinned;

                Action togglePinAction = () =>
                {
                    if (isPinned)
                    {
                        var req = new UnpinScoreRequest(score.OnlineID);
                        req.Success += () =>
                        {
                            score.IsPinned = false;
                            Schedule(() => OnPinStatusChanged?.Invoke());
                        };
                        req.Failure += error =>
                        {
                            notifications?.Post(new SimpleNotification
                            {
                                Text = "Failed to unpin score. The feature may not be available yet.",
                                Icon = FontAwesome.Solid.ExclamationTriangle,
                            });
                            // Still close the menu on failure
                            Schedule(() => OnPinStatusChanged?.Invoke());
                        };
                        api.Queue(req);
                    }
                    else
                    {
                        Console.WriteLine($"[ScoreActionsContainer] Pinning score {score.OnlineID}");
                        var req = new PinScoreRequest(score.OnlineID);
                        req.Success += () =>
                        {
                            Console.WriteLine($"[ScoreActionsContainer] Successfully pinned score {score.OnlineID}");
                            score.IsPinned = true;
                            Schedule(() => OnPinStatusChanged?.Invoke());
                        };
                        req.Failure += error =>
                        {
                            Console.WriteLine($"[ScoreActionsContainer] Failed to pin score {score.OnlineID}: {error.Message}");
                            notifications?.Post(new SimpleNotification
                            {
                                Text = "Failed to pin score. The feature may not be available yet.",
                                Icon = FontAwesome.Solid.ExclamationTriangle,
                            });
                            // Still close the menu on failure
                            Schedule(() => OnPinStatusChanged?.Invoke());
                        };
                        api.Queue(req);
                    }
                };

                items.Add(new OsuMenuItem(isPinned ? "Unpin score" : "Pin score",
                                         isPinned ? MenuItemType.Destructive : MenuItemType.Highlighted,
                                         togglePinAction));
            }

            return items.ToArray();
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.FadeColour(colourProvider.Content1, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.FadeColour(colourProvider.Light1, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
