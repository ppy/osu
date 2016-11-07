//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Background;

namespace osu.Game.GameModes.Backgrounds
{
    public class BackgroundModeCustom : BackgroundMode
    {
        private readonly string textureName;
        private readonly Texture texture;

        public BackgroundModeCustom(string textureName)
        {
            this.textureName = textureName;
        }

        public BackgroundModeCustom(Texture texture)
        {
            this.texture = texture;
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            Add((texture != null) ? new Background(texture) : new Background(textureName));
        }

        public override bool Equals(BackgroundMode other)
        {
            return base.Equals(other) && textureName == ((BackgroundModeCustom)other).textureName;
        }
    }
}