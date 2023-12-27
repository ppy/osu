// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class AudioOffsetAdjustControl : SettingsItem<double>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
        }

        protected override Drawable CreateControl() => new AudioOffsetPreview();

        private partial class AudioOffsetPreview : CompositeDrawable
        {
        }
    }
}
