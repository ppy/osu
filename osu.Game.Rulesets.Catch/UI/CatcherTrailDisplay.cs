// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
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
        /// The most recent dash trail added in this container.
        /// Only alive (not faded out) trails are considered.
        /// </summary>
        [CanBeNull]
        public CatcherTrail LastDashTrail => dashTrails.Concat(hyperDashTrails)
                                                       .OrderByDescending(trail => trail.LifetimeStart)
                                                       .FirstOrDefault();

        public Color4 HyperDashTrailsColour => hyperDashTrails.Colour;

        public Color4 EndGlowSpritesColour => endGlowSprites.Colour;

        private readonly DrawablePool<CatcherTrail> trailPool;

        private readonly Container<CatcherTrail> dashTrails;
        private readonly Container<CatcherTrail> hyperDashTrails;
        private readonly Container<CatcherTrail> endGlowSprites;

        public CatcherTrailDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                trailPool = new DrawablePool<CatcherTrail>(30),
                dashTrails = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both },
                hyperDashTrails = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
                endGlowSprites = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
            };
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            hyperDashTrails.Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ?? Catcher.DEFAULT_HYPER_DASH_COLOUR;
            endGlowSprites.Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value ?? hyperDashTrails.Colour;
        }

        /// <summary>
        /// Displays a single end-glow catcher sprite.
        /// </summary>
        public void DisplayEndGlow(CatcherAnimationState animationState, float x, Vector2 scale)
        {
            var endGlow = createTrail(animationState, x, scale);

            endGlowSprites.Add(endGlow);

            endGlow.MoveToOffset(new Vector2(0, -10), 1200, Easing.In);
            endGlow.ScaleTo(endGlow.Scale * 0.95f).ScaleTo(endGlow.Scale * 1.2f, 1200, Easing.In);
            endGlow.FadeOut(1200);
            endGlow.Expire(true);
        }

        public void DisplayDashTrail(CatcherAnimationState animationState, float x, Vector2 scale, bool hyperDashing)
        {
            var sprite = createTrail(animationState, x, scale);

            if (hyperDashing)
                hyperDashTrails.Add(sprite);
            else
                dashTrails.Add(sprite);

            sprite.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
            sprite.Expire(true);
        }

        private CatcherTrail createTrail(CatcherAnimationState animationState, float x, Vector2 scale)
        {
            CatcherTrail sprite = trailPool.Get();

            sprite.AnimationState = animationState;
            sprite.Scale = scale;
            sprite.Position = new Vector2(x, 0);

            return sprite;
        }
    }
}
