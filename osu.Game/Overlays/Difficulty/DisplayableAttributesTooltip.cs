// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Difficulty
{
    public partial class DisplayableAttributesTooltip : VisibilityContainer, ITooltip<IReadOnlyDictionary<string, LocalisableString>?>
    {
        private FillFlowContainer attributesFillFlow = null!;

        private Container content = null!;
        private IReadOnlyDictionary<string, LocalisableString>? displayableAttributes { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray3,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 10, Horizontal = 15 },
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                attributesFillFlow = new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    AutoSizeAxes = Axes.Both
                                }
                            }
                        }
                    }
                },
            };

            updateDisplay();
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        public void SetContent(IReadOnlyDictionary<string, LocalisableString>? attributes)
        {
            if (displayableAttributes != null && attributes != null && displayableAttributes.SequenceEqual(attributes))
            {
                return;
            }

            displayableAttributes = attributes;
            updateDisplay();
        }

        private void updateDisplay()
        {
            attributesFillFlow.Clear();

            if (displayableAttributes != null)
            {
                attributesFillFlow.AddRange(displayableAttributes.Select(kv => new DisplayableAttributeDisplay(kv.Key, kv.Value)));
            }

            if (attributesFillFlow.Any())
                content.Show();
            else
                content.Hide();
        }

        private partial class DisplayableAttributeDisplay : CompositeDrawable
        {
            public DisplayableAttributeDisplay(string name, LocalisableString value)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new OsuSpriteText
                {
                    Text = $"{name}: {value}"
                };
            }
        }
    }
}
