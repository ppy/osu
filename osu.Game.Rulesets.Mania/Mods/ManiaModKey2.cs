// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey2 : ManiaKeyMod
    {
        public override int KeyCount => 2;
        public override string Name => "双键";
        public override string Acronym => "2K";
        public override LocalisableString Description => @"双键位模式";
    }
}
