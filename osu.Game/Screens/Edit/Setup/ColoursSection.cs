// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

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

            if (Beatmap.BeatmapSkin != null)
                comboColours.Colours.BindTo(Beatmap.BeatmapSkin.ComboColours);
        }
    }
}
