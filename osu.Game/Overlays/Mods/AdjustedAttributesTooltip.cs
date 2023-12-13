// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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
    public partial class AdjustedAttributesTooltip : VisibilityContainer, ITooltip
    {
        private readonly Dictionary<string, Bindable<OldNewPair>> attributes = new Dictionary<string, Bindable<OldNewPair>>();

        private FillFlowContainer? attributesFillFlow;

        private Container content = null!;

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
                                new OsuSpriteText
                                {
                                    Text = "One or more values are being adjusted by mods that change speed.",
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

            foreach (var attribute in attributes)
                attributesFillFlow?.Add(new AttributeDisplay(attribute.Key, attribute.Value.GetBoundCopy()));

            updateVisibility();
        }

        public void AddAttribute(string name)
        {
            Bindable<OldNewPair> newBindable = new Bindable<OldNewPair>();
            newBindable.BindValueChanged(_ => updateVisibility());
            attributes.Add(name, newBindable);

            attributesFillFlow?.Add(new AttributeDisplay(name, newBindable.GetBoundCopy()));
        }

        public void UpdateAttribute(string name, double oldValue, double newValue)
        {
            if (!attributes.ContainsKey(name)) return;

            Bindable<OldNewPair> attribute = attributes[name];

            OldNewPair attributeValue = attribute.Value;
            attributeValue.OldValue = oldValue;
            attributeValue.NewValue = newValue;

            attribute.Value = attributeValue;
        }

        protected override void Update()
        {
        }

        public void SetContent(object content)
        {
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        private void updateVisibility()
        {
            if (!IsLoaded)
                return;

            if (attributes.Any(attribute => !Precision.AlmostEquals(attribute.Value.Value.OldValue, attribute.Value.Value.NewValue)))
                content.Show();
            else
                content.Hide();
        }

        private partial class AttributeDisplay : CompositeDrawable
        {
            public readonly Bindable<OldNewPair> AttributeValues;
            public readonly string AttributeName;

            private readonly OsuSpriteText text;

            public AttributeDisplay(string name, Bindable<OldNewPair> values)
            {
                AutoSizeAxes = Axes.Both;

                AttributeName = name;
                AttributeValues = values;

                InternalChild = text = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(weight: FontWeight.Bold)
                };

                AttributeValues.BindValueChanged(_ => update(), true);
            }

            private void update()
            {
                if (Precision.AlmostEquals(AttributeValues.Value.OldValue, AttributeValues.Value.NewValue))
                {
                    Hide();
                    return;
                }

                Show();
                text.Text = $"{AttributeName}: {(AttributeValues.Value.OldValue):0.0#} → {(AttributeValues.Value.NewValue):0.0#}";
            }
        }

        private struct OldNewPair
        {
            public double OldValue, NewValue;
        }
    }
}
