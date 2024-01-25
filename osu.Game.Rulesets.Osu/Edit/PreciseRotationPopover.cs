// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PreciseRotationPopover : OsuPopover
    {
        private readonly SelectionRotationHandler rotationHandler;

        private readonly Bindable<PreciseRotationInfo> rotationInfo = new Bindable<PreciseRotationInfo>(new PreciseRotationInfo(0, RotationOrigin.PlayfieldCentre));

        private SliderWithTextBoxInput<float> angleInput = null!;
        private EditorRadioButtonCollection rotationOrigin = null!;

        private RadioButton selectionCentreButton = null!;
        public PreciseRotationPopover(SelectionRotationHandler rotationHandler)
        {
            this.rotationHandler = rotationHandler;

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    angleInput = new SliderWithTextBoxInput<float>("Angle (degrees):")
                    {
                        Current = new BindableNumber<float>
                        {
                            MinValue = -360,
                            MaxValue = 360,
                            Precision = 1
                        },
                        Instantaneous = true
                    },
                    rotationOrigin = new EditorRadioButtonCollection
                    {
                        RelativeSizeAxes = Axes.X,
                        Items = new[]
                        {
                            new RadioButton("Playfield centre",
                                () => rotationInfo.Value = rotationInfo.Value with { Origin = RotationOrigin.PlayfieldCentre },
                                () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                            selectionCentreButton = new RadioButton("Selection centre",
                                () => rotationInfo.Value = rotationInfo.Value with { Origin = RotationOrigin.SelectionCentre },
                                () => new SpriteIcon { Icon = FontAwesome.Solid.VectorSquare })
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => angleInput.TakeFocus());
            angleInput.Current.BindValueChanged(angle => rotationInfo.Value = rotationInfo.Value with { Degrees = angle.NewValue });
            rotationOrigin.Items.First().Select();

            rotationHandler.CanRotateSelectionOrigin.BindValueChanged(e =>
            {
                selectionCentreButton.Selected.Disabled = !e.NewValue;
            }, true);

            rotationInfo.BindValueChanged(rotation =>
            {
                rotationHandler.Update(rotation.NewValue.Degrees, rotation.NewValue.Origin == RotationOrigin.PlayfieldCentre ? OsuPlayfield.BASE_SIZE / 2 : null);
            });
        }

        protected override void PopIn()
        {
            base.PopIn();
            rotationHandler.Begin();
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (IsLoaded)
                rotationHandler.Commit();
        }
    }

    public enum RotationOrigin
    {
        PlayfieldCentre,
        SelectionCentre
    }

    public record PreciseRotationInfo(float Degrees, RotationOrigin Origin);
}
