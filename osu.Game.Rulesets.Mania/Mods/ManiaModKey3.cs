// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey3 : ManiaKeyMod
    {
        public override int KeyCount => 3;
        public override string Name => "三键";
        public override string Acronym => "3K";
        public override LocalisableString Description => @"三键位模式";
    }
}
