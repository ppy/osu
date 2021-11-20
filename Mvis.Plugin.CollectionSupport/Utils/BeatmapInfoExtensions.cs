// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;

namespace Mvis.Plugin.CollectionSupport.Utils
{
    public static class BeatmapInfoExtensions
    {
        //只能先这样做了，BeatmapManager的GetWorkingBeatmap要求参数是BeatmapInfo，但是
        //BeatmapCollection.Beatmaps从BeatmapInfo变成了IBeatmapInfo
        public static BeatmapInfo AsBeatmapInfo(this IBeatmapInfo iInfo)
        {
            if (iInfo is BeatmapInfo beatmapInfo) return beatmapInfo;

            throw new InvalidCastException($"{iInfo} 不是 BeatmapInfo");
        }
    }
}
