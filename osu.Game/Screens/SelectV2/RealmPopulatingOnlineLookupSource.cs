// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This component is designed to perform lookups of online data
    /// and store portions of it for later local use to the realm database.
    /// </summary>
    /// <example>
    /// This component is designed to locally persist potentially-volatile online information such as:
    /// <list type="bullet">
    /// <item>user tags assigned to difficulties of a beatmap,</item>
    /// <item>the beatmap's <see cref="BeatmapInfo.Status"/>,</item>
    /// <item>guest mappers assigned to difficulties of a beatmap,</item>
    /// <item>the local user's best score on a given beatmap.</item>
    /// </list>
    /// </example>
    public partial class RealmPopulatingOnlineLookupSource : Component
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public Task<APIBeatmapSet?> GetBeatmapSetAsync(int id, CancellationToken token = default)
        {
            var request = new GetBeatmapSetRequest(id);
            var tcs = new TaskCompletionSource<APIBeatmapSet?>();

            // async request success callback is a bit of a dangerous game, but there's some reasoning for it.
            // - don't really want to use `IAPIAccess.PerformAsync()` because we still want to respect request queueing & online status checks
            // - we want the realm write here to be async because it is known to be slow for some users with large beatmap collections
            // - at the time of writing `RealmAccess.WriteAsync()` can only be safely called from update thread,
            //   and API request completion callbacks are automatically marshaled onto update thread scheduler,
            //   so calling `WriteAsync()` within the callback is a somewhat "nice" way of guaranteeing that the call is safe
            //   (rather than having to enforce that `GetBeatmapSetAsync()` can only be called from update thread, or locally scheduling)
            request.Success += async onlineBeatmapSet =>
            {
                if (token.IsCancellationRequested)
                {
                    tcs.SetCanceled(token);
                    return;
                }

                var tagsById = (onlineBeatmapSet.RelatedTags ?? []).ToDictionary(t => t.Id);
                var onlineBeatmaps = onlineBeatmapSet.Beatmaps.ToDictionary(b => b.OnlineID);
                await realm.WriteAsync(r =>
                {
                    var beatmapSet = r.All<BeatmapSetInfo>().Where(b => b.OnlineID == id);

                    foreach (var dbBeatmapSet in beatmapSet)
                    {
                        dbBeatmapSet.Status = onlineBeatmapSet.Status;

                        foreach (var dbBeatmap in dbBeatmapSet.Beatmaps)
                        {
                            if (onlineBeatmaps.TryGetValue(dbBeatmap.OnlineID, out var onlineBeatmap))
                            {
                                // compare `BeatmapUpdaterMetadataLookup`
                                dbBeatmap.OnlineMD5Hash = onlineBeatmap.MD5Hash;
                                dbBeatmap.LastOnlineUpdate = onlineBeatmap.LastUpdated;

                                if (dbBeatmap.MatchesOnlineVersion)
                                    dbBeatmap.Status = onlineBeatmap.Status;

                                string[] userTagsArray = onlineBeatmap.TopTags?
                                                                      .Select(t => (topTag: t, relatedTag: tagsById.GetValueOrDefault(t.TagId)))
                                                                      .Where(t => t.relatedTag != null)
                                                                      // see https://github.com/ppy/osu-web/blob/bb3bd2e7c6f84f26066df5ea20a81c77ec9bb60a/resources/js/beatmapsets-show/controller.ts#L103-L106 for sort criteria
                                                                      .OrderByDescending(t => t.topTag.VoteCount)
                                                                      .ThenBy(t => t.relatedTag!.Name)
                                                                      .Select(t => t.relatedTag!.Name)
                                                                      .ToArray() ?? [];
                                dbBeatmap.Metadata.UserTags.Clear();
                                dbBeatmap.Metadata.UserTags.AddRange(userTagsArray);
                            }
                        }
                    }
                }).ConfigureAwait(true);
                tcs.SetResult(onlineBeatmapSet);
            };
            request.Failure += tcs.SetException;
            api.Queue(request);
            return tcs.Task;
        }
    }
}
