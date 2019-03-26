// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
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

        protected virtual Facade CreateFacade() => new Facade();

        public FacadeContainer()
        {
            facade = CreateFacade();
        }

        private Vector2 logoTrackingPosition => logo.Parent.ToLocalSpace(facade.ScreenSpaceDrawQuad.Centre);

        public void SetLogo(OsuLogo logo, double transformDelay = 0)
        {
            if (logo != null)
            {
                facade.Size = new Vector2(logo.SizeForFlow * 0.3f);
                this.logo = logo;
                Scheduler.AddDelayed(() =>
                {
                    tracking = true;
                }, transformDelay);
            }
        }

        private double startTime;
        private double duration = 1000;

        private Vector2 startPosition;
        private Easing easing = Easing.InOutExpo;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (logo == null || !tracking)
                return;

            facade.Size = new Vector2(logo.SizeForFlow * 0.3f);

            if (facade.IsLoaded && logo.Position != logoTrackingPosition)
            {
                if (logo.RelativePositionAxes != Axes.None)
                {
                    logo.Position = logo.Parent.ToLocalSpace(logo.Position);
                    logo.RelativePositionAxes = Axes.None;
                }

                if (startTime == 0)
                {
                    startTime = Time.Current;
                }

                var endTime = startTime + duration;
                var remainingDuration = endTime - Time.Current;

                if (remainingDuration <= 0)
                {
                    remainingDuration = 0;
                }

                float currentTime = (float)Interpolation.ApplyEasing(easing, remainingDuration / duration);
                logo.Position = Vector2.Lerp(logoTrackingPosition, startPosition, currentTime);
            }
        }
    }
}

public class Facade : Container
{
}