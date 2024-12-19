// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface.PageSelector;

namespace osu.Game.Overlays
{
    /// <inheritdoc />
    /// <summary>
    /// An extended overlay header that add a pagination support for a <see cref="TabControlOverlayHeader{T}" />
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public abstract partial class PagedTabControlOverlayHeader<TEnum> : TabControlOverlayHeader<TEnum>
    {
        private readonly PageSelector pageSelector;

        public BindableInt CurrentPage => pageSelector.CurrentPage;
        public BindableInt AvailablesPages => pageSelector.AvailablePages;

        protected PagedTabControlOverlayHeader()
        {
            HeaderInfo.Add(
                pageSelector = new PageSelector
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Vertical = 15 },
                });
        }

        public void ShowPageSelector() => pageSelector.Show();

        public void HidePageSelector() => pageSelector.Hide();
    }
}
