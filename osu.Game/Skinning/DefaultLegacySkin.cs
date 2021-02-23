// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Stores;
using osu.Game.IO;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultLegacySkin : LegacySkin
    {
        public DefaultLegacySkin(IResourceStore<byte[]> storage, IStorageResourceProvider resources)
            : base(Info, storage, resources, string.Empty)
        {
            Configuration.CustomColours["SliderBall"] = new Color4(2, 170, 255, 255);
            Configuration.AddComboColours(
                new Color4(255, 192, 0, 255),
                new Color4(0, 202, 0, 255),
                new Color4(18, 124, 255, 255),
                new Color4(242, 24, 57, 255)
            );

            Configuration.LegacyVersion = 2.7m;
        }

        public static SkinInfo Info { get; } = new SkinInfo
        {
            ID = SkinInfo.CLASSIC_SKIN, // this is temporary until database storage is decided upon.
            Name = "osu!classic",
            Creator = "team osu!"
        };
    }
}
