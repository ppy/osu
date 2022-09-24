// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey7 : ManiaKeyMod
    {
        public override int KeyCount => 7;
        public override string Name => "七键";
        public override string Acronym => "7K";
        public override LocalisableString Description => @"七键位模式";
    }
}
