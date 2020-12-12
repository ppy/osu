// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// Represents a component responsible for displaying
    /// the appropriate catcher trails when requested to.
    /// </summary>
    public class CatcherTrailDisplay : CompositeDrawable
    {
        private readonly Catcher catcher;

        private readonly DrawablePool<CatcherTrailSprite> trailPool;

        private readonly Container<CatcherTrailSprite> dashTrails;
        private readonly Container<CatcherTrailSprite> hyperDashTrails;
        private readonly Container<CatcherTrailSprite> endGlowSprites;

        private Color4 hyperDashTrailsColour = Catcher.DEFAULT_HYPER_DASH_COLOUR;

        public Color4 HyperDashTrailsColour
        {
            get => hyperDashTrailsColour;
            set
            {
                if (hyperDashTrailsColour == value)
                    return;

                hyperDashTrailsColour = value;
                hyperDashTrails.Colour = hyperDashTrailsColour;
            }
        }

        private Color4 endGlowSpritesColour = Catcher.DEFAULT_HYPER_DASH_COLOUR;

        public Color4 EndGlowSpritesColour
        {
            get => endGlowSpritesColour;
            set
            {
                if (endGlowSpritesColour == value)
                    return;

                endGlowSpritesColour = value;
                endGlowSprites.Colour = endGlowSpritesColour;
            }
        }

        private bool trail;

        /// <summary>
        /// Whether to start displaying trails following the catcher.
        /// </summary>
        public bool DisplayTrail
        {
            get => trail;
            set
            {
                if (trail == value)
                    return;

                trail = value;

                if (trail)
                    displayTrail();
            }
        }

        public CatcherTrailDisplay([NotNull] Catcher catcher)
        {
            this.catcher = catcher ?? throw new ArgumentNullException(nameof(catcher));

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                trailPool = new DrawablePool<CatcherTrailSprite>(30),
                dashTrails = new Container<CatcherTrailSprite> { RelativeSizeAxes = Axes.Both },
                hyperDashTrails = new Container<CatcherTrailSprite> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
                endGlowSprites = new Container<CatcherTrailSprite> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
            };
        }

        /// <summary>
        /// Displays a single end-glow catcher sprite.
        /// </summary>
        public void DisplayEndGlow()
        {
            var endGlow = createTrailSprite(endGlowSprites);

            endGlow.MoveToOffset(new Vector2(0, -10), 1200, Easing.In);
            endGlow.ScaleTo(endGlow.Scale * 0.95f).ScaleTo(endGlow.Scale * 1.2f, 1200, Easing.In);
            endGlow.FadeOut(1200);
            endGlow.Expire(true);
        }

        private void displayTrail()
        {
            if (!DisplayTrail)
                return;

            var sprite = createTrailSprite(catcher.HyperDashing ? hyperDashTrails : dashTrails);

            sprite.FadeTo(0.4f).FadeOut(800, Easing.OutQuint);
            sprite.Expire(true);

            Scheduler.AddDelayed(displayTrail, catcher.HyperDashing ? 25 : 50);
        }

        private CatcherTrailSprite createTrailSprite(Container<CatcherTrailSprite> target)
        {
            var texture = (catcher.CurrentDrawableCatcher as TextureAnimation)?.CurrentFrame ?? ((Sprite)catcher.CurrentDrawableCatcher).Texture;

            CatcherTrailSprite sprite = trailPool.Get();

            sprite.Texture = texture;
            sprite.Anchor = catcher.Anchor;
            sprite.Scale = catcher.Scale;
            sprite.Blending = BlendingParameters.Additive;
            sprite.RelativePositionAxes = catcher.RelativePositionAxes;
            sprite.Position = catcher.Position;

            target.Add(sprite);

            return sprite;
        }
    }
}
