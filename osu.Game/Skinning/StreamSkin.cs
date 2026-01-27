// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.IO.Stores;
using osu.Game.Extensions;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    /// <summary>
    /// The skin from osu!stream.
    /// </summary>
    /// <remarks>
    /// The assets were taken from the osu!stream GitHub repository: https://github.com/ppy/osu-stream/tree/master/Artwork, https://github.com/ppy/osu-stream/tree/master/osu!stream/Skins/Default
    /// </remarks>
    public class StreamSkin : LegacySkin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.STREAM_SKIN,
            Name = "osu! \"stream\" (2011)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(StreamSkin).GetInvariantInstantiationInfo(),
        };

        public StreamSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public StreamSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(
                skin,
                resources,
                new NamespacedResourceStore<byte[]>(resources.Resources, "Skins/Stream")
            )
        {
        }
    }
}
