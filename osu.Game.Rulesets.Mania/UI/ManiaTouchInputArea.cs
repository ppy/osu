// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class ManiaTouchInputArea : CompositeDrawable, ISerialisableDrawable
    {
        [SettingSource("Spacing", "The spacing between receptors.")]
        public BindableFloat Spacing { get; } = new BindableFloat(10)
        {
            Precision = 1,
            MinValue = 0,
            MaxValue = 100,
        };

        [SettingSource("Opacity", "The receptor opacity.")]
        public BindableFloat Opacity { get; } = new BindableFloat(1)
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 1
        };

        [Resolved]
        private DrawableManiaRuleset drawableRuleset { get; set; } = null!;

        public ManiaTouchInputArea()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            RelativeSizeAxes = Axes.Both;
            Height = 0.5f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            List<Drawable> receptorGridContent = new List<Drawable>();
            List<Dimension> receptorGridDimensions = new List<Dimension>();

            bool first = true;

            foreach (var stage in drawableRuleset.Playfield.Stages)
            {
                foreach (var column in stage.Columns)
                {
                    if (!first)
                    {
                        receptorGridContent.Add(new Gutter { Spacing = { BindTarget = Spacing } });
                        receptorGridDimensions.Add(new Dimension(GridSizeMode.AutoSize));
                    }

                    receptorGridContent.Add(new InputReceptor { Action = { BindTarget = column.Action } });
                    receptorGridDimensions.Add(new Dimension());

                    first = false;
                }
            }

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[] { receptorGridContent.ToArray() },
                ColumnDimensions = receptorGridDimensions.ToArray()
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Opacity.BindValueChanged(o => Alpha = o.NewValue, true);
        }

        public bool UsesFixedAnchor { get; set; }

        public partial class InputReceptor : CompositeDrawable
        {
            public readonly IBindable<ManiaAction> Action = new Bindable<ManiaAction>();

            private readonly Box highlightOverlay;

            [Resolved]
            private ManiaInputManager? inputManager { get; set; }

            private bool isPressed;

            public InputReceptor()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.15f,
                            },
                            highlightOverlay = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                Blending = BlendingParameters.Additive,
                            }
                        }
                    }
                };
            }

            protected override bool OnTouchDown(TouchDownEvent e)
            {
                updateButton(true);
                return true;
            }

            protected override void OnTouchUp(TouchUpEvent e)
            {
                updateButton(false);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                updateButton(true);
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                updateButton(false);
            }

            private void updateButton(bool press)
            {
                if (press == isPressed)
                    return;

                isPressed = press;

                if (press)
                {
                    inputManager?.KeyBindingContainer?.TriggerPressed(Action.Value);
                    highlightOverlay.FadeTo(0.1f, 80, Easing.OutQuint);
                }
                else
                {
                    inputManager?.KeyBindingContainer?.TriggerReleased(Action.Value);
                    highlightOverlay.FadeTo(0, 400, Easing.OutQuint);
                }
            }
        }

        private partial class Gutter : Drawable
        {
            public readonly IBindable<float> Spacing = new Bindable<float>();

            public Gutter()
            {
                Spacing.BindValueChanged(s => Size = new Vector2(s.NewValue));
            }
        }
    }
}
