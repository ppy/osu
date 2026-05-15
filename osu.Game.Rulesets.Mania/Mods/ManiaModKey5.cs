// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey5 : ManiaKeyMod
    {
        public override int KeyCount => 5;
        public override string Name => "Five Keys";
        public override string Acronym => "5K";
        public override IconUsage? Icon => OsuIcon.ModFiveKeys;
        public override LocalisableString Description => @"Play with five keys.";
    }
}
