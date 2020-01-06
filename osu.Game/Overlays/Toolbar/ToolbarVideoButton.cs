// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarVideoButton : ToolbarButton
    {
        public ToolbarVideoButton()
        {
            Icon = FontAwesome.Solid.Video;
            TooltipMain = "点赞 投币 收藏 转发 qwq";
            TooltipSub = "此按钮将不会出现在发行版当中,不过仍可以从源代码启用";
        }
    }
}
