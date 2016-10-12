//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;

namespace osu.Game.Graphics.Cursor
{
    class OsuCursorContainer : CursorContainer
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            ActiveCursor.Scale = new OpenTK.Vector2(1);
            ActiveCursor.ScaleTo(1.2f, 100, EasingTypes.OutQuad);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (!state.Mouse.HasMainButtonPressed)
                ActiveCursor.ScaleTo(1, 200, EasingTypes.OutQuad);
            return base.OnMouseUp(state, args);
        }

        class OsuCursor : AutoSizeContainer
        {
            public OsuCursor()
            {
                Origin = Anchor.Centre;
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);

                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = game.Textures.Get(@"Cursor/cursor")
                    }
                };
            }
        }
    }

}
