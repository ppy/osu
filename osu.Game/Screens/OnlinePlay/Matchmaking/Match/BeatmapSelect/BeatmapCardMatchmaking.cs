// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class BeatmapCardMatchmaking : OsuClickableContainer
    {
        public const float WIDTH = 345;
        public const float HEIGHT = 80;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        private readonly List<APIUser> users = new List<APIUser>();

        private Container contentContainer = null!;
        private Drawable flashLayer = null!;
        private BeatmapCardMatchmakingContent? content;

        public BeatmapCardMatchmaking()
        {
            Width = WIDTH;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                }
            };
        }

        public void AddUser(APIUser user)
        {
            users.Add(user);
            content?.SelectionOverlay.AddUser(user);
        }

        public void RemoveUser(APIUser user)
        {
            users.Remove(user);
            content?.SelectionOverlay.RemoveUser(user.Id);
        }

        public void DisplayItem(MultiplayerPlaylistItem item)
        {
            Ruleset? ruleset = rulesetStore.GetRuleset(item.RulesetID)?.CreateInstance();

            if (ruleset == null)
                return;

            Mod[] mods = item.RequiredMods.Select(m => m.ToMod(ruleset)).ToArray();

            Task.Run(loadBeatmap);

            async Task loadBeatmap()
            {
                APIBeatmap? beatmap = await beatmapLookupCache.GetBeatmapAsync(item.BeatmapID).ConfigureAwait(false);

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

                loadContent(new BeatmapCardMatchmakingBeatmapContent(beatmap, mods));
            }
        }

        public void DisplayRandom() => loadContent(new BeatmapCardMatchmakingRandomContent());

        private void loadContent(BeatmapCardMatchmakingContent newContent) => Schedule(() =>
        {
            bool flashNewContent = content != null;

            contentContainer.Child = content = newContent;

            foreach (var user in users)
                newContent.SelectionOverlay.AddUser(user);

            if (flashNewContent)
                flashLayer.FadeOutFromOne(1000, Easing.In);
        });
    }
}
