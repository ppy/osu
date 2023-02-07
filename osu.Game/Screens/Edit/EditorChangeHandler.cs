// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Tracks changes to the <see cref="Editor"/>.
    /// </summary>
    public abstract partial class EditorChangeHandler : TransactionalCommitComponent, IEditorChangeHandler
    {
        public readonly Bindable<bool> CanUndo = new Bindable<bool>();
        public readonly Bindable<bool> CanRedo = new Bindable<bool>();

        public event Action? OnStateChange;

        private readonly List<byte[]> savedStates = new List<byte[]>();

        private int currentState = -1;

        /// <summary>
        /// A SHA-2 hash representing the current visible editor state.
        /// </summary>
        public string CurrentStateHash
        {
            get
            {
                ensureStateSaved();

                using (var stream = new MemoryStream(savedStates[currentState]))
                    return stream.ComputeSHA2Hash();
            }
        }

        private bool isRestoring;

        public const int MAX_SAVED_STATES = 50;

        public override void BeginChange()
        {
            ensureStateSaved();

            base.BeginChange();
        }

        private void ensureStateSaved()
        {
            if (savedStates.Count == 0)
                SaveState();
        }

        protected override void UpdateState()
        {
            if (isRestoring)
                return;

            using (var stream = new MemoryStream())
            {
                WriteCurrentStateToStream(stream);
                byte[] newState = stream.ToArray();

                // if the previous state is binary equal we don't need to push a new one, unless this is the initial state.
                if (savedStates.Count > 0 && newState.SequenceEqual(savedStates[currentState])) return;

                if (currentState < savedStates.Count - 1)
                    savedStates.RemoveRange(currentState + 1, savedStates.Count - currentState - 1);

                if (savedStates.Count > MAX_SAVED_STATES)
                    savedStates.RemoveAt(0);

                savedStates.Add(newState);

                currentState = savedStates.Count - 1;

                OnStateChange?.Invoke();
                updateBindables();
            }
        }

        /// <summary>
        /// Restores an older or newer state.
        /// </summary>
        /// <param name="direction">The direction to restore in. If less than 0, an older state will be used. If greater than 0, a newer state will be used.</param>
        public void RestoreState(int direction)
        {
            if (TransactionActive)
                return;

            if (savedStates.Count == 0)
                return;

            int newState = Math.Clamp(currentState + direction, 0, savedStates.Count - 1);
            if (currentState == newState)
                return;

            isRestoring = true;

            ApplyStateChange(savedStates[currentState], savedStates[newState]);

            currentState = newState;

            isRestoring = false;

            OnStateChange?.Invoke();
            updateBindables();
        }

        /// <summary>
        /// Write a serialised copy of the currently tracked state to the provided stream.
        /// This will be stored as a state which can be restored in the future.
        /// </summary>
        /// <param name="stream">The stream which the state should be written to.</param>
        protected abstract void WriteCurrentStateToStream(MemoryStream stream);

        /// <summary>
        /// Given a previous and new state, apply any changes required to bring the current state in line with the new state.
        /// </summary>
        /// <param name="previousState">The previous (current before this call) serialised state.</param>
        /// <param name="newState">The new state to be applied.</param>
        protected abstract void ApplyStateChange(byte[] previousState, byte[] newState);

        private void updateBindables()
        {
            CanUndo.Value = savedStates.Count > 0 && currentState > 0;
            CanRedo.Value = currentState < savedStates.Count - 1;
        }
    }
}
