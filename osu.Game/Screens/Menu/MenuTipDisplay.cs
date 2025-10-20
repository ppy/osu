// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Screens.Menu
{
    public partial class MenuTipDisplay : CompositeDrawable
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private LinkFlowContainer textFlow = null!;

        private Bindable<bool> showMenuTips = null!;

        [Resolved]
        private RealmKeyBindingStore keyBindingStore { get; set; } = null!;

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
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4Extensions.FromHex("#171A1C"),
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.75f,
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

            void formatSemiBold(SpriteText t)
            {
                t.Font = OsuFont.GetFont(Typeface.TorusAlternate, 16, weight: FontWeight.SemiBold);
                t.Colour = colours.Pink0;
            }

            var tip = getRandomTip();

            textFlow.Clear();
            textFlow.AddIcon(FontAwesome.Solid.Lightbulb, icon =>
            {
                icon.Colour = colours.Pink0;
                icon.Size = new Vector2(16);
            });
            textFlow.AddText(" ");
            textFlow.AddText(MenuTipStrings.MenuTipTitle.ToSentence(), formatSemiBold);
            textFlow.AddParagraph(tip, formatRegular);

            this
                .FadeOut()
                .ScaleTo(0.9f)
                .Delay(600)
                .FadeInFromZero(800, Easing.OutQuint)
                .ScaleTo(1, 800, Easing.OutElasticHalf)
                .Delay(1000 + 80 * tip.ToString().Length)
                .Then()
                .FadeOutFromOne(2000, Easing.OutQuint);
        }

        private const int available_tips = 30;

        private LocalisableString getRandomTip()
        {
            int tipIndex = RNG.Next(0, available_tips);

            switch (tipIndex)
            {
                case 0:
                    return MenuTipStrings.ToggleToolbarShortcut(
                        keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.ToggleToolbar).FirstOrDefault() ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 1:
                    return MenuTipStrings.GameSettingsShortcut(
                        keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.ToggleSettings).FirstOrDefault() ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 2:
                    return MenuTipStrings.DynamicSettings;

                case 3:
                    return MenuTipStrings.NewFeaturesAreComingOnline;

                case 4:
                    return MenuTipStrings.UIScalingSettings;

                case 5:
                    return MenuTipStrings.ScreenScalingSettings;

                case 6:
                    return MenuTipStrings.FreeOsuDirect(keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.ToggleBeatmapListing).FirstOrDefault()
                                                        ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 7:
                    return MenuTipStrings.ReplaySeeking;

                case 8:
                    return MenuTipStrings.MultithreadingSupport;

                case 9:
                    return MenuTipStrings.TryNewMods;

                case 10:
                    return MenuTipStrings.EmbeddedWebContent;

                case 11:
                    return MenuTipStrings.BeatmapRightClick;

                case 12:
                    return MenuTipStrings.TemporaryDeleteOperations;

                case 13:
                    return MenuTipStrings.DiscoverPlaylists;

                case 14:
                    return MenuTipStrings.ToggleAdvancedFPSCounter;

                case 15:
                    return MenuTipStrings.GlobalStatisticsShortcut;

                case 16:
                    return MenuTipStrings.ReplayPausing(keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.TogglePauseReplay).FirstOrDefault() ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 17:
                    return MenuTipStrings.ConfigurableHotkeys;

                case 18:
                    return MenuTipStrings.PeekHUDWhenHidden(keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.HoldForHUD).FirstOrDefault() ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 19:
                    return MenuTipStrings.SkinEditor(keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.ToggleSkinEditor).FirstOrDefault() ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 20:
                    return MenuTipStrings.DragAndDropImageInSkinEditor;

                case 21:
                    return MenuTipStrings.ModPresets;

                case 22:
                    return MenuTipStrings.ModCustomisationSettings;

                case 23:
                    return MenuTipStrings.RandomSkinShortcut(keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.RandomSkin).FirstOrDefault() ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 24:
                    return MenuTipStrings.ToggleReplaySettingsShortcut(keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.ToggleReplaySettings).FirstOrDefault()
                                                                       ?? InputSettingsStrings.ActionHasNoKeyBinding);

                case 25:
                    return MenuTipStrings.CopyModsFromScore;

                case 26:
                    return MenuTipStrings.AutoplayBeatmapShortcut;

                case 27:
                    return MenuTipStrings.LazerIsNotAWord;

                case 28:
                    return MenuTipStrings.RightMouseAbsoluteScroll;

                case 29:
                    return MenuTipStrings.ShiftClickInBeatmapOverlay;
            }

            return string.Empty;
        }
    }
}
