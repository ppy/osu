// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuPool<T> : DrawablePool<T>
        where T : DrawableHitObject, new()
    {
        private readonly Func<DrawableHitObject, double, bool> checkHittable;
        private readonly Action<Drawable> onLoaded;

        public DrawableOsuPool(Func<DrawableHitObject, double, bool> checkHittable, Action<Drawable> onLoaded, int initialSize, int? maximumSize = null)
            : base(initialSize, maximumSize)
        {
            this.checkHittable = checkHittable;
            this.onLoaded = onLoaded;
        }

        protected override T CreateNewDrawable() => base.CreateNewDrawable().With(o =>
        {
            var osuObject = (DrawableOsuHitObject)(object)o;

            osuObject.CheckHittable = checkHittable;
            osuObject.OnLoadComplete += onLoaded;
        });
    }
}
