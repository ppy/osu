using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class RendererOptions : OptionsSubsection
    {
        protected override string Header => "Renderer";
    
        public RendererOptions()
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                new SpriteText { Text = "Frame limiter: TODO dropdown" },
                new BasicCheckBox { LabelText = "Show FPS counter" },
                new BasicCheckBox { LabelText = "Reduce dropped frames" },
                new BasicCheckBox { LabelText = "Detect performance issues" },
            };
        }
    }
}