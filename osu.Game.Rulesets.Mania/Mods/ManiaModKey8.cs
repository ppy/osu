// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey8 : ManiaKeyMod
    {
        public override int KeyCount => 8;
        public override string Name => "八键";
        public override string Acronym => "8K";
        public override LocalisableString Description => @"八键位模式";
    }
}
