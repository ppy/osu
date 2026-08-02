// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.Break
{
    public partial class BlurredIcon : GlowIcon
    {
        public BlurredIcon()
        {
            EffectBlending = BlendingParameters.Additive;
            DrawOriginal = false;
        }
    }
}
