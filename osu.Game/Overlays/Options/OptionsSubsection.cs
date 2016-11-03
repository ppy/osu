using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public class OptionsSubsection : Container
    {
        private SpriteText header;
        private Container content;
        protected override Container Content => content;
        
        public string Header
        {
            get { return header.Text; }
            set { header.Text = value.ToUpper(); }
        }
    
        public OptionsSubsection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AddInternal(new Drawable[]
            {
                content = new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 5),
                    Children = new[]
                    {
                        header = new SpriteText
                        {
                            TextSize = 25,
                            // TODO: Bold
                        }
                    }
                },
            });
        }
    }
}

