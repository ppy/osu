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
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.CLASSIC_SKIN, // this is temporary until database storage is decided upon.
            Name = "osu!classic",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(DefaultLegacySkin).GetInvariantInstantiationInfo()
        };

        public DefaultLegacySkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public DefaultLegacySkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(
                skin,
                resources,
                // In the case of the actual default legacy skin (ie. the fallback one, which a user hasn't applied any modifications to) we want to use the game provided resources.
                skin.Protected ? new NamespacedResourceStore<byte[]>(resources.Resources, "Skins/Legacy") : null
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
    }
}
