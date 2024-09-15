// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods.CipherTransformers;
using osuTK;

public enum Transformers
{
    [Description("None")]
    None,

    [Description("Circle dance")]
    CircleDance
}

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModCipher : ModCipher
    {
        public override LocalisableString Description => "Cipher for Osu";
        public override Type[] IncompatibleMods => [];

        [SettingSource("Transformer", "Transformer used to encode text into play data")]
        public Bindable<Transformers> Transformer { get; } = new Bindable<Transformers>();

        public override Func<Vector2, Vector2>? TransformMouseInput
        {
            get
            {
                setUpCipherTransformer();

                if (cipherTransformer != null)
                {
                    return cipherTransformer.Transform;
                }

                return null;
            }
            set => base.TransformMouseInput = value;
        }

        private CipherTransformer? cipherTransformer;

        /// <summary>
        /// Here CipherTransformers are initialized with values from mod's Customize menu
        /// </summary>
        private void setUpCipherTransformer()
        {
            switch (Transformer.Value)
            {
                case Transformers.CircleDance:
                    cipherTransformer = new CircleDanceTransformer(100f, 2f);
                    break;

                default:
                case Transformers.None:
                    break;
            }
        }
    }
}
