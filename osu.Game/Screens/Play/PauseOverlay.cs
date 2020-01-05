// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PauseOverlay : GameplayMenuOverlay
    {
        public Action OnResume;

        public override string Header => "游戏暂停";
        public override string Description => "要去做什么呢owo?";

        protected override Action BackAction => () => InternalButtons.Children.First().Click();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("继续", colours.Green, () => OnResume?.Invoke());
            AddButton("重试", colours.YellowDark, () => OnRetry?.Invoke());
            AddButton("退出", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());
        }
    }
}
