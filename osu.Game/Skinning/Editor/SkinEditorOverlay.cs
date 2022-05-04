// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Screens;

namespace osu.Game.Skinning.Editor
{
    /// <summary>
    /// A container which handles loading a skin editor on user request for a specified target.
    /// This also handles the scaling / positioning adjustment of the target.
    /// </summary>
    public class SkinEditorOverlay : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        private readonly ScalingContainer scalingContainer;

        protected override bool BlockNonPositionalInput => true;

        [CanBeNull]
        private SkinEditor skinEditor;

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        private OsuScreen lastTargetScreen;

        public SkinEditorOverlay(ScalingContainer scalingContainer)
        {
            this.scalingContainer = scalingContainer;
            RelativeSizeAxes = Axes.Both;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (skinEditor?.State.Value != Visibility.Visible)
                        break;

                    Hide();
                    return true;
            }

            return false;
        }

        protected override void PopIn()
        {
            if (skinEditor != null)
            {
                skinEditor.Show();
                return;
            }

            var editor = new SkinEditor();

            editor.State.BindValueChanged(visibility => updateComponentVisibility());

            skinEditor = editor;

            LoadComponentAsync(editor, _ =>
            {
                if (editor != skinEditor)
                    return;

                AddInternal(editor);

                SetTarget(lastTargetScreen);
            });
        }

        protected override void PopOut() => skinEditor?.Hide();

        private void updateComponentVisibility()
        {
            Debug.Assert(skinEditor != null);

            const float toolbar_padding_requirement = 0.18f;

            if (skinEditor.State.Value == Visibility.Visible)
            {
                scalingContainer.SetCustomRect(new RectangleF(toolbar_padding_requirement, 0.2f, 0.8f - toolbar_padding_requirement, 0.7f), true);

                game?.Toolbar.Hide();
                game?.CloseAllOverlays();
            }
            else
            {
                scalingContainer.SetCustomRect(null);

                if (lastTargetScreen?.HideOverlaysOnEnter != true)
                    game?.Toolbar.Show();
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        /// <summary>
        /// Set a new target screen which will be used to find skinnable components.
        /// </summary>
        public void SetTarget(OsuScreen screen)
        {
            lastTargetScreen = screen;

            if (skinEditor == null) return;

            skinEditor.Save();

            // ensure the toolbar is re-hidden even if a new screen decides to try and show it.
            updateComponentVisibility();

            // AddOnce with parameter will ensure the newest target is loaded if there is any overlap.
            Scheduler.AddOnce(setTarget, screen);
        }

        private void setTarget(OsuScreen target)
        {
            Debug.Assert(skinEditor != null);

            if (!target.IsLoaded)
            {
                Scheduler.AddOnce(setTarget, target);
                return;
            }

            if (skinEditor.State.Value == Visibility.Visible)
                skinEditor.UpdateTargetScreen(target);
            else
            {
                skinEditor.Hide();
                skinEditor.Expire();
                skinEditor = null;
            }
        }
    }
}
