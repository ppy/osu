// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class FavouriteButton : HeaderButton, IHasTooltip
    {
        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        private readonly BindableBool favourited = new BindableBool();

        private PostBeatmapFavouriteRequest request;
        private LoadingLayer loading;

        private readonly IBindable<User> localUser = new Bindable<User>();

        public string TooltipText
        {
            get
            {
                if (!Enabled.Value) return string.Empty;

                return (favourited.Value ? "Unfavourite" : "Favourite") + " this beatmapset";
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(IAPIProvider api, NotificationOverlay notifications)
        {
            SpriteIcon icon;

            AddRange(new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Regular.Heart,
                    Size = new Vector2(18),
                    Shadow = false,
                },
                loading = new LoadingLayer(icon, false),
            });

            Action = () =>
            {
                // guaranteed by disabled state above.
                Debug.Assert(BeatmapSet.Value.OnlineBeatmapSetID != null);

                loading.Show();

                request?.Cancel();

                request = new PostBeatmapFavouriteRequest(BeatmapSet.Value.OnlineBeatmapSetID.Value, favourited.Value ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite);

                request.Success += () =>
                {
                    favourited.Toggle();
                    loading.Hide();
                };

                request.Failure += e =>
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = e.Message,
                        Icon = FontAwesome.Solid.Times,
                    });

                    loading.Hide();
                };

                api.Queue(request);
            };

            favourited.ValueChanged += favourited => icon.Icon = favourited.NewValue ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => updateEnabled());

            // must be run after setting the Action to ensure correct enabled state (setting an Action forces a button to be enabled).
            BeatmapSet.BindValueChanged(setInfo =>
            {
                updateEnabled();
                favourited.Value = setInfo.NewValue?.OnlineInfo?.HasFavourited ?? false;
            }, true);
        }

        private void updateEnabled() => Enabled.Value = !(localUser.Value is GuestUser) && BeatmapSet.Value?.OnlineBeatmapSetID > 0;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Width = DrawHeight;
        }
    }
}
