// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class FavouriteButton : BeatmapCardIconButton
    {
        private readonly APIBeatmapSet beatmapSet;

        private PostBeatmapFavouriteRequest favouriteRequest;

        public FavouriteButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            updateState();
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            Action = () =>
            {
                var actionType = beatmapSet.HasFavourited ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite;

                favouriteRequest?.Cancel();
                favouriteRequest = new PostBeatmapFavouriteRequest(beatmapSet.OnlineID, actionType);

                Enabled.Value = false;
                favouriteRequest.Success += () =>
                {
                    beatmapSet.HasFavourited = actionType == BeatmapFavouriteAction.Favourite;
                    Enabled.Value = true;
                    updateState();
                };
                favouriteRequest.Failure += e =>
                {
                    Logger.Error(e, $"Failed to {actionType.ToString().ToLower()} beatmap: {e.Message}");
                    Enabled.Value = true;
                };

                api.Queue(favouriteRequest);
            };
        }

        private void updateState()
        {
            if (beatmapSet.HasFavourited)
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
