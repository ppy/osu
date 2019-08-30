// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacyCursor : CompositeDrawable
    {
        public LegacyCursor()
        {
            Size = new Vector2(50);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            InternalChildren = new Drawable[]
            {
                new NonPlayfieldSprite
                {
                    Texture = skin.GetTexture("cursormiddle"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new NonPlayfieldSprite
                {
                    Texture = skin.GetTexture("cursor"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }
    }
}
