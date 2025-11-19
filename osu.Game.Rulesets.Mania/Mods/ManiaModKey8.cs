// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey8 : ManiaKeyMod
    {
        public override int KeyCount => 8;
        public override string Name => "Eight Keys";
        public override string Acronym => "8K";
        public override IconUsage? Icon => OsuIcon.ModEightKeys;
        public override LocalisableString Description => @"Play with eight keys.";
    }
}
