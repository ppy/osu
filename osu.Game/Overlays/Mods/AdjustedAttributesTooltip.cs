// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Difficulty;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class AdjustedAttributesTooltip : VisibilityContainer, ITooltip<AdjustedAttributesTooltip.Data?>
    {
        private readonly OverlayColourProvider? colourProvider;
        private FillFlowContainer attributesFillFlow = null!;

        private Container content = null!;

        private Data? data;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public AdjustedAttributesTooltip(OverlayColourProvider? colourProvider = null)
        {
            this.colourProvider = colourProvider;
        }

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
                            Colour = colourProvider?.Background4 ?? colours.Gray3,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 10, Horizontal = 15 },
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "One or more values are being adjusted by mods.",
                                },
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

        private void updateDisplay()
        {
            attributesFillFlow.Clear();

            if (data != null)
            {
                foreach (var attribute in data.Attributes)
                {
                    if (!Precision.AlmostEquals(attribute.Value, attribute.AdjustedValue))
                        attributesFillFlow.Add(new AttributeDisplay(attribute.Acronym, attribute.Value, attribute.AdjustedValue));
                }
            }

            if (attributesFillFlow.Any())
                content.Show();
            else
                content.Hide();
        }

        public void SetContent(Data? data)
        {
            if (this.data == data)
                return;

            this.data = data;
            updateDisplay();
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        public class Data
        {
            public IReadOnlyCollection<RulesetBeatmapAttribute> Attributes { get; }

            public Data(IReadOnlyCollection<RulesetBeatmapAttribute> attributes)
            {
                Attributes = attributes;
            }
        }

        private partial class AttributeDisplay : CompositeDrawable
        {
            public AttributeDisplay(string name, double original, double adjusted)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(weight: FontWeight.Bold),
                    Text = $"{name}: {original:0.0#} → {adjusted:0.0#}"
                };
            }
        }
    }
}
