// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModCipher : ModCipher
    {
        public override LocalisableString Description => "Cipher for Osu";
        public override Type[] IncompatibleMods => [];

        public override Func<Vector2, Vector2> TransformMouseInput
        {
            get => vector2 =>
            {
                return vector2;
            };
            set => base.TransformMouseInput = value;
        }
    }
}
