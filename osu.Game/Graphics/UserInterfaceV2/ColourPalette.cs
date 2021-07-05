// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A component which displays a collection of colours in individual <see cref="ColourDisplay"/>s.
    /// </summary>
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
        private Container placeholder;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                palette = new FillFlowContainer<ColourDisplay>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Full
                },
                placeholder = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Text = "(none)",
                        Font = OsuFont.Default.With(weight: FontWeight.Bold)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Colours.BindCollectionChanged((_, __) => updatePalette(), true);
            FinishTransforms(true);
        }

        private const int fade_duration = 200;

        private void updatePalette()
        {
            palette.Clear();

            if (Colours.Any())
            {
                palette.FadeIn(fade_duration, Easing.OutQuint);
                placeholder.FadeOut(fade_duration, Easing.OutQuint);
            }
            else
            {
                palette.FadeOut(fade_duration, Easing.OutQuint);
                placeholder.FadeIn(fade_duration, Easing.OutQuint);
            }

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
