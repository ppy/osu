// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class FavouriteButton : GrayButton
    {
        public readonly BeatmapSetInfo BeatmapSetInfo;
        private APIBeatmapSet? beatmapSet;
        private readonly Bindable<BeatmapSetFavouriteState> current;

        private PostBeatmapFavouriteRequest? favouriteRequest;
        private LoadingLayer loading = null!;

        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public FavouriteButton(BeatmapSetInfo beatmapSetInfo)
            : base(FontAwesome.Regular.Heart)
        {
            BeatmapSetInfo = beatmapSetInfo;
            current = new BindableWithCurrent<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(false, 0));

            Size = new Vector2(75, 30);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(loading = new LoadingLayer(true, false));

            Action = toggleFavouriteStatus;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ => updateState(), true);

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => updateUser(), true);
        }

        private void getBeatmapSet()
        {
            GetBeatmapSetRequest beatmapSetRequest = new GetBeatmapSetRequest(BeatmapSetInfo.OnlineID);

            loading.Show();
            beatmapSetRequest.Success += beatmapSet =>
            {
                this.beatmapSet = beatmapSet;
                current.Value = new BeatmapSetFavouriteState(this.beatmapSet.HasFavourited, this.beatmapSet.FavouriteCount);

                loading.Hide();
                Enabled.Value = true;
            };
            beatmapSetRequest.Failure += e =>
            {
                Logger.Log($"Favourite button failed to fetch beatmap info: {e}", LoggingTarget.Network);

                Schedule(() =>
                {
                    loading.Hide();
                    Enabled.Value = false;
                    TooltipText = "this beatmap cannot be favourited";
                });
            };
            api.Queue(beatmapSetRequest);
        }

        private void toggleFavouriteStatus()
        {
            if (beatmapSet == null)
                return;

            Enabled.Value = false;
            loading.Show();

            var actionType = current.Value.Favourited ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite;

            favouriteRequest?.Cancel();
            favouriteRequest = new PostBeatmapFavouriteRequest(beatmapSet.OnlineID, actionType);

            favouriteRequest.Success += () =>
            {
                bool favourited = actionType == BeatmapFavouriteAction.Favourite;

                current.Value = new BeatmapSetFavouriteState(favourited, current.Value.FavouriteCount + (favourited ? 1 : -1));

                Enabled.Value = true;
                loading.Hide();
                api.LocalUserState.UpdateFavouriteBeatmapSets();
            };
            favouriteRequest.Failure += e =>
            {
                Logger.Error(e, $"Failed to {actionType.ToString().ToLowerInvariant()} beatmap: {e.Message}");

                Schedule(() =>
                {
                    Enabled.Value = true;
                    loading.Hide();
                });
            };

            api.Queue(favouriteRequest);
        }

        private void updateUser()
        {
            if (!(localUser.Value is GuestUser) && BeatmapSetInfo.OnlineID > 0)
                getBeatmapSet();
            else
            {
                Enabled.Value = false;
                current.Value = new BeatmapSetFavouriteState(false, 0);
                updateState();
                TooltipText = BeatmapsetsStrings.ShowDetailsFavouriteLogin;
            }
        }

        private void updateState()
        {
            if (current.Value.Favourited)
            {
                Background.Colour = colours.Green;
                Icon.Icon = FontAwesome.Solid.Heart;
                TooltipText = BeatmapsetsStrings.ShowDetailsUnfavourite;
            }
            else
            {
                Background.Colour = colours.Gray4;
                Icon.Icon = FontAwesome.Regular.Heart;
                TooltipText = BeatmapsetsStrings.ShowDetailsFavourite;
            }
        }
    }
}
