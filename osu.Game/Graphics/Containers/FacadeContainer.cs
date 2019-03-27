// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that creates a <see cref="Facade"/> to be used by its children.
    /// This container also updates the position and size of the Facade, and contains logic for tracking an <see cref="OsuLogo"/> on the Facade's position.
    /// </summary>
    public class FacadeContainer : Container
    {
        protected virtual Facade CreateFacade() => new Facade();

        public readonly Facade Facade;

        /// <summary>
        /// Whether or not the logo assigned to this FacadeContainer should be tracking the position its facade.
        /// </summary>
        public bool Tracking;

        private OsuLogo logo;
        private float facadeScale;
        private Vector2 startPosition;
        private Easing easing;
        private double startTime;
        private double duration;

        public FacadeContainer()
        {
            Facade = CreateFacade();
        }

        /// <summary>
        /// Assign the logo that should track the Facade's position, as well as how it should transform to its initial position.
        /// </summary>
        /// <param name="logo"> The instance of the logo to be used for tracking. </param>
        /// <param name="facadeScale"> The scale of the facade. </param>
        /// <param name="duration"> The duration of the initial transform. Default is instant.</param>
        /// <param name="easing"> The easing type of the initial transform. </param>
        public void SetLogo(OsuLogo logo, float facadeScale, double duration = 0, Easing easing = Easing.None)
        {
            if (logo != null)
            {
                this.logo = logo;
            }

            this.facadeScale = facadeScale;
            this.duration = duration;
            this.easing = easing;
        }

        private Vector2 logoTrackingPosition => logo.Parent.ToLocalSpace(Facade.ScreenSpaceDrawQuad.Centre);

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (logo == null || !Tracking)
                return;

            Facade.Size = new Vector2(logo.SizeForFlow * facadeScale);

            if (Facade.IsLoaded && logo.Position != logoTrackingPosition)
            {
                // Required for the correct position of the logo to be set with respect to logoTrackingPosition
                logo.RelativePositionAxes = Axes.None;

                // If this is our first update since tracking has started, initialize our starting values for interpolation
                if (startTime == 0)
                {
                    startTime = Time.Current;
                    startPosition = logo.Position;
                }

                var endTime = startTime + duration;
                var remainingDuration = endTime - Time.Current;

                // If our transform should be instant, our position should already be at logoTrackingPosition, thus set the blend to 0.
                // If we are already past when the transform should be finished playing, set the blend to 0 so that the logo is always at the position of the facade.
                var blend = duration > 0 && remainingDuration > 0
                    ? (float)Interpolation.ApplyEasing(easing, remainingDuration / duration)
                    : 0;

                // Interpolate the position of the logo, where blend 0 is the position of the Facade, and blend 1 is where the logo was when it first began interpolating.
                logo.Position = Vector2.Lerp(logoTrackingPosition, startPosition, blend);
            }
        }
    }
}

/// <summary>
/// A placeholder container that serves as a dummy object to denote another object's location and size.
/// </summary>
public class Facade : Container
{
}
