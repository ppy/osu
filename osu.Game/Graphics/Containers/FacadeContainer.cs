// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public class FacadeContainer : Container
    {
        [Cached]
        private Facade facade;

        private OsuLogo logo;

        private bool tracking;
        private bool smoothTransform;

        public FacadeContainer()
        {
            facade = new Facade();
        }

        private Vector2 logoTrackingPosition => logo.Parent.ToLocalSpace(facade.ScreenSpaceDrawQuad.Centre);

        public void SetLogo(OsuLogo logo, bool resuming, double transformDelay)
        {
            if (logo != null)
            {
                facade.Size = new Vector2(logo.SizeForFlow * 0.3f);
                this.logo = logo;
                Scheduler.AddDelayed(() =>
                {
                    tracking = true;
                    smoothTransform = !resuming;
                }, transformDelay);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            facade.Size = new Vector2(logo.SizeForFlow * 0.3f);

            if (!tracking)
                return;

            logo.RelativePositionAxes = Axes.None;

            bool childrenLoaded = true;

            foreach (var d in Children)
            {
                if (!d.IsAlive)
                    childrenLoaded = false;
            }

            if (smoothTransform && childrenLoaded)
            {
                // Our initial movement to the tracking location should be smooth.
                Schedule(() => logo.MoveTo(logoTrackingPosition, 500, Easing.InOutExpo));
                smoothTransform = false;
            }
            else if (logo.Transforms.Count == 0)
            {
                // If all transforms have finished playing, the logo constantly track the position of the facade.
                logo.Position = logoTrackingPosition;
            }
        }
    }

    public class Facade : Container
    {
    }
}
