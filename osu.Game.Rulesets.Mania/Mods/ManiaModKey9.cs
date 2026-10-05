// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey9 : ManiaKeyMod
    {
        public override int KeyCount => 9;
        public override string Name => "Nine Keys";
        public override string Acronym => "9K";
        public override IconUsage? Icon => OsuIcon.ModNineKeys;
        public override LocalisableString Description => @"Play with nine keys.";
    }
}
