// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class SamplePointPiece : HitObjectPointPiece, IHasPopover
    {
        private readonly HitObject hitObject;

        private readonly Bindable<string> bank;
        private readonly BindableNumber<int> volume;

        public SamplePointPiece(HitObject hitObject)
            : base(hitObject.SampleControlPoint)
        {
            this.hitObject = hitObject;
            volume = hitObject.SampleControlPoint.SampleVolumeBindable.GetBoundCopy();
            bank = hitObject.SampleControlPoint.SampleBankBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            volume.BindValueChanged(volume => updateText());
            bank.BindValueChanged(bank => updateText(), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
        }

        private void updateText()
        {
            Label.Text = $"{bank.Value} {volume.Value}";
        }

        public Popover GetPopover() => new SampleEditPopover(hitObject);

        public class SampleEditPopover : OsuPopover
        {
            private readonly HitObject hitObject;
            private readonly SampleControlPoint point;

            private LabelledTextBox bank;
            private SliderWithTextBoxInput<int> volume;

            [Resolved(canBeNull: true)]
            private EditorBeatmap beatmap { get; set; }

            public SampleEditPopover(HitObject hitObject)
            {
                this.hitObject = hitObject;
                point = hitObject.SampleControlPoint;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Width = 200,
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            bank = new LabelledTextBox
                            {
                                Label = "Bank Name",
                            },
                            volume = new SliderWithTextBoxInput<int>("Volume")
                            {
                                Current = new SampleControlPoint().SampleVolumeBindable,
                            }
                        }
                    }
                };

                bank.Current = point.SampleBankBindable;
                bank.Current.BindValueChanged(_ => beatmap.Update(hitObject));

                volume.Current = point.SampleVolumeBindable;
                volume.Current.BindValueChanged(_ => beatmap.Update(hitObject));
            }
        }
    }
}
