// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Edit.Setup
{
    internal class ColoursSection : SetupSection
    {
        public override LocalisableString Title => EditorSetupColoursStrings.Colours;

        private LabelledColourPalette comboColours;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                comboColours = new LabelledColourPalette
                {
                    Label = EditorSetupColoursStrings.HitcircleSliderCombos,
                    FixedLabelWidth = LABEL_WIDTH,
                    ColourNamePrefix = MatchesStrings.MatchScoreStatsCombo
                }
            };

            if (Beatmap.BeatmapSkin != null)
                comboColours.Colours.BindTo(Beatmap.BeatmapSkin.ComboColours);
        }
    }
}
