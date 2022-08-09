// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;
using System;

namespace osu.Game.Extensions
{
    public static class DrawableExtensions
    {
        /// <summary>
        /// Shakes this drawable.
        /// </summary>
        /// <param name="target">The target to shake.</param>
        /// <param name="shakeDuration">The length of a single shake.</param>
        /// <param name="shakeMagnitude">Pixels of displacement per shake.</param>
        /// <param name="maximumLength">The maximum length the shake should last.</param>
        public static void Shake(this Drawable target, double shakeDuration = 80, float shakeMagnitude = 8, double? maximumLength = null)
        {
            // if we don't have enough time, don't bother shaking.
            if (maximumLength < shakeDuration * 2)
                return;

            var sequence = target.MoveToX(shakeMagnitude, shakeDuration / 2, Easing.OutSine).Then()
                                 .MoveToX(-shakeMagnitude, shakeDuration, Easing.InOutSine).Then();

            // if we don't have enough time for the second shake, skip it.
            if (!maximumLength.HasValue || maximumLength >= shakeDuration * 4)
            {
                sequence = sequence
                           .MoveToX(shakeMagnitude, shakeDuration, Easing.InOutSine).Then()
                           .MoveToX(-shakeMagnitude, shakeDuration, Easing.InOutSine).Then();
            }

            sequence.MoveToX(0, shakeDuration / 2, Easing.InSine);
        }

        /// <summary>
        /// Accepts a delta vector in screen-space coordinates and converts it to one which can be applied to this drawable's position.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        /// <param name="delta">A delta in screen-space coordinates.</param>
        /// <returns>The delta vector in Parent's coordinates.</returns>
        public static Vector2 ScreenSpaceDeltaToParentSpace(this Drawable drawable, Vector2 delta) =>
            drawable.Parent.ToLocalSpace(drawable.Parent.ToScreenSpace(Vector2.Zero) + delta);

        public static SkinnableInfo CreateSkinnableInfo(this Drawable component) => new SkinnableInfo(component);

        public static void ApplySkinnableInfo(this Drawable component, SkinnableInfo info)
        {
            // todo: can probably make this better via deserialisation directly using a common interface.
            component.Position = info.Position;
            component.Rotation = info.Rotation;
            component.Scale = info.Scale;
            component.Anchor = info.Anchor;
            component.Origin = info.Origin;

            if (component is ISkinnableDrawable skinnable)
            {
                skinnable.UsesFixedAnchor = info.UsesFixedAnchor;

                foreach (var (_, property) in component.GetSettingsSourceProperties())
                {
                    if (!info.Settings.TryGetValue(property.Name.ToSnakeCase(), out object settingValue))
                        continue;

                    skinnable.CopyAdjustedSetting((IBindable)property.GetValue(component), settingValue);
                }
            }

            if (component is Container container)
            {
                foreach (var child in info.Children)
                    container.Add(child.CreateInstance());
            }
        }

        /// <summary>
        /// Keeps the drawable upright and unstretched preventing it from being rotated, sheared, scaled or flipped with its Parent.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public static void KeepUprightAndUnscaled(this Drawable drawable)
        {
            // Decomposes the inverse of the parent FrawInfo.Matrix into rotation, shear and scale.
            var parentMatrix = drawable.Parent.DrawInfo.Matrix;
            parentMatrix.Transpose();

            // Remove Translation.
            parentMatrix.M13 = 0.0f;
            parentMatrix.M23 = 0.0f;

            Matrix3 C = parentMatrix.Inverted();

            // Extract the rotation.
            float angle = MathF.Atan2(C.M21, C.M11);
            drawable.Rotation = MathHelper.RadiansToDegrees(angle);

            // Remove rotation from the C matrix so that it only contains shear and scale.
            Matrix3 m = Matrix3.CreateRotationZ(-angle);
            m.Transpose();
            C = m * C;

            // Extract shear and scale.
            float alpha, sx, sy;
            sx = C.M11;
            sy = C.M22;
            alpha = C.M12 / C.M22;

            drawable.Scale = new Vector2(sx, sy);
            drawable.Shear = new Vector2(-alpha, 0);
        }
    }
}
