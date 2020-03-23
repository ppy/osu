// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public abstract class DashboardDisplay : CompositeDrawable
    {
        protected DashboardDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        public abstract void Fetch();
    }

    public abstract class DashboardDisplay<TModel> : DashboardDisplay
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private APIRequest<TModel> request;

        public override void Fetch()
        {
            if (!api.IsLoggedIn)
                return;

            request = CreateRequest();
            request.Success += response => Schedule(() => OnSuccess(response));
            api.Queue(request);
        }

        protected abstract APIRequest<TModel> CreateRequest();

        protected abstract void OnSuccess(TModel response);

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
