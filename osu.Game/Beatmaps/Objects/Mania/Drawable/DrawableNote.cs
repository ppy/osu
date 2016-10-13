//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using OpenTK;

namespace osu.Game.Beatmaps.Objects.Mania.Drawable
{
    public class DrawableNote : Sprite
    {
        private readonly ManiaBaseHit note;

        public DrawableNote(ManiaBaseHit note)
        {
            this.note = note;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.1f);
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            Texture = game.Textures.Get(@"Menu/logo");

            Transforms.Add(new TransformPositionY(Clock) { StartTime = note.StartTime - 200, EndTime = note.StartTime, StartValue = -0.1f, EndValue = 0.9f });
            Transforms.Add(new TransformAlpha(Clock) { StartTime = note.StartTime + note.Duration + 200, EndTime = note.StartTime + note.Duration + 400, StartValue = 1, EndValue = 0 });
            Expire(true);
        }
    }
}
