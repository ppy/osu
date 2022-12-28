// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Configuration.AccelUtils;

namespace osu.Game.Beatmaps.Drawables
{
    [LongRunningLoad]
    public partial class OnlineBeatmapSetCover : Sprite
    {
        private readonly IBeatmapSetOnlineInfo set;
        private readonly BeatmapSetCoverType type;

        public OnlineBeatmapSetCover(IBeatmapSetOnlineInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            ArgumentNullException.ThrowIfNull(set);

            this.set = set;
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, MConfigManager mConfig)
        {
            string resource = null;

            if (mConfig.Get<bool>(MSetting.UseAccelForDefault))
            {
                string targetID = set.Covers.Card;
                targetID = targetID.Replace("https://assets.ppy.sh/beatmaps/", "").Split('/')[0];

                var dict = new Dictionary<string, object>
                {
                    ["BID"] = targetID
                };

                string result;
                bool success = mConfig.Get<string>(MSetting.CoverAccelSource).TryParseAccelUrl(dict, out result, out _, true);

                if (success)
                    resource = result;
                else
                    Logger.Log("解析封面加速地址失败, 请检查相关设置", level: LogLevel.Important);
            }

            if (resource == null)
            {
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
