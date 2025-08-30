// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    internal partial class SkinSettingsToolbox : EditorSidebarSection
    {
        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        protected override Container<Drawable> Content { get; }

        private readonly Drawable component;

        /// <summary>
        /// Default setting for all skin HUD components
        /// </summary>
        private BindableFloat alpha { get; } = new BindableFloat(1f)
        {
            MinValue = 0.05f,
            MaxValue = 1f,
            Precision = 0.01f,
        };

        public SkinSettingsToolbox(Drawable component)
            : base(SkinEditorStrings.Settings(component.GetType().Name))
        {
            this.component = component;
            alpha.Value = this.component.Alpha;

            base.Content.Add(Content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var controls = component.CreateSettingsControls().ToArray().Append(
                new SettingsSlider<float>
                {
                    LabelText = "Alpha",
                    TooltipText = "Object visibility",
                    Current = alpha,
                    KeyboardStep = 0.1f,
                });

            Content.AddRange(controls);

            // track any changes to update undo states.
            foreach (var c in controls.OfType<ISettingsItem>())
            {
                // TODO: SettingChanged is called too often for cases like SettingsTextBox and SettingsSlider.
                // We will want to expose a SettingCommitted or similar to make this work better.
                c.SettingChanged += () => changeHandler?.SaveState();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            alpha.BindValueChanged(a => component.Alpha = a.NewValue);
        }
    }
}
