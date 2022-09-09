// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenCustom : BackgroundScreen
    {
        private readonly string textureName;

        public BackgroundScreenCustom(string textureName)
        {
            this.textureName = textureName;
            AddInternal(new Background(textureName));
        }

        public override bool Equals(BackgroundScreen other)
        {
            if (other is BackgroundScreenCustom backgroundScreenCustom)
                return base.Equals(other) && textureName == backgroundScreenCustom.textureName;

            return false;
        }
    }
}
