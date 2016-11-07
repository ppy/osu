using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class EditorOptions : OptionsSection
    {
        protected override string Header => "Editor";
        public override FontAwesome Icon => FontAwesome.fa_pencil;
    
        public EditorOptions()
        {
            content.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new BasicCheckBox { LabelText = "Background video" },
                new BasicCheckBox { LabelText = "Always use default skin" },
                new BasicCheckBox { LabelText = "Snaking sliders" },
                new BasicCheckBox { LabelText = "Hit animations" },
                new BasicCheckBox { LabelText = "Follow points" },
                new BasicCheckBox { LabelText = "Stacking" },
            };
        }
    }
}

