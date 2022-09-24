// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey10 : ManiaKeyMod
    {
        public override int KeyCount => 10;
        public override string Name => "十键";
        public override string Acronym => "10K";
        public override LocalisableString Description => @"十键位模式";
    }
}
