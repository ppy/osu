using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Settings
{
    public partial class LayoutSettingsSubsection : CompositeDrawable
    {
        private readonly Bindable<VisualizerLayout> layoutBindable = new Bindable<VisualizerLayout>();

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            config.BindWith(SandboxRulesetSetting.VisualizerLayout, layoutBindable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            layoutBindable.BindValueChanged(updateSubsection, true);
        }

        private void updateSubsection(ValueChangedEvent<VisualizerLayout> layout)
        {
            Subsection s;

            switch (layout.NewValue)
            {
                default:
                case VisualizerLayout.TypeA:
                    s = new TypeASubsection();
                    break;

                case VisualizerLayout.TypeB:
                    s = new TypeBSubsection();
                    break;

                case VisualizerLayout.Empty:
                    s = new EmptySubsection();
                    break;
            }

            loadSubsection(s);
        }

        private void loadSubsection(Subsection s)
        {
            InternalChild = s;
        }

        private abstract partial class Subsection : FillFlowContainer
        {
            public Subsection()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(0, 10);
            }
        }

        private partial class TypeASubsection : Subsection
        {
            [BackgroundDependencyLoader]
            private void load(SandboxRulesetConfigManager config)
            {
                AddRange(new Drawable[]
                {
                    new SettingsSlider<int>
                    {
                        LabelText = "Radius",
                        KeyboardStep = 1,
                        Current = config.GetBindable<int>(SandboxRulesetSetting.Radius)
                    },
                    new SettingsEnumDropdown<CircularBarType>
                    {
                        LabelText = "Bar type",
                        Current = config.GetBindable<CircularBarType>(SandboxRulesetSetting.CircularBarType)
                    },
                    new SettingsCheckbox
                    {
                        LabelText = "Symmetry",
                        Current = config.GetBindable<bool>(SandboxRulesetSetting.Symmetry)
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Visualizer count",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.VisualizerAmount),
                        KeyboardStep = 1,
                        TransferValueOnCommit = true
                    },
                    new SettingsSlider<double>
                    {
                        LabelText = "Bar width",
                        Current = config.GetBindable<double>(SandboxRulesetSetting.BarWidthA),
                        KeyboardStep = 0.1f
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Total bar count",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.BarsPerVisual),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Decay",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.DecayA),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Height Multiplier",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.MultiplierA),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Smoothness",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.SmoothnessA),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Rotation",
                        KeyboardStep = 1,
                        Current = config.GetBindable<int>(SandboxRulesetSetting.Rotation)
                    },
                    new ColourPickerDropdown("Visualizer colour", SandboxRulesetSetting.TypeAColour),
                    new ColourPickerDropdown("Progress bar colour", SandboxRulesetSetting.TypeAProgressColour),
                    new ColourPickerDropdown("Title colour", SandboxRulesetSetting.TypeATextColour)
                });
            }
        }

        private partial class TypeBSubsection : Subsection
        {
            [BackgroundDependencyLoader]
            private void load(SandboxRulesetConfigManager config)
            {
                AddRange(new Drawable[]
                {
                    new SettingsEnumDropdown<LinearBarType>
                    {
                        LabelText = "Bar type",
                        Current = config.GetBindable<LinearBarType>(SandboxRulesetSetting.LinearBarType)
                    },
                    new SettingsSlider<double>
                    {
                        LabelText = "Bar width",
                        Current = config.GetBindable<double>(SandboxRulesetSetting.BarWidthB),
                        KeyboardStep = 0.1f
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Total bar count",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.BarCountB),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Decay",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.DecayB),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Height Multiplier",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.MultiplierB),
                        KeyboardStep = 1
                    },
                    new SettingsSlider<int>
                    {
                        LabelText = "Smoothness",
                        Current = config.GetBindable<int>(SandboxRulesetSetting.SmoothnessB),
                        KeyboardStep = 1
                    },
                    new ColourPickerDropdown("Visualizer colour", SandboxRulesetSetting.TypeBColour),
                    new ColourPickerDropdown("Progress bar colour", SandboxRulesetSetting.TypeBProgressColour),
                    new ColourPickerDropdown("Title colour", SandboxRulesetSetting.TypeBTextColour)
                });
            }
        }

        private partial class EmptySubsection : Subsection
        {
        }
    }
}
