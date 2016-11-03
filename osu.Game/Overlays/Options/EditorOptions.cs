using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class EditorOptions : OptionsSection
    {
        public EditorOptions()
        {
            Header = "Editor";
            Children = new Drawable[]
            {
                new OptionsSubsection
                {
                    Header = "General",
                    Children = new Drawable[]
                    {
                        new BasicCheckBox { LabelText = "Background video" },
                        new BasicCheckBox { LabelText = "Always use default skin" },
                        new BasicCheckBox { LabelText = "Snaking sliders" },
                        new BasicCheckBox { LabelText = "Hit animations" },
                        new BasicCheckBox { LabelText = "Follow points" },
                        new BasicCheckBox { LabelText = "Stacking" },
                    }
                }
            };
        }
    }
}

