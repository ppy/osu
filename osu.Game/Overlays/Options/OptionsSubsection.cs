//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public abstract class OptionsSubsection : Container
    {
        private Container<Drawable> content;
        protected override Container<Drawable> Content => content;

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
                            TextSize = 17,
                            Text = Header.ToUpper(),
                            Font = @"Exo2.0-Black",
                        }
                    }
                },
            });
        }
    }
}

