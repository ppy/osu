// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Game.Audio;
using osu.Game.Extensions;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public class ArgonProSkin : ArgonSkin
    {
        public new static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.ARGON_PRO_SKIN,
            Name = "osu! \"argon\" pro (2022)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(ArgonProSkin).GetInvariantInstantiationInfo()
        };

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            foreach (string lookup in sampleInfo.LookupNames)
            {
                var sample = Samples?.Get(lookup)
                             ?? Resources.AudioManager?.Samples.Get(lookup.Replace(@"Gameplay/", @"Gameplay/ArgonPro/"))
                             ?? Resources.AudioManager?.Samples.Get(lookup.Replace(@"Gameplay/", @"Gameplay/Argon/"))
                             ?? Resources.AudioManager?.Samples.Get(lookup);

                if (sample != null)
                    return sample;
            }

            return null;
        }

        public ArgonProSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public ArgonProSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(skin, resources)
        {
        }
    }
}
