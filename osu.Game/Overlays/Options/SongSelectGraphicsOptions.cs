using System;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class SongSelectGraphicsOptions : OptionsSubsection
    {
        public SongSelectGraphicsOptions()
        {
            Header = "Song Select";
            Children = new[]
            {
                new BasicCheckBox { LabelText = "Show thumbnails" }
            };
        }
    }
}