// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
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
    public partial class FavouriteButton : OsuAnimatedButton
    {
        private readonly Box background;
        private readonly SpriteIcon icon;
        private readonly BindableWithCurrent<BeatmapSetFavouriteState> current;

        public Bindable<BeatmapSetFavouriteState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly APIBeatmapSet beatmapSet;

        private PostBeatmapFavouriteRequest favouriteRequest;
        private LoadingLayer loading;

        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        public FavouriteButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;
            current = new BindableWithCurrent<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(this.beatmapSet.HasFavourited, this.beatmapSet.FavouriteCount));

            Size = new Vector2(50, 30);

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = FontAwesome.Regular.Heart,
                },
                loading = new LoadingLayer(true, false),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateState();

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => updateEnabled());

            Action = toggleFavouriteStatus;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ => updateState(), true);
        }

        private void toggleFavouriteStatus()
        {

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
            };
            favouriteRequest.Failure += e =>
            {
                Logger.Error(e, $"Failed to {actionType.ToString().ToLowerInvariant()} beatmap: {e.Message}");
                Enabled.Value = true;
                loading.Hide();
            };

            api.Queue(favouriteRequest);
        }

        private void updateEnabled() => Enabled.Value = !(localUser.Value is GuestUser) && beatmapSet.OnlineID > 0;

        private void updateState()
        {
            if (current?.Value == null)
                return;

            if (current.Value.Favourited)
            {
                background.Colour = colours.Green;
                icon.Icon = FontAwesome.Solid.Heart;
                TooltipText = BeatmapsetsStrings.ShowDetailsUnfavourite;
            }
            else
            {
                background.Colour = colours.Gray4;
                icon.Icon = FontAwesome.Regular.Heart;
                TooltipText = BeatmapsetsStrings.ShowDetailsFavourite;
            }
        }
    }
}
