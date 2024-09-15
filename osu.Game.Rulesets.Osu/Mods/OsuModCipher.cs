// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModCipher : ModCipher
    {
        public override LocalisableString Description => "Cipher for Osu";
        public override Type[] IncompatibleMods => [];
    }
}
