// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that creates a <see cref="Facade"/> to be used to update and track the position of an <see cref="OsuLogo"/>.
    /// </summary>
    public class LogoFacadeContainer : Container
    {
        protected virtual Facade CreateFacade() => new Facade();

        public Facade LogoFacade { get; }

        /// <summary>
        /// Whether or not the logo assigned to this FacadeContainer should be tracking the position its facade.
        /// </summary>
        public bool Tracking = false;

        protected OsuLogo Logo;

        private float facadeScale;
        private Easing easing;
        private Vector2? startPosition;
        private double? startTime;
        private double duration;

        public LogoFacadeContainer()
        {
            LogoFacade = CreateFacade();
        }

        /// <summary>
        /// Assign the logo that should track the Facade's position, as well as how it should transform to its initial position.
        /// </summary>
        /// <param name="logo">The instance of the logo to be used for tracking.</param>
        /// <param name="facadeScale">The scale of the facade. Does not actually affect the logo itself.</param>
        /// <param name="duration">The duration of the initial transform. Default is instant.</param>
        /// <param name="easing">The easing type of the initial transform.</param>
        public void SetLogo(OsuLogo logo, float facadeScale = 1.0f, double duration = 0, Easing easing = Easing.None)
        {
            Logo = logo ?? throw new ArgumentNullException(nameof(logo));
            this.facadeScale = facadeScale;
            this.duration = duration;
            this.easing = easing;

            startTime = null;
            startPosition = null;
        }

        protected Vector2 LogoTrackingPosition()
        {
            Vector2 local;
            local.X = Logo.Parent.ToLocalSpace(LogoFacade.ScreenSpaceDrawQuad.Centre).X / Logo.Parent.RelativeToAbsoluteFactor.X;
            local.Y = Logo.Parent.ToLocalSpace(LogoFacade.ScreenSpaceDrawQuad.Centre).Y / Logo.Parent.RelativeToAbsoluteFactor.Y;
            return local;
        }

        protected override void Update()
        {
            base.Update();

            if (Logo == null || !Tracking)
                return;

            // Account for the scale of the actual logo container, as SizeForFlow only accounts for the sprite scale.
            LogoFacade.Size = new Vector2(Logo.SizeForFlow * Logo.Scale.X * facadeScale);

            if (LogoFacade.Parent != null && Logo.Position != LogoTrackingPosition())
            {
                // If this is our first update since tracking has started, initialize our starting values for interpolation
                if (startTime == null || startPosition == null)
                {
                    startTime = Time.Current;
                    startPosition = Logo.Position;
                }

                if (duration != 0)
                {
                    double elapsedDuration = Time.Current - startTime ?? throw new ArgumentNullException(nameof(startTime));

                    var mount = (float)Interpolation.ApplyEasing(easing, Math.Min(elapsedDuration / duration, 1));

                    // Interpolate the position of the logo, where mount 0 is where the logo was when it first began interpolating, and mount 1 is the target location.
                    Logo.Position = Vector2.Lerp(startPosition ?? throw new ArgumentNullException(nameof(startPosition)), LogoTrackingPosition(), mount);
                }
                else
                {
                    Logo.Position = LogoTrackingPosition();
                }
            }
        }

        /// <summary>
        /// A placeholder container that serves as a dummy object to denote another object's location and size.
        /// </summary>
        public class Facade : Container
        {
        }
    }
}
