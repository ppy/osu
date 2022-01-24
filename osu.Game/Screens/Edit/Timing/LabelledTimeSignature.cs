// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Timing
{
    public class LabelledTimeSignature : LabelledComponent<LabelledTimeSignature.TimeSignatureBox, TimeSignature>
    {
        public LabelledTimeSignature()
            : base(false)
        {
        }

        protected override TimeSignatureBox CreateComponent() => new TimeSignatureBox();

        public class TimeSignatureBox : CompositeDrawable, IHasCurrentValue<TimeSignature>
        {
            private readonly BindableWithCurrent<TimeSignature> current = new BindableWithCurrent<TimeSignature>(TimeSignature.SimpleQuadruple);

            public Bindable<TimeSignature> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            private OsuNumberBox numeratorBox;

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        numeratorBox = new OsuNumberBox
                        {
                            Width = 40,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            CornerRadius = CORNER_RADIUS,
                            CommitOnFocusLost = true
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding
                            {
                                Left = 5,
                                Right = CONTENT_PADDING_HORIZONTAL
                            },
                            Text = "/ 4",
                            Font = OsuFont.Default.With(size: 20)
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(_ => updateFromCurrent(), true);
                numeratorBox.OnCommit += (_, __) => updateFromNumeratorBox();
            }

            private void updateFromCurrent()
            {
                numeratorBox.Current.Value = Current.Value.Numerator.ToString();
            }

            private void updateFromNumeratorBox()
            {
                if (int.TryParse(numeratorBox.Current.Value, out int numerator) && numerator > 0)
                    Current.Value = new TimeSignature(numerator);
                else
                {
                    // trigger `Current` change to restore the numerator box's text to a valid value.
                    Current.TriggerChange();
                }
            }
        }
    }
}
