// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class NowPlayingCommand : Component
    {
        [Resolved]
        private IChannelPostTarget channelManager { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            string verb;
            BeatmapInfo beatmap;

            switch (api.Activity.Value)
            {
                case UserActivity.SoloGame solo:
                    verb = "playing";
                    beatmap = solo.Beatmap;
                    break;

                case UserActivity.Editing edit:
                    verb = "editing";
                    beatmap = edit.Beatmap;
                    break;

                default:
                    verb = "listening to";
                    beatmap = currentBeatmap.Value.BeatmapInfo;
                    break;
            }

            var beatmapString = beatmap.OnlineBeatmapID.HasValue ? $"[{api.WebsiteRootUrl}/b/{beatmap.OnlineBeatmapID} {beatmap}]" : beatmap.ToString();

            channelManager.PostMessage($"is {verb} {beatmapString}", true);
            Expire();
        }
    }
}
