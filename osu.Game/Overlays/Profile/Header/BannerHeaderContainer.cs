// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;

namespace osu.Game.Overlays.Profile.Header
{
    public partial class BannerHeaderContainer : CompositeDrawable
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            FillAspectRatio = 1000 / 60f;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(u => updateDisplay(u.NewValue?.User), true);
        }

        private CancellationTokenSource? cancellationTokenSource;

        private void updateDisplay(APIUser? user)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            ClearInternal();

            var banner = user?.TournamentBanner;

            if (banner != null)
            {
                Show();

                LoadComponentAsync(new DrawableTournamentBanner(banner), AddInternal, cancellationTokenSource.Token);
            }
            else
            {
                Hide();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
