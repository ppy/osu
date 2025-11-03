// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public partial class FavouriteButton : BeatmapCardIconButton, IHasCurrentValue<BeatmapSetFavouriteState>
    {
        private readonly BindableWithCurrent<BeatmapSetFavouriteState> current;

        public Bindable<BeatmapSetFavouriteState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly APIBeatmapSet beatmapSet;

        private PostBeatmapFavouriteRequest favouriteRequest;

        [Resolved]
        private IAPIProvider api { get; set; }

        public FavouriteButton(APIBeatmapSet beatmapSet)
        {
            current = new BindableWithCurrent<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(beatmapSet.HasFavourited, beatmapSet.FavouriteCount));
            this.beatmapSet = beatmapSet;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Action = toggleFavouriteStatus;
            current.BindValueChanged(_ => updateState(), true);
        }

        private void toggleFavouriteStatus()
        {
            var actionType = current.Value.Favourited ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite;

            favouriteRequest?.Cancel();
            favouriteRequest = new PostBeatmapFavouriteRequest(beatmapSet.OnlineID, actionType);

            SetLoading(true);

            favouriteRequest.Success += () =>
            {
                bool favourited = actionType == BeatmapFavouriteAction.Favourite;

                current.Value = new BeatmapSetFavouriteState(favourited, current.Value.FavouriteCount + (favourited ? 1 : -1));

                SetLoading(false);
                api.LocalUserState.UpdateFavouriteBeatmapSets();
            };
            favouriteRequest.Failure += e =>
            {
                Logger.Error(e, $"Failed to {actionType.ToString().ToLowerInvariant()} beatmap: {e.Message}");
                SetLoading(false);
            };

            api.Queue(favouriteRequest);
        }

        private void updateState()
        {
            if (current.Value.Favourited)
            {
                Icon.Icon = FontAwesome.Solid.Heart;
                TooltipText = BeatmapsetsStrings.ShowDetailsUnfavourite;
            }
            else
            {
                Icon.Icon = FontAwesome.Regular.Heart;
                TooltipText = BeatmapsetsStrings.ShowDetailsFavourite;
            }
        }
    }
}
