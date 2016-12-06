//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Audio
{
    public class OffsetAdjustmentOptions : OptionsSubsection
    {
        protected override string Header => "Offset Adjustment";

        public OffsetAdjustmentOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Universal Offset: TODO slider" },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Offset wizard"
                }
            };
        }
    }
}