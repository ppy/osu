// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
            var backgroundScreenCustom = other as BackgroundScreenCustom;
            if (backgroundScreenCustom == null) return false;

            return base.Equals(other) && textureName == backgroundScreenCustom.textureName;
        }
    }
}
