// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModGrow : OsuModObjectScaleTween
    {
        public override string Name => "Grow";

        public override string Acronym => "GR";

        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAltV;

        public override string Description => "在正确的大小击打物件!";

        protected override float StartScale => 0.5f;
    }
}
