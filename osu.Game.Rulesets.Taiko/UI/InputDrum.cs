// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal partial class InputDrum : Container
    {
        public InputDrum()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.InputDrum), _ => new DefaultInputDrum())
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                },
            };
        }
    }
}
