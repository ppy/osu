// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Overlays
{
    public abstract partial class TabbableOnlineOverlay<THeader, TEnum> : OnlineOverlay<THeader>
        where THeader : TabControlOverlayHeader<TEnum>
    {
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        private CancellationTokenSource cancellationToken;
        private bool displayUpdateRequired = true;

        protected TabbableOnlineOverlay(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Header.Current.BindValueChanged(tab => OnTabChanged(tab.NewValue));
        }

        protected override void PopIn()
        {
            base.PopIn();

            // We don't want to create a new display on every call, only when exiting from fully closed state.
            if (displayUpdateRequired)
            {
                Header.Current.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            LoadDisplay(Empty());
            displayUpdateRequired = true;
        }

        protected void LoadDisplay(Drawable display)
        {
            ScrollFlow.ScrollToStart();

            LoadComponentAsync(display, loaded =>
            {
                Loading.Hide();

                Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected virtual void OnTabChanged(TEnum tab)
        {
            cancellationToken?.Cancel();
            Loading.Show();

            if (!API.IsLoggedIn)
            {
                LoadDisplay(Empty());
                return;
            }

            CreateDisplayToLoad(tab);
        }

        protected abstract void CreateDisplayToLoad(TEnum tab);

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            if (State.Value == Visibility.Hidden)
                return;

            Header.Current.TriggerChange();
        });

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
