using System;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Graphics
{
    public class SongSelectGraphicsOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";
    
        public SongSelectGraphicsOptions()
        {
            Children = new[]
            {
                new BasicCheckBox { LabelText = "Show thumbnails" }
            };
        }
    }
}