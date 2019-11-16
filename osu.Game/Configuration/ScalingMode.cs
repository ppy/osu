// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ScalingMode
    {
        [Description(@"关")]
        Off,

        [Description(@"所有元素")]
        Everything,

        [Description("除Overlay(工具栏之类的)以外的所有元素")]
        ExcludeOverlays,

        [Description(@"仅游戏内元素")]
        Gameplay,
    }
}
