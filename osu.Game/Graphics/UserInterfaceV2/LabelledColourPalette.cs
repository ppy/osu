// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class LabelledColourPalette : LabelledDrawable<ColourPalette>
    {
        public LabelledColourPalette()
            : base(true)
        {
        }

        public BindableList<Color4> Colours => Component.Colours;

        public string ColourNamePrefix
        {
            get => Component.ColourNamePrefix;
            set => Component.ColourNamePrefix = value;
        }

        protected override ColourPalette CreateComponent() => new ColourPalette();
    }
}
