// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays
{
    public abstract partial class PageableTabbleOnlineOverlay<THeader, TEnum> : TabbableOnlineOverlay<THeader, TEnum>
        where THeader : PagedTabControlOverlayHeader<TEnum>
    {
        protected PageableTabbleOnlineOverlay(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Header.CurrentPage.BindValueChanged(page => OnPageChanged(page.NewValue));
        }

        protected override void OnTabChanged(TEnum tab)
        {
            // Go back to first page if we switch to another tab
            Header.CurrentPage.SetDefault();
            base.OnTabChanged(tab);
        }

        protected virtual void OnPageChanged(int page)
        {
            base.OnTabChanged(Header.Current.Value);
        }
    }
}
