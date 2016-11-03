using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OtherInputOptions : OptionsSubsection
    {
        public OtherInputOptions()
        {
            Header = "Other";
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "OS TabletPC support" },
                new BasicCheckBox { LabelText = "Wiimote/TaTaCon Drum Support" },
            };
        }
    }
}

