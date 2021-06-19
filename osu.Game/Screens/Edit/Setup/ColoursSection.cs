// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Skinning;
using osuTK.Graphics;

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

            var colours = Beatmap.BeatmapSkin?.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value;
            if (colours != null)
                comboColours.Colours.AddRange(colours);
        }
    }
}
