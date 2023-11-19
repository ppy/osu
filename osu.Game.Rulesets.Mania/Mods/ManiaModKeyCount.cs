// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKeyCount : ManiaKeyMod
    {
        public override string Name => "Key Count";
        public override string Acronym => "KC";
        public override LocalisableString Description => @"Change the number of keys.";
    }
}
