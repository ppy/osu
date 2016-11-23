//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundModeCustom : BackgroundMode
    {
        private readonly string textureName;

        public BackgroundModeCustom(string textureName)
        {
            this.textureName = textureName;
            Add(new Background(textureName));
        }

        public override bool Equals(BackgroundMode other)
        {
            return base.Equals(other) && textureName == ((BackgroundModeCustom)other).textureName;
        }
    }
}