// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;

namespace osu.Game.Skinning.Editor
{
    /// <summary>
    /// A container which handles loading a skin editor on user request for a specified target.
    /// This also handles the scaling / positioning adjustment of the target.
    /// </summary>
    public class SkinEditorOverlay : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        private readonly ScalingContainer target;

        [CanBeNull]
        private SkinEditor skinEditor;

        public const float VISIBLE_TARGET_SCALE = 0.8f;

        public SkinEditorOverlay(ScalingContainer target)
        {
            this.target = target;
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

                case GlobalAction.ToggleSkinEditor:
                    Toggle();
                    return true;
            }

            return false;
        }

        public void Toggle()
        {
            if (skinEditor == null)
                Show();
            else
                skinEditor.ToggleVisibility();
        }

        public override void Hide()
        {
            // base call intentionally omitted.
            skinEditor?.Hide();
        }

        public override void Show()
        {
            // base call intentionally omitted as we have custom behaviour.

            if (skinEditor != null)
            {
                skinEditor.Show();
                return;
            }

            var editor = new SkinEditor(target);
            editor.State.BindValueChanged(editorVisibilityChanged);

            skinEditor = editor;

            // Schedule ensures that if `Show` is called before this overlay is loaded,
            // it will not throw (LoadComponentAsync requires the load target to be in a loaded state).
            Schedule(() =>
            {
                if (editor != skinEditor)
                    return;

                LoadComponentAsync(editor, _ =>
                {
                    if (editor != skinEditor)
                        return;

                    AddInternal(editor);
                });
            });
        }

        private void editorVisibilityChanged(ValueChangedEvent<Visibility> visibility)
        {
            if (visibility.NewValue == Visibility.Visible)
            {
                target.SetCustomRect(new RectangleF(0.18f, 0.1f, VISIBLE_TARGET_SCALE, VISIBLE_TARGET_SCALE), true);
            }
            else
            {
                target.SetCustomRect(null);
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        /// <summary>
        /// Set a new target screen which will be used to find skinnable components.
        /// </summary>
        public void SetTarget(Screen screen)
        {
            if (skinEditor == null) return;

            skinEditor.Save();

            // AddOnce with parameter will ensure the newest target is loaded if there is any overlap.
            Scheduler.AddOnce(setTarget, screen);
        }

        private void setTarget(Screen target)
        {
            Debug.Assert(skinEditor != null);

            if (!target.IsCurrentScreen())
                return;

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
