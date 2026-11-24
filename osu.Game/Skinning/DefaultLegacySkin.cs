// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            Name = "osu! \"classic\" (2013)",
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
            : base(skin, resources, new NamespacedResourceStore<byte[]>(resources.Resources, "Skins/Legacy"))
        {
        }

        protected override void SetDefaultConfiguration()
        {
            base.SetDefaultConfiguration();

            Configuration.CustomColours["SliderBall"] = new Color4(2, 170, 255, 255);

            Configuration.ConfigDictionary[nameof(SkinConfiguration.LegacySetting.AllowSliderBallTint)] = @"true";

            Configuration.LegacyVersion = 2.7m;
            Configuration.IsLatestVersion = true;
        }
    }
}
