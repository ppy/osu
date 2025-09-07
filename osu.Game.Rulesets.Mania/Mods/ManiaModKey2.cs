// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey2 : ManiaKeyMod
    {
        public override int KeyCount => 2;
        public override string Name => "Two Keys";
        public override string Acronym => "2K";
        public override IconUsage? Icon => OsuIcon.ModTwoKeys;
        public override LocalisableString Description => @"Play with two keys.";
        public override bool Ranked => false;
    }
}
