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

        private readonly Channel target;

        /// <summary>
        /// Creates a new <see cref="NowPlayingCommand"/> to post the currently-playing beatmap to a parenting <see cref="IChannelPostTarget"/>.
        /// </summary>
        /// <param name="target">The target channel to post to. If <c>null</c>, the currently-selected channel will be posted to.</param>
        public NowPlayingCommand(Channel target = null)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            string verb;
            IBeatmapInfo beatmapInfo;

            switch (api.Activity.Value)
            {
                case UserActivity.InGame game:
                    verb = "playing";
                    beatmapInfo = game.BeatmapInfo;
                    break;

                case UserActivity.Editing edit:
                    verb = "editing";
                    beatmapInfo = edit.BeatmapInfo;
                    break;

                default:
                    verb = "listening to";
                    beatmapInfo = currentBeatmap.Value.BeatmapInfo;
                    break;
            }

            string beatmapString = beatmapInfo.OnlineID > 0 ? $"[{api.WebsiteRootUrl}/b/{beatmapInfo.OnlineID} {beatmapInfo}]" : beatmapInfo.ToString();

            channelManager.PostMessage($"is {verb} {beatmapString}", true, target);
            Expire();
        }
    }
}
