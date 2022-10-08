// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey6 : ManiaKeyMod
    {
        public override int KeyCount => 6;
        public override string Name => "六键";
        public override string Acronym => "6K";
        public override LocalisableString Description => @"六键位模式";
    }
}
