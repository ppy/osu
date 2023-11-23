// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class AdjustedAttributesTooltip : CompositeDrawable, ITooltip
    {
        private Dictionary<string, Bindable<OldNewPair>> attributes = new Dictionary<string, Bindable<OldNewPair>>();

        private Container content;

        private FillFlowContainer attributesFillFlow;

        [Resolved]
        private OsuColour colours { get; set; } = null!;


        public AdjustedAttributesTooltip()
        {
            // Need to be initialized in constructor to ensure accessability in AddAttribute function
            InternalChild = content = new Container
            {
                AutoSizeAxes = Axes.Both
            };
            attributesFillFlow = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 15;

            content.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray1,
                    Alpha = 0.8f
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding {Vertical = 10, Horizontal = 15},
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "One or more values are being adjusted by mods that change speed.",
                        },
                        attributesFillFlow
                    }
                }
            });
        }

        private void checkAttributes()
        {
            foreach (var attribute in attributes)
            {
                if (!Precision.AlmostEquals(attribute.Value.Value.Old, attribute.Value.Value.New))
                {
                    content.Show();
                    return;
                }
            }
            content.Hide();
        }

        public void AddAttribute(string name)
        {
            Bindable<OldNewPair> newBindable = new Bindable<OldNewPair>();
            newBindable.BindValueChanged(_ => checkAttributes());
            attributes.Add(name, newBindable);
            attributesFillFlow.Add(new AttributeDisplay(name, newBindable.GetBoundCopy()));
        }
        public void UpdateAttribute(string name, double oldValue, double newValue)
        {
            Bindable<OldNewPair> attribute = attributes[name];

            OldNewPair attributeValue = attribute.Value;
            attributeValue.Old = oldValue;
            attributeValue.New = newValue;

            attribute.Value = attributeValue;
        }

        protected override void Update()
        {
        }
        public void SetContent(object content)
        {
        }

        public void Move(Vector2 pos)
        {
            Position = pos;
        }

        private struct OldNewPair
        {
            public double Old, New;
        }

        private partial class AttributeDisplay : CompositeDrawable
        {
            public Bindable<OldNewPair> AttributeValues = new Bindable<OldNewPair>();
            public string AttributeName;

            private OsuSpriteText text = new OsuSpriteText
            {
                Font = OsuFont.Default.With(weight: FontWeight.Bold)
            };
            public AttributeDisplay(string name, Bindable<OldNewPair> boundCopy)
            {
                AutoSizeAxes = Axes.Both;

                AttributeName = name;
                AttributeValues = boundCopy;
                InternalChild = text;
                AttributeValues.BindValueChanged(_ => update(), true);
            }

            private void update()
            {
                if (Precision.AlmostEquals(AttributeValues.Value.Old, AttributeValues.Value.New))
                {
                    Hide();
                }
                else
                {
                    Show();
                    text.Text = $"{AttributeName}: {(AttributeValues.Value.Old):0.0#} → {(AttributeValues.Value.New):0.0#}";
                }
            }
        }
    }
}
