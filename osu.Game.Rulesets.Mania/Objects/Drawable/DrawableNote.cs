// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Objects.Drawable
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

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get(@"Menu/logo");

            const double duration = 0;

            Transforms.Add(new TransformPositionY { StartTime = note.StartTime - 200, EndTime = note.StartTime, StartValue = -0.1f, EndValue = 0.9f });
            Transforms.Add(new TransformAlpha { StartTime = note.StartTime + duration + 200, EndTime = note.StartTime + duration + 400, StartValue = 1, EndValue = 0 });
            Expire(true);
        }
    }
}
