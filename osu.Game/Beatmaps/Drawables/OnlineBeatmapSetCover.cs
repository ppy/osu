// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Game.Configuration;

namespace osu.Game.Beatmaps.Drawables
{
    [LongRunningLoad]
    public class OnlineBeatmapSetCover : Sprite
    {
        private readonly IBeatmapSetOnlineInfo set;
        private readonly BeatmapSetCoverType type;

        public OnlineBeatmapSetCover(IBeatmapSetOnlineInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            this.set = set;
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, MConfigManager mfconfig)
        {
            string resource = null;

            switch (mfconfig.Get<bool>(MSetting.UseSayobot))
            {
                case true:
                    string targetID = set.Covers.Card;
                    targetID = targetID.Replace("https://assets.ppy.sh/beatmaps/", "").Split('/')[0];

                    switch (type)
                    {
                        case BeatmapSetCoverType.Cover:
                            resource = $"https://a.sayobot.cn/beatmaps/{targetID}/covers/cover.jpg";
                            break;

                        case BeatmapSetCoverType.Card:
                            resource = $"https://a.sayobot.cn/beatmaps/{targetID}/covers/cover.jpg";
                            break;

                        case BeatmapSetCoverType.List:
                            resource = $"https://a.sayobot.cn/beatmaps/{targetID}/covers/cover.jpg";
                            break;
                    }

                    break;

                default:
                    switch (type)
                    {
                        case BeatmapSetCoverType.Cover:
                            resource = set.Covers.Cover;
                            break;

                        case BeatmapSetCoverType.Card:
                            resource = set.Covers.Card;
                            break;

                        case BeatmapSetCoverType.List:
                            resource = set.Covers.List;
                            break;
                    }

                    break;
            }

            if (resource != null)
                Texture = textures.Get(resource);
        }
    }

    public enum BeatmapSetCoverType
    {
        Cover,
        Card,
        List
    }
}
