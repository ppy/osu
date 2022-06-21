// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class ColoursSection : SetupSection
    {
        public override LocalisableString Title => "Colours";

        private LabelledColourPalette comboColours;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                comboColours = new LabelledColourPalette
                {
                    Label = "Hitcircle / Slider Combos",
                    FixedLabelWidth = LABEL_WIDTH,
                    ColourNamePrefix = "Combo"
                }
            };

            if (Beatmap.BeatmapSkin != null)
                comboColours.Colours.BindTo(Beatmap.BeatmapSkin.ComboColours);
        }
    }
}
