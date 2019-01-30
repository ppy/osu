// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class PlayfieldAdjustmentContainer : Container
    {
        private const float default_relative_height = TaikoPlayfield.DEFAULT_HEIGHT / 768;
        private const float default_aspect = 16f / 9f;

        protected override void Update()
        {
            base.Update();

            float aspectAdjust = MathHelper.Clamp(Parent.ChildSize.X / Parent.ChildSize.Y, 0.4f, 4) / default_aspect;
            Size = new Vector2(1, default_relative_height * aspectAdjust);
        }
    }
}
