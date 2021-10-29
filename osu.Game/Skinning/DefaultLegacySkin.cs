// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.IO.Stores;
using osu.Game.Extensions;
using osu.Game.IO;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultLegacySkin : LegacySkin
    {
        public DefaultLegacySkin(IStorageResourceProvider resources)
            : this(Info, resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public DefaultLegacySkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(
                skin,
                new NamespacedResourceStore<byte[]>(resources.Resources, "Skins/Legacy"),
                resources,
                // A default legacy skin may still have a skin.ini if it is modified by the user.
                // We must specify the stream directly as we are redirecting storage to the osu-resources location for other files.
                new LegacySkinResourceStore<SkinFileInfo>(skin, resources.Files).GetStream("skin.ini")
            )
        {
            Configuration.CustomColours["SliderBall"] = new Color4(2, 170, 255, 255);
            Configuration.CustomComboColours = new List<Color4>
            {
                new Color4(255, 192, 0, 255),
                new Color4(0, 202, 0, 255),
                new Color4(18, 124, 255, 255),
                new Color4(242, 24, 57, 255)
            };

            Configuration.LegacyVersion = 2.7m;
        }

        public static SkinInfo Info { get; } = new SkinInfo
        {
            ID = SkinInfo.CLASSIC_SKIN, // this is temporary until database storage is decided upon.
            Name = "osu!classic",
            Creator = "team osu!",
            InstantiationInfo = typeof(DefaultLegacySkin).GetInvariantInstantiationInfo()
        };
    }
}
