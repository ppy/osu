// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class ColoursSection : SetupSection
    {
        public override LocalisableString Title => "配色";

        private LabelledColourPalette comboColours;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                comboColours = new LabelledColourPalette
                {
                    Label = "物件 / 滑条连击",
                    FixedLabelWidth = LABEL_WIDTH,
                    ColourNamePrefix = "连击"
                }
            };

            if (Beatmap.BeatmapSkin != null)
                comboColours.Colours.BindTo(Beatmap.BeatmapSkin.ComboColours);
        }
    }
}
