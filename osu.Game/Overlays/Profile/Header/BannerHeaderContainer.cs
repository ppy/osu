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
    public partial class BannerHeaderContainer : FillFlowContainer
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
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

            var banners = user?.TournamentBanners;

            if (banners?.Length > 0)
            {
                Show();

                for (int index = 0; index < banners.Length; index++)
                {
                    int displayIndex = index;
                    LoadComponentAsync(new DrawableTournamentBanner(banners[index]), asyncBanner =>
                    {
                        // load in stable order regardless of async load order.
                        Insert(displayIndex, asyncBanner);
                    }, cancellationTokenSource.Token);
                }
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
