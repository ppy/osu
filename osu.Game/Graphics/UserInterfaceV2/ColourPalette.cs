// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class ColourPalette : CompositeDrawable
    {
        public BindableList<Color4> Colours { get; } = new BindableList<Color4>();

        private string colourNamePrefix = "Colour";

        public string ColourNamePrefix
        {
            get => colourNamePrefix;
            set
            {
                if (colourNamePrefix == value)
                    return;

                colourNamePrefix = value;

                if (IsLoaded)
                    reindexItems();
            }
        }

        private FillFlowContainer<ColourDisplay> palette;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = palette = new FillFlowContainer<ColourDisplay>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Direction = FillDirection.Full
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Colours.BindCollectionChanged((_, __) => updatePalette(), true);
        }

        private void updatePalette()
        {
            palette.Clear();

            foreach (var item in Colours)
            {
                palette.Add(new ColourDisplay
                {
                    Current = { Value = item }
                });
            }

            reindexItems();
        }

        private void reindexItems()
        {
            int index = 1;

            foreach (var colour in palette)
            {
                colour.ColourName = $"{colourNamePrefix} {index}";
                index += 1;
            }
        }
    }
}
