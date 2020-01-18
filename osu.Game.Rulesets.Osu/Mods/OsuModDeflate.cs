// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDeflate : OsuModObjectScaleTween
    {
        public override string Name => "Deflate";

        public override string Acronym => "DF";

        public override IconUsage? Icon => FontAwesome.Solid.CompressArrowsAlt;

        public override string Description => "在正确的大小击打物件!";

        protected override float StartScale => 2f;
    }
}
