// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public partial class FavouriteButton : BeatmapCardIconButton, IHasCurrentValue<BeatmapSetFavouriteState>
    {
        private readonly BindableWithCurrent<BeatmapSetFavouriteState> current = new BindableWithCurrent<BeatmapSetFavouriteState>();

        public Bindable<BeatmapSetFavouriteState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly APIBeatmapSet beatmapSet;

        private PostBeatmapFavouriteRequest favouriteRequest;

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly IBindableList<APIBeatmapSet> beatmapFavourites = new BindableList<APIBeatmapSet>();

        public FavouriteButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Action = toggleFavouriteStatus;
            current.BindValueChanged(_ => updateState(), true);

            beatmapFavourites.BindTo(api.BeatmapFavourites);
            beatmapFavourites.BindCollectionChanged((_, _) => updateFavouriteStatus(), true);
        }

        private void toggleFavouriteStatus()
        {
            var actionType = current.Value.Favourited ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite;

            favouriteRequest?.Cancel();

            if (actionType == BeatmapFavouriteAction.Favourite)
                favouriteRequest = api.AddToFavourites(beatmapSet);
            else
                favouriteRequest = api.RemoveFromFavourites(beatmapSet);

            SetLoading(true);

            favouriteRequest.Success += () => SetLoading(false);
            favouriteRequest.Failure += _ => SetLoading(false);

            api.Queue(favouriteRequest);
        }

        private void updateFavouriteStatus() => current.Value = api.GetFavouriteState(beatmapSet);

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
