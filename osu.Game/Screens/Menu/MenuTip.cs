// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public partial class MenuTip : CompositeDrawable
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private LinkFlowContainer textFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                textFlow = new LinkFlowContainer
                {
                    Width = 700,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Spacing = new Vector2(0, 2),
                },
            };
        }

        public void ShowNextTip()
        {
            if (!config.Get<bool>(OsuSetting.MenuTips)) return;

            static void formatRegular(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular);
            static void formatSemiBold(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold);

            string tip = getRandomTip();

            AutoSizeAxes = Axes.Both;

            textFlow.Clear();
            textFlow.AddParagraph("a tip for you:", formatSemiBold);
            textFlow.AddParagraph(tip, formatRegular);

            this.FadeInFromZero(200, Easing.OutQuint)
                .Delay(1000 + 80 * tip.Length)
                .Then()
                .FadeOutFromOne(2000, Easing.OutQuint)
                .Finally(_ => AutoSizeAxes = Axes.X);
        }

        private string getRandomTip()
        {
            string[] tips =
            {
                "You can press Ctrl-T anywhere in the game to toggle the toolbar!",
                "You can press Ctrl-O anywhere in the game to access options!",
                "All settings are dynamic and take effect in real-time. Try changing the skin while watching autoplay!",
                "New features are coming online every update. Make sure to stay up-to-date!",
                "If you find the UI too large or small, try adjusting UI scale in settings!",
                "Try adjusting the \"Screen Scaling\" mode to change your gameplay or UI area, even in fullscreen!",
                "What used to be \"osu!direct\" is available to all users just like on the website. You can access it anywhere using Ctrl-B!",
                "Seeking in replays is available by dragging on the difficulty bar at the bottom of the screen!",
                "Multithreading support means that even with low \"FPS\" your input and judgements will be accurate!",
                "Try scrolling down in the mod select panel to find a bunch of new fun mods!",
                "Most of the web content (profiles, rankings, etc.) are available natively in-game from the icons on the toolbar!",
                "Get more details, hide or delete a beatmap by right-clicking on its panel at song select!",
                "All delete operations are temporary until exiting. Restore accidentally deleted content from the maintenance settings!",
                "Check out the \"playlists\" system, which lets users create their own custom and permanent leaderboards!",
                "Toggle advanced frame / thread statistics with Ctrl-F11!",
                "Take a look under the hood at performance counters and enable verbose performance logging with Ctrl-F2!",
            };

            return tips[RNG.Next(0, tips.Length)];
        }
    }
}
