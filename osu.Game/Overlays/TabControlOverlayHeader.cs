// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Overlays
{
    public abstract class TabControlOverlayHeader<T> : ControllableOverlayHeader<OverlayHeaderTabControl<T>, T>
    {
        public readonly Bindable<T> Current = new Bindable<T>();

        protected override OverlayHeaderTabControl<T> CreateControl() => new OverlayHeaderTabControl<T>
        {
            Current = Current
        };
    }
}
