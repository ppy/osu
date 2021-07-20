// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapSet
{
    public enum MetadataType
    {
        [Description("标签")]
        Tags,
        
        [Description("来源")]
        Source,

        [Description("描述")]
        Description,

        [Description("流派")]
        Genre,

        [Description("语言")]
        Language
    }
}
