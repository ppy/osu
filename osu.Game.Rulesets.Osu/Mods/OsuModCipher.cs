// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public abstract class OsuModCipher : ModCipher
    {
        public override IconUsage? Icon => null;

        public override ModType Type => ModType.Ciphers;

        public override Type[] IncompatibleMods => [];

        public override Func<Vector2, Vector2>? TransformMouseInput
        {
            get => Transform;
            set => base.TransformMouseInput = value;
        }

        /// <summary>
        /// Runs every replay frame used by ReplayRecorder
        /// </summary>
        public abstract Vector2 Transform(Vector2 mousePosition);
    }
}
