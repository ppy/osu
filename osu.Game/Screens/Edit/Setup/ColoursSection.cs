// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Skinning;
using osu.Game.Utils;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ColoursSection : SetupSection
    {
        public override LocalisableString Title => EditorSetupStrings.ColoursHeader;

        private FormColourPalette comboColours = null!;
        private RoundedButton pickFromBackgroundButton = null!;

        private readonly BindableInt coloursToExtract = new BindableInt
        {
            Default = SkinConfiguration.DefaultComboColours.Count,
            MinValue = BackgroundComboColourExtractor.MIN_COLOUR_COUNT,
            MaxValue = BackgroundComboColourExtractor.MAX_COLOUR_COUNT,
        };

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private Editor? editor { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                comboColours = new FormColourPalette
                {
                    Caption = EditorSetupStrings.HitCircleSliderCombos,
                },
                new FormSliderBar<int>
                {
                    Caption = EditorSetupStrings.ColoursToExtract,
                    HintText = EditorSetupStrings.ColoursToExtractDescription,
                    Current = coloursToExtract,
                    TransferValueOnCommit = true,
                    TabbableContentContainer = this,
                },
                pickFromBackgroundButton = new RoundedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = EditorSetupStrings.PickComboColoursFromBackground,
                    TooltipText = EditorSetupStrings.PickComboColoursFromBackgroundDescription,
                    Action = () => dialogOverlay?.Push(new ConfirmDialog(EditorDialogsStrings.OverwriteComboColoursConfirmation, pickFromBackground)),
                },
            };
        }

        private bool syncingColours;

        protected override void LoadComplete()
        {
            if (Beatmap.BeatmapSkin != null)
                comboColours.Colours.AddRange(Beatmap.BeatmapSkin.ComboColours);

            if (comboColours.Colours.Count == 0)
            {
                // compare ctor of `EditorBeatmapSkin`
                for (int i = 0; i < SkinConfiguration.DefaultComboColours.Count; ++i)
                    comboColours.Colours.Add(SkinConfiguration.DefaultComboColours[(i + 1) % SkinConfiguration.DefaultComboColours.Count]);
            }

            coloursToExtract.Value = comboColours.Colours.Count;

            comboColours.Colours.BindCollectionChanged((_, _) =>
            {
                if (Beatmap.BeatmapSkin != null)
                {
                    if (syncingColours)
                        return;

                    syncingColours = true;

                    Beatmap.BeatmapSkin.ComboColours.Clear();
                    Beatmap.BeatmapSkin.ComboColours.AddRange(comboColours.Colours);

                    updateAddButtonVisibility();

                    syncingColours = false;
                }
            });

            Beatmap.BeatmapSkin?.ComboColours.BindCollectionChanged((_, _) =>
            {
                if (syncingColours)
                    return;

                syncingColours = true;

                comboColours.Colours.Clear();
                comboColours.Colours.AddRange(Beatmap.BeatmapSkin?.ComboColours);

                updateAddButtonVisibility();

                syncingColours = false;
            });

            working.BindValueChanged(_ => updatePickFromBackgroundState(), true);

            // Refresh disabled state on save (triggers when a background is added).
            if (editor != null)
                editor.Saved += updatePickFromBackgroundState;

            updateAddButtonVisibility();

            void updateAddButtonVisibility() => comboColours.CanAdd.Value = comboColours.Colours.Count < LegacyBeatmapDecoder.MAX_COMBO_COLOUR_COUNT;
        }

        private void pickFromBackground()
        {
            string backgroundFile = working.Value.Metadata.BackgroundFile;
            string? storagePath = working.Value.BeatmapSetInfo.GetPathForFile(backgroundFile);

            if (storagePath == null)
                return;

            try
            {
                using var stream = working.Value.GetStream(storagePath);
                var colours = BackgroundComboColourExtractor.Extract(stream, coloursToExtract.Value);

                comboColours.Colours.Clear();
                comboColours.Colours.AddRange(colours);
            }
            catch (Exception e)
            {
                Logger.Error(e, @"Failed to extract combo colours from background");
            }
        }

        private void updatePickFromBackgroundState()
        {
            bool canPick = !string.IsNullOrEmpty(working.Value.Metadata.BackgroundFile);

            pickFromBackgroundButton.Enabled.Value = canPick;
            coloursToExtract.Disabled = !canPick;
        }
    }
}
