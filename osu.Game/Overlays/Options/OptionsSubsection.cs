using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public abstract class OptionsSubsection : Container
    {
        private Container content;
        protected override Container Content => content;
        
        protected abstract string Header { get; }
    
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
                        new SpriteText
                        {
                            TextSize = 25,
                            Text = Header,
                            // TODO: Bold
                        }
                    }
                },
            });
        }
    }
}

