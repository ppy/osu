// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Users
{
    public class Badge : IHasDrawableRepresentation<Sprite>
    {
        public string Name;
        public Texture Texture; // TODO: Replace this with something better

        public Sprite CreateDrawable()
        {
            return new Sprite
            {
                Texture = Texture,
            };
        }
    }
}
