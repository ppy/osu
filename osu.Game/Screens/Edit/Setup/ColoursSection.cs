// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Formats;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Skinning;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ColoursSection : SetupSection
    {
        public override LocalisableString Title => EditorSetupStrings.ColoursHeader;

        private FormColourPalette comboColours = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                comboColours = new FormColourPalette
                {
                    Caption = EditorSetupStrings.HitCircleSliderCombos,
                }
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

            updateAddButtonVisibility();

            void updateAddButtonVisibility() => comboColours.CanAdd.Value = comboColours.Colours.Count < LegacyBeatmapDecoder.MAX_COMBO_COLOUR_COUNT;
        }
    }
}
