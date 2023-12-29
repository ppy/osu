// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class MenuTip : CompositeDrawable
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private LinkFlowContainer textFlow = null!;

        private Bindable<bool> showMenuTips = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerExponent = 2.5f,
                    CornerRadius = 15,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.4f,
                        },
                    }
                },
                textFlow = new LinkFlowContainer
                {
                    Width = 600,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.TopCentre,
                    Spacing = new Vector2(0, 2),
                    Margin = new MarginPadding(10)
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showMenuTips = config.GetBindable<bool>(OsuSetting.MenuTips);
            showMenuTips.BindValueChanged(_ => ShowNextTip(), true);
        }

        public void ShowNextTip()
        {
            if (!showMenuTips.Value)
            {
                this.FadeOut(100, Easing.OutQuint);
                return;
            }

            static void formatRegular(SpriteText t) => t.Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular);
            static void formatSemiBold(SpriteText t) => t.Font = OsuFont.GetFont(size: 16, weight: FontWeight.SemiBold);

            string tip = getRandomTip();

            textFlow.Clear();
            textFlow.AddParagraph("a tip for you:", formatSemiBold);
            textFlow.AddParagraph(tip, formatRegular);

            this.FadeInFromZero(200, Easing.OutQuint)
                .Delay(1000 + 80 * tip.Length)
                .Then()
                .FadeOutFromOne(2000, Easing.OutQuint);
        }

        private string getRandomTip()
        {
            string[] tips =
            {
                "Press Ctrl-T anywhere in the game to toggle the toolbar!",
                "Press Ctrl-O anywhere in the game to access options!",
                "All settings are dynamic and take effect in real-time. Try changing the skin while watching autoplay!",
                "New features are coming online every update. Make sure to stay up-to-date!",
                "If you find the UI too large or small, try adjusting UI scale in settings!",
                "Try adjusting the \"Screen Scaling\" mode to change your gameplay or UI area, even in fullscreen!",
                "What used to be \"osu!direct\" is available to all users just like on the website. You can access it anywhere using Ctrl-B!",
                "Seeking in replays is available by dragging on the difficulty bar at the bottom of the screen or by using the left and right arrow keys!",
                "Multithreading support means that even with low \"FPS\" your input and judgements will be accurate!",
                "Try scrolling right in mod select to find a bunch of new fun mods!",
                "Most of the web content (profiles, rankings, etc.) are available natively in-game from the icons on the toolbar!",
                "Get more details, hide or delete a beatmap by right-clicking on its panel at song select!",
                "All delete operations are temporary until exiting. Restore accidentally deleted content from the maintenance settings!",
                "Check out the \"playlists\" system, which lets users create their own custom and permanent leaderboards!",
                "Toggle advanced frame / thread statistics with Ctrl-F11!",
                "Take a look under the hood at performance counters and enable verbose performance logging with Ctrl-F2!",
                "You can pause during a replay by pressing Space!",
                "Most of the hotkeys in the game are configurable and can be changed to anything you want. Check the bindings panel under input settings!",
                "When your gameplay HUD is hidden, you can press and hold Ctrl to view it temporarily!",
                "Your gameplay HUD can be customized by using the skin layout editor. Open it at any time via Ctrl-Shift-S!",
                "Drag and drop any image into the skin editor to load it in quickly!",
                "You can create mod presets to make toggling your favorite mod combinations easier!",
                "Many mods have customisation settings that drastically change how they function. Click the Mod Customisation button in mod select to view settings!",
                "Press Ctrl-Shift-R to switch to a random skin!",
                "Press Ctrl-Shift-F to toggle the FPS Counter. But make sure not to pay too much attention to it!",
                "While watching a replay, press Ctrl-H to toggle replay settings!",
                "You can easily copy the mods from scores on a leaderboard by right-clicking on them!",
                "Ctrl-Enter at song select will start a beatmap in autoplay mode!"
            };

            return tips[RNG.Next(0, tips.Length)];
        }
    }
}
