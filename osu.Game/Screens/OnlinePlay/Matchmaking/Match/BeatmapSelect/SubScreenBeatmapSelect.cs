// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class SubScreenBeatmapSelect : MatchmakingSubScreen
    {
        public override PanelDisplayStyle PlayersDisplayStyle => PanelDisplayStyle.Split;
        public override Drawable PlayersDisplayArea { get; }

        private readonly BeatmapSelectGrid beatmapSelectGrid;
        private readonly LoadingSpinner loadingSpinner;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public SubScreenBeatmapSelect()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 200 },
                    Children = new Drawable[]
                    {
                        beatmapSelectGrid = new BeatmapSelectGrid
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        loadingSpinner = new LoadingSpinner
                        {
                            Size = new Vector2(64),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            State = { Value = Visibility.Visible }
                        }
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

            beatmapSelectGrid.ItemSelected += item => client.MatchmakingToggleSelection(item.ID);
            client.MatchmakingItemSelected += onItemSelected;
            client.MatchmakingItemDeselected += onItemDeselected;
            client.SettingsChanged += onSettingsChanged;

            Debug.Assert(client.Room != null);

            loadItems(client.Room.Playlist.ToArray()).FireAndForget();
        }

        private async Task loadItems(MultiplayerPlaylistItem[] items)
        {
            var beatmaps = await beatmapLookupCache.GetBeatmapsAsync(items.Select(it => it.BeatmapID).ToArray()).ConfigureAwait(false);
            var matchmakingItems = new List<MatchmakingPlaylistItem>();

            foreach (var entry in items.Zip(beatmaps))
            {
                var (item, beatmap) = entry;

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

                matchmakingItems.Add(new MatchmakingPlaylistItem(item, beatmap, mods));
            }

            Scheduler.Add(() =>
            {
                loadingSpinner.Hide();
                beatmapSelectGrid.AddItems(matchmakingItems);
            });
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

        public void RollFinalBeatmap(long[] candidateItems, long finalItem) => beatmapSelectGrid.RollAndDisplayFinalBeatmap(candidateItems, finalItem);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchmakingItemSelected -= onItemSelected;
                client.MatchmakingItemDeselected -= onItemDeselected;
                client.SettingsChanged -= onSettingsChanged;
            }
        }
    }
}
