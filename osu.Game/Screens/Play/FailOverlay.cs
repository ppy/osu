// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : GameplayMenuOverlay
    {
        public override string Header => "failed";
        public override string Description => "you're dead, try again?";

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("Retry", colours.YellowDark, () => OnRetry?.Invoke());
            AddButton("Quit", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());
        }
    }
}
