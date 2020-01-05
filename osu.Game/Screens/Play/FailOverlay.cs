// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : GameplayMenuOverlay
    {
        public override string Header => "游戏失败";
        public override string Description => "保持你的决心!";

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("重试", colours.YellowDark, () => OnRetry?.Invoke());
            AddButton("退出", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());
        }
    }
}
