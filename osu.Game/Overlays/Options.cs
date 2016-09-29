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

namespace osu.Game.Overlays
{
    public class Options : Container
    {
        const float width = 300;

        public override void Load()
        {
            base.Load();

            Depth = float.MaxValue;
            SizeMode = InheritMode.Y;
            Size = new Vector2(width, 1);
            Position = new Vector2(-width, 0);

            Children = new Drawable[]
            {
                new Box
                {
                    SizeMode = InheritMode.XY,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.9f)
                }
            };
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (!poppedOut) return false;

                    PoppedOut = false;
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        private bool poppedOut;

        public bool PoppedOut
        {
            get { return poppedOut; }

            set
            {
                if (value == poppedOut) return;

                poppedOut = value;

                if (poppedOut)
                {
                    MoveTo(new Vector2(0, 0), 300, EasingTypes.Out);
                }
                else
                {
                    MoveTo(new Vector2(-width, 0), 300, EasingTypes.Out);
                }

            }
        }
    }
}
