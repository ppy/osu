//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Framework;

namespace osu.Game.Overlays
{
    public class Options : Container, IStateful<Visibility>
    {
        const float width = 300;

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

        private Visibility state;

        public Visibility State
        {
            get { return state; }

            set
            {
                if (value == state) return;

                state = value;

                switch (state)
                {
                    case Visibility.Hidden:
                        MoveTo(new Vector2(-width, 0), 300, EasingTypes.Out);
                        break;
                    case Visibility.Visible:
                        MoveTo(new Vector2(0, 0), 300, EasingTypes.Out);
                        break;
                }
            }
        }
    }
}
