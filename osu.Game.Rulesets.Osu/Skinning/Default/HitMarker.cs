// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class HitMarker : Sprite
    {
        private readonly OsuAction? action;

        public HitMarker(OsuAction? action = null)
        {
            this.action = action;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                    Texture = skin.GetTexture(@"hitmarker-left");
                    break;

                case OsuAction.RightButton:
                    Texture = skin.GetTexture(@"hitmarker-right");
                    break;

                default:
                    Texture = skin.GetTexture(@"aimmarker");
                    break;
            }
        }
    }
}
