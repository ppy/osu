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
using Realms;

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

            request.Success += onlineBeatmapSet =>
            {
                if (token.IsCancellationRequested)
                {
                    tcs.SetCanceled(token);
                    return;
                }

                var tagsById = (onlineBeatmapSet.RelatedTags ?? []).ToDictionary(t => t.Id);
                var onlineBeatmaps = onlineBeatmapSet.Beatmaps.ToDictionary(b => b.OnlineID);
                realm.Write(r =>
                {
                    foreach (var dbBeatmap in r.All<BeatmapInfo>().Filter($@"{nameof(BeatmapInfo.BeatmapSet)}.{nameof(BeatmapSetInfo.OnlineID)} == $0", id))
                    {
                        if (onlineBeatmaps.TryGetValue(dbBeatmap.OnlineID, out var onlineBeatmap))
                        {
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
                });
                tcs.SetResult(onlineBeatmapSet);
            };
            request.Failure += tcs.SetException;
            api.Queue(request);
            return tcs.Task;
        }
    }
}
