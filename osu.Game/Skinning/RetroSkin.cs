// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Extensions;
using osu.Game.IO;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skin that looks like osu!stable as it was around 2008.
    /// </summary>
    /// <remarks>
    /// "Around 2008" was chosen as the cutoff for this skin because that's when the look of core gameplay settled into its final design (until <see cref="DefaultLegacySkin"/>). Skin elements from later versions of osu! were preferred as long as they only fixed bugs or applied minor tweaks to 2008 elements.
    /// </remarks>
    public class RetroSkin : LegacySkin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.RETRO_SKIN,
            Name = "osu! \"retro\" (2008)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(RetroSkin).GetInvariantInstantiationInfo(),
        };

        public RetroSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public RetroSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(
                skin,
                resources,
                new NamespacedResourceStore<byte[]>(resources.Resources, "Skins/Retro")
            )
        {
            Configuration.ConfigDictionary[@"SliderBallFlip"] = "0";
            Configuration.ConfigDictionary[@"SliderBallFrames"] = "10";
            Configuration.ConfigDictionary[@"AllowSliderBallTint"] = "0";
            Configuration.ConfigDictionary[@"CursorTrailRotate"] = "0";
            Configuration.ConfigDictionary[@"Version"] = "1";

            Configuration.CustomComboColours =
            [
                new Color4(255, 150, 0, 255),
                new Color4(5, 240, 5, 255),
                new Color4(5, 5, 240, 255),
                new Color4(240, 5, 5, 255)
            ];

            Configuration.ConfigDictionary[@"HitCircleOverlap"] = "3";
            Configuration.ConfigDictionary[@"ScoreOverlap"] = "3";
            Configuration.ConfigDictionary[@"ComboOverlap"] = "3";
        }

        public override Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            // Retro taiko hit explosions use osu textures
            if (componentName.StartsWith("taiko-hit", StringComparison.Ordinal))
                componentName = componentName.Substring(6);

            // Retro taiko slider has no fail variant, but it needs to exist to avoid displaying nothing
            if (componentName == "taiko-slider-fail")
                componentName = "taiko-slider";

            return base.GetTexture(componentName, wrapModeS, wrapModeT);
        }
    }
}
