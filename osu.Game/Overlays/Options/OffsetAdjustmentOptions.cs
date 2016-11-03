using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OffsetAdjustmentOptions : OptionsSubsection
    {
        public OffsetAdjustmentOptions()
        {
            Header = "Offset Adjustment";
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