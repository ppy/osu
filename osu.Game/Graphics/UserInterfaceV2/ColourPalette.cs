// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A component which displays a collection of colours in individual <see cref="ColourDisplay"/>s.
    /// </summary>
    public partial class ColourPalette : CompositeDrawable
    {
        public BindableList<Colour4> Colours { get; } = new BindableList<Colour4>();

        private LocalisableString colourNamePrefix = "Colour";

        public LocalisableString ColourNamePrefix
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

        private FillFlowContainer palette;

        private IEnumerable<ColourDisplay> colourDisplays => palette.OfType<ColourDisplay>();

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeDuration = fade_duration;
            AutoSizeEasing = Easing.OutQuint;

            InternalChild = palette = new FillFlowContainer
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

            Colours.BindCollectionChanged((_, args) =>
            {
                if (args.Action != NotifyCollectionChangedAction.Replace)
                    updatePalette();
            }, true);
            FinishTransforms(true);
        }

        private const int fade_duration = 200;

        private void updatePalette()
        {
            palette.Clear();

            for (int i = 0; i < Colours.Count; ++i)
            {
                // copy to avoid accesses to modified closure.
                int colourIndex = i;
                ColourDisplay display;

                palette.Add(display = new ColourDisplay
                {
                    Current = { Value = Colours[colourIndex] }
                });

                display.Current.BindValueChanged(colour => Colours[colourIndex] = colour.NewValue);
                display.DeleteRequested += colourDeletionRequested;
            }

            palette.Add(new AddColourButton
            {
                Action = () => Colours.Add(Colour4.White)
            });

            reindexItems();
        }

        private void colourDeletionRequested(ColourDisplay display) => Colours.RemoveAt(palette.IndexOf(display));

        private void reindexItems()
        {
            int index = 1;

            foreach (var colourDisplay in colourDisplays)
            {
                colourDisplay.ColourName = $"{colourNamePrefix} {index}";
                index += 1;
            }
        }

        internal partial class AddColourButton : CompositeDrawable
        {
            public Action Action
            {
                set => circularButton.Action = value;
            }

            private readonly OsuClickableContainer circularButton;

            public AddColourButton()
            {
                AutoSizeAxes = Axes.Y;
                Width = 100;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        circularButton = new OsuClickableContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 100,
                            CornerRadius = 50,
                            Masking = true,
                            BorderThickness = 5,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.Transparent,
                                    AlwaysPresent = true
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(20),
                                    Icon = FontAwesome.Solid.Plus
                                }
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "New"
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                circularButton.BorderColour = colours.BlueDarker;
            }
        }
    }
}
