// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenCustom : BackgroundScreen
    {
        private readonly string textureName;

        public BackgroundScreenCustom(string textureName)
        {
            this.textureName = textureName;
            Add(new Background(textureName));
        }

        public override bool Equals(BackgroundScreen other)
        {
            return base.Equals(other) && textureName == ((BackgroundScreenCustom)other).textureName;
        }
    }
}