// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey7 : ManiaKeyMod
    {
        public override int KeyCount => 7;
        public override string Name => "Seven Keys";
        public override string Acronym => "7K";
        public override IconUsage? Icon => OsuIcon.ModSevenKeys;
        public override LocalisableString Description => @"Play with seven keys.";
    }
}
