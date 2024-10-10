// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Commands.Proxies
{
    public interface ICommandProxy<T>
    {
        EditorCommandHandler? CommandHandler { get; init; }
        T Target { get; init; }
        void Submit(IEditorCommand command);
    }

    public readonly struct CommandProxy<T> : ICommandProxy<T>
    {
        public CommandProxy(EditorCommandHandler? commandHandler, T target)
        {
            CommandHandler = commandHandler;
            Target = target;
        }

        public EditorCommandHandler? CommandHandler { get; init; }
        public T Target { get; init; }
        public void Submit(IEditorCommand command) => CommandHandler.SafeSubmit(command);
    }
}
