// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PreciseRotationPopover : OsuPopover
    {
        private readonly SelectionRotationHandler rotationHandler;

        private readonly OsuGridToolboxGroup gridToolbox;

        private readonly Bindable<PreciseRotationInfo> rotationInfo = new Bindable<PreciseRotationInfo>(new PreciseRotationInfo(0, EditorOrigin.GridCentre));

        private SliderWithTextBoxInput<float> angleInput = null!;
        private EditorRadioButtonCollection rotationOrigin = null!;

        private RadioButton gridCentreButton = null!;
        private RadioButton playfieldCentreButton = null!;
        private RadioButton selectionCentreButton = null!;

        private Bindable<EditorOrigin> configRotationOrigin = null!;

        public PreciseRotationPopover(SelectionRotationHandler rotationHandler, OsuGridToolboxGroup gridToolbox)
        {
            this.rotationHandler = rotationHandler;
            this.gridToolbox = gridToolbox;

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            configRotationOrigin = config.GetBindable<EditorOrigin>(OsuSetting.EditorRotationOrigin);

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
                        KeyboardStep = 1f,
                        Instantaneous = true
                    },
                    rotationOrigin = new EditorRadioButtonCollection
                    {
                        RelativeSizeAxes = Axes.X,
                        Items = new[]
                        {
                            gridCentreButton = new RadioButton("Grid centre",
                                () => rotationInfo.Value = rotationInfo.Value with { Origin = EditorOrigin.GridCentre },
                                () => new SpriteIcon { Icon = FontAwesome.Regular.PlusSquare }),
                            playfieldCentreButton = new RadioButton("Playfield centre",
                                () => rotationInfo.Value = rotationInfo.Value with { Origin = EditorOrigin.PlayfieldCentre },
                                () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                            selectionCentreButton = new RadioButton("Selection centre",
                                () => rotationInfo.Value = rotationInfo.Value with { Origin = EditorOrigin.SelectionCentre },
                                () => new SpriteIcon { Icon = FontAwesome.Solid.VectorSquare })
                        }
                    }
                }
            };
            selectionCentreButton.Selected.DisabledChanged += isDisabled =>
            {
                selectionCentreButton.TooltipText = isDisabled ? "Select more than one object to perform selection-based rotation." : string.Empty;
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => angleInput.TakeFocus());
            angleInput.Current.BindValueChanged(angle => rotationInfo.Value = rotationInfo.Value with { Degrees = angle.NewValue });

            rotationHandler.CanRotateAroundSelectionOrigin.BindValueChanged(e =>
            {
                selectionCentreButton.Selected.Disabled = !e.NewValue;
            }, true);

            bool didSelect = false;

            configRotationOrigin.BindValueChanged(val =>
            {
                switch (configRotationOrigin.Value)
                {
                    case EditorOrigin.GridCentre:
                        if (!gridCentreButton.Selected.Disabled)
                        {
                            gridCentreButton.Select();
                            didSelect = true;
                        }

                        break;

                    case EditorOrigin.PlayfieldCentre:
                        if (!playfieldCentreButton.Selected.Disabled)
                        {
                            playfieldCentreButton.Select();
                            didSelect = true;
                        }

                        break;

                    case EditorOrigin.SelectionCentre:
                        if (!selectionCentreButton.Selected.Disabled)
                        {
                            selectionCentreButton.Select();
                            didSelect = true;
                        }

                        break;
                }
            }, true);

            if (!didSelect)
                rotationOrigin.Items.First(b => !b.Selected.Disabled).Select();

            gridCentreButton.Selected.BindValueChanged(b =>
            {
                if (b.NewValue) configRotationOrigin.Value = EditorOrigin.GridCentre;
            });
            playfieldCentreButton.Selected.BindValueChanged(b =>
            {
                if (b.NewValue) configRotationOrigin.Value = EditorOrigin.PlayfieldCentre;
            });
            selectionCentreButton.Selected.BindValueChanged(b =>
            {
                if (b.NewValue) configRotationOrigin.Value = EditorOrigin.SelectionCentre;
            });

            rotationInfo.BindValueChanged(rotation =>
            {
                // can happen if the popover is dismissed by a keyboard key press while dragging UI controls
                if (!rotationHandler.OperationInProgress.Value)
                    return;

                rotationHandler.Update(rotation.NewValue.Degrees, getOriginPosition(rotation.NewValue));
            });
        }

        private Vector2? getOriginPosition(PreciseRotationInfo rotation) =>
            rotation.Origin switch
            {
                EditorOrigin.GridCentre => gridToolbox.StartPosition.Value,
                EditorOrigin.PlayfieldCentre => OsuPlayfield.BASE_SIZE / 2,
                EditorOrigin.SelectionCentre => null,
                _ => throw new ArgumentOutOfRangeException(nameof(rotation))
            };

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

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Select && !e.Repeat)
            {
                this.HidePopover();
                return true;
            }

            return base.OnPressed(e);
        }
    }

    public record PreciseRotationInfo(float Degrees, EditorOrigin Origin);
}
