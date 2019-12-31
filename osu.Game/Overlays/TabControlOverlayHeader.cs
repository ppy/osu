// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public abstract class TabControlOverlayHeader<T> : ControllableOverlayHeader<T>
    {
        public readonly Bindable<T> Current = new Bindable<T>();

        protected OverlayHeaderTabControl<T> TabControl;

        protected override TabControl<T> CreateControl() => TabControl = new OverlayHeaderTabControl<T>(ColourScheme)
        {
            Current = Current
        };

        protected TabControlOverlayHeader(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
        }
    }
}
