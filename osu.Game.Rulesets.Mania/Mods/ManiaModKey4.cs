// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey4 : ManiaKeyMod
    {
        public override int KeyCount => 4;
        public override string Name => "Four Keys";
        public override string Acronym => "4K";
        public override IconUsage? Icon => OsuIcon.ModFourKeys;
        public override LocalisableString Description => @"Play with four keys.";
    }
}
