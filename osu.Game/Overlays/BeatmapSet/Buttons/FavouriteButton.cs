// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class FavouriteButton : HeaderButton, IHasTooltip
    {
        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        private readonly Bindable<bool> favourited = new Bindable<bool>();

        private PostBeatmapFavouriteRequest request;
        private DimmedLoadingLayer loading;

        public string TooltipText => (favourited.Value ? "Unfavourite" : "Favourite") + " this beatmapset";

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
                loading = new DimmedLoadingLayer(),
            });

            BeatmapSet.BindValueChanged(setInfo =>
            {
                if (setInfo.NewValue?.OnlineInfo?.HasFavourited == null)
                    return;

                favourited.Value = setInfo.NewValue.OnlineInfo.HasFavourited;
            });

            favourited.ValueChanged += favourited =>
            {
                loading.Hide();

                icon.Icon = favourited.NewValue ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;
            };

            Action = () =>
            {
                if (loading.State.Value == Visibility.Visible)
                    return;

                loading.Show();

                request?.Cancel();
                request = new PostBeatmapFavouriteRequest(BeatmapSet.Value?.OnlineBeatmapSetID ?? 0, favourited.Value ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite);
                request.Success += () => favourited.Value = !favourited.Value;
                request.Failure += exception =>
                {
                    if (exception.Message == "UnprocessableEntity")
                    {
                        notifications.Post(new SimpleNotification
                        {
                            Text = @"You have too many favourited beatmaps! Please unfavourite some before trying again.",
                            Icon = FontAwesome.Solid.Times,
                        });
                        loading.Hide();
                    }
                };
                api.Queue(request);
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Width = DrawHeight;
        }
    }
}
