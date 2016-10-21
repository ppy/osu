//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;

namespace osu.Game.Overlays
{
    public class Options : OverlayContainer
    {
        private const float width = 300;

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Depth = float.MaxValue;
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(width, 1);
            Position = new Vector2(-width, 0);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.9f)
                }
            };
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (State == Visibility.Hidden) return false;

                    State = Visibility.Hidden;
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        protected override void PopIn()
        {
            MoveToX(0, 300, EasingTypes.Out);
        }

        protected override void PopOut()
        {
            MoveToX(-width, 300, EasingTypes.Out);
        }
    }
}
