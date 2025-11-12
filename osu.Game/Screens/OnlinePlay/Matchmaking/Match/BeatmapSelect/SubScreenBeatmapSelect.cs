// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class SubScreenBeatmapSelect : MatchmakingSubScreen
    {
        public override PanelDisplayStyle PlayersDisplayStyle => PanelDisplayStyle.Split;
        public override Drawable PlayersDisplayArea { get; }

        private readonly BeatmapSelectGrid beatmapSelectGrid;
        private readonly List<MultiplayerPlaylistItem> items = new List<MultiplayerPlaylistItem>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        public SubScreenBeatmapSelect()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 200 },
                    Child = beatmapSelectGrid = new BeatmapSelectGrid
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = PlayersDisplayArea = new Container().With(d =>
                    {
                        d.RelativeSizeAxes = Axes.Both;
                    })
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.ItemAdded += onItemAdded;

            foreach (var item in client.Room!.Playlist)
                onItemAdded(item);

            beatmapSelectGrid.ItemSelected += item => client.MatchmakingToggleSelection(item.ID);

            client.MatchmakingItemSelected += onItemSelected;
            client.MatchmakingItemDeselected += onItemDeselected;
            client.SettingsChanged += onSettingsChanged;
        }

        private void onItemAdded(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            if (item.Expired)
                return;

            items.Add(item);
            Scheduler.AddOnce(addItems);
        });

        private void addItems() => Task.Run(addItemsAsync);

        private async Task addItemsAsync()
        {
            var itemsImmutable = items.ToImmutableArray();
            items.Clear();

            var beatmaps = await beatmapLookupCache.GetBeatmapsAsync(itemsImmutable.Select(item => item.BeatmapID).ToArray()).ConfigureAwait(false);

            var matchmakingItems = new List<IMatchmakingPlaylistItem>();

            foreach (var record in itemsImmutable.Zip(beatmaps))
            {
                var (item, beatmap) = record;

                beatmap ??= new APIBeatmap
                {
                    BeatmapSet = new APIBeatmapSet
                    {
                        Title = "unknown beatmap",
                        TitleUnicode = "unknown beatmap",
                        Artist = "unknown artist",
                        ArtistUnicode = "unknown artist",
                    }
                };

                beatmap.StarRating = item.StarRating;

                Ruleset? ruleset = rulesetStore.GetRuleset(item.RulesetID)?.CreateInstance();

                Debug.Assert(ruleset != null);

                Mod[] mods = item.RequiredMods.Select(m => m.ToMod(ruleset)).ToArray();

                matchmakingItems.Add(new MatchmakingPlaylistItemBeatmap(item, beatmap, mods));
            }

            matchmakingItems.Add(new MatchmakingPlaylistItemRandom());

            Schedule(() => beatmapSelectGrid.AddItems(matchmakingItems));
        }

        private void onItemSelected(int userId, long itemId)
        {
            var user = client.Room!.Users.First(it => it.UserID == userId).User!;
            beatmapSelectGrid.SetUserSelection(user, itemId, true);
        }

        private void onItemDeselected(int userId, long itemId)
        {
            var user = client.Room!.Users.First(it => it.UserID == userId).User!;
            beatmapSelectGrid.SetUserSelection(user, itemId, false);
        }

        private void onSettingsChanged(MultiplayerRoomSettings settings)
        {
            if (client.Room!.MatchState is not MatchmakingRoomState matchmakingState)
                return;

            if (matchmakingState.Stage != MatchmakingStage.ServerBeatmapFinalised)
                return;

            if (matchmakingState.CandidateItem != -1)
                return;

            beatmapSelectGrid.RevealRandomItem(client.Room!.CurrentPlaylistItem);
        }

        public void RollFinalBeatmap(long[] candidateItems, long finalItem) => beatmapSelectGrid.RollAndDisplayFinalBeatmap(candidateItems, finalItem, finalItem);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.ItemAdded -= onItemAdded;
                client.MatchmakingItemSelected -= onItemSelected;
                client.MatchmakingItemDeselected -= onItemDeselected;
                client.SettingsChanged -= onSettingsChanged;
            }
        }
    }
}
