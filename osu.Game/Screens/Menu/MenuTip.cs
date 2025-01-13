// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osu.Game.Localisation;

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

            var tip = getRandomTip();

            textFlow.Clear();
            textFlow.AddParagraph(MenuTipStrings.MenuTipTitle, formatSemiBold);
            textFlow.AddParagraph(tip, formatRegular);

            this.FadeInFromZero(200, Easing.OutQuint)
                .Delay(1000 + 80 * tip.ToString().Length)
                .Then()
                .FadeOutFromOne(2000, Easing.OutQuint);
        }

        private LocalisableString getRandomTip()
        {
            LocalisableString[] tips =
            {
                MenuTipStrings.ToggleToolbarShortcut,
                MenuTipStrings.GameSettingsShortcut,
                MenuTipStrings.DynamicSettings,
                MenuTipStrings.NewFeaturesAreComingOnline,
                MenuTipStrings.UIScalingSettings,
                MenuTipStrings.ScreenScalingSettings,
                MenuTipStrings.FreeOsuDirect,
                MenuTipStrings.ReplaySeeking,
                MenuTipStrings.MultithreadingSupport,
                MenuTipStrings.TryNewMods,
                MenuTipStrings.EmbeddedWebContent,
                MenuTipStrings.BeatmapRightClick,
                MenuTipStrings.TemporaryDeleteOperations,
                MenuTipStrings.DiscoverPlaylists,
                MenuTipStrings.ToggleAdvancedFPSCounter,
                MenuTipStrings.GlobalStatisticsShortcut,
                MenuTipStrings.ReplayPausing,
                MenuTipStrings.ConfigurableHotkeys,
                MenuTipStrings.PeekHUDWhenHidden,
                MenuTipStrings.SkinEditor,
                MenuTipStrings.DragAndDropImageInSkinEditor,
                MenuTipStrings.ModPresets,
                MenuTipStrings.ModCustomisationSettings,
                MenuTipStrings.RandomSkinShortcut,
                MenuTipStrings.ToggleReplaySettingsShortcut,
                MenuTipStrings.CopyModsFromScore,
                MenuTipStrings.AutoplayBeatmapShortcut,
                MenuTipStrings.LazerIsNotAWord
            };

            return tips[RNG.Next(0, tips.Length)];
        }
    }
}
