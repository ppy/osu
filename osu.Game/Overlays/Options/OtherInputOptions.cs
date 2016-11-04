using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OtherInputOptions : OptionsSubsection
    {
        protected override string Header => "Other";
    
        public OtherInputOptions()
        {
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "OS TabletPC support" },
                new BasicCheckBox { LabelText = "Wiimote/TaTaCon Drum Support" },
            };
        }
    }
}

