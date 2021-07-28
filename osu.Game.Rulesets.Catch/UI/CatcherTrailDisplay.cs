// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// Represents a component responsible for displaying
    /// the appropriate catcher trails when requested to.
    /// </summary>
    public class CatcherTrailDisplay : SkinReloadableDrawable
    {
        /// <summary>
        /// The most recent time a dash trail was added to this container.
        /// Only alive (not faded out) trails are considered.
        /// Returns <see cref="double.NegativeInfinity"/> if no dash trail is alive.
        /// </summary>
        public double LastDashTrailTime => getLastDashTrailTime();

        public Color4 HyperDashTrailsColour => hyperDashTrails.Colour;

        public Color4 HyperDashAfterImageColour => hyperDashAfterImages.Colour;

        private readonly DrawablePool<CatcherTrail> trailPool;

        private readonly Container<CatcherTrail> dashTrails;
        private readonly Container<CatcherTrail> hyperDashTrails;
        private readonly Container<CatcherTrail> hyperDashAfterImages;

        public CatcherTrailDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                trailPool = new DrawablePool<CatcherTrail>(30),
                dashTrails = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both },
                hyperDashTrails = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
                hyperDashAfterImages = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
            };
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            hyperDashTrails.Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ?? Catcher.DEFAULT_HYPER_DASH_COLOUR;
            hyperDashAfterImages.Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value ?? hyperDashTrails.Colour;
        }

        /// <summary>
        /// Displays a hyper-dash after-image of the catcher.
        /// </summary>
        public void DisplayHyperDashAfterImage(CatcherAnimationState animationState, float x, Vector2 scale)
        {
            var trail = createTrail(animationState, x, scale);

            hyperDashAfterImages.Add(trail);

            trail.MoveToOffset(new Vector2(0, -10), 1200, Easing.In);
            trail.ScaleTo(trail.Scale * 0.95f).ScaleTo(trail.Scale * 1.2f, 1200, Easing.In);
            trail.FadeOut(1200);
            trail.Expire(true);
        }

        public void DisplayDashTrail(CatcherAnimationState animationState, float x, Vector2 scale, bool hyperDashing)
        {
            var trail = createTrail(animationState, x, scale);

            if (hyperDashing)
                hyperDashTrails.Add(trail);
            else
                dashTrails.Add(trail);

            trail.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
            trail.Expire(true);
        }

        private CatcherTrail createTrail(CatcherAnimationState animationState, float x, Vector2 scale)
        {
            CatcherTrail trail = trailPool.Get();

            trail.AnimationState = animationState;
            trail.Scale = scale;
            trail.Position = new Vector2(x, 0);

            return trail;
        }

        private double getLastDashTrailTime()
        {
            double maxTime = double.NegativeInfinity;

            foreach (var trail in dashTrails)
                maxTime = Math.Max(maxTime, trail.LifetimeStart);

            foreach (var trail in hyperDashTrails)
                maxTime = Math.Max(maxTime, trail.LifetimeStart);

            return maxTime;
        }
    }
}
