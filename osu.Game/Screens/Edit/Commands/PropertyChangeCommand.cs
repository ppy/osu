// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Commands
{
    public abstract class PropertyChangeCommand<TTarget, TProperty> : IMergeableCommand where TTarget : class
    {
        protected abstract TProperty ReadValue(TTarget target);

        protected abstract void WriteValue(TTarget target, TProperty value);

        protected abstract PropertyChangeCommand<TTarget, TProperty> CreateInstance(TTarget target, TProperty value);

        public readonly TTarget Target;

        public readonly TProperty Value;

        protected PropertyChangeCommand(TTarget target, TProperty value)
        {
            Target = target;
            Value = value;
        }

        public void Apply() => WriteValue(Target, Value);

        public IEditorCommand CreateUndo() => CreateInstance(Target, Value);

        public IMergeableCommand? MergeWith(IEditorCommand previous)
        {
            if (previous is PropertyChangeCommand<TTarget, TProperty> command && command.Target == Target)
                return command;

            return null;
        }
    }
}
