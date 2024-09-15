// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModCipher : ModCipher
    {
        public override LocalisableString Description => "Cipher for Osu";
        public override Type[] IncompatibleMods => [];

        public override Func<Vector2, Vector2>? TransformMouseInput
        {
            get => circleDanceTransform;
            set => base.TransformMouseInput = value;
        }

        private float arcCounter;

        private Vector2 circleDanceTransform(Vector2 center)
        {
            const float radius = 100f;
            float x = (float)(radius * Math.Cos(arcCounter));
            float y = (float)(radius * Math.Sin(arcCounter));
            arcCounter += 2f;
            return center + new Vector2(x, y);
        }
    }
}
