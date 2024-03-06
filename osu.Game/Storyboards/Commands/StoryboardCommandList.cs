// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Storyboards.Commands
{
    // public class StoryboardCommandList<T> : IStoryboardCommandList
    // {
    //     // todo: change to sorted list and avoid enumerable type on exposed properties?
    //     private readonly List<StoryboardCommand<T>> commands = new List<StoryboardCommand<T>>();
    //
    //     public IEnumerable<StoryboardCommand<T>> Commands => commands.OrderBy(c => c.StartTime);
    //
    //     IEnumerable<IStoryboardCommand> IStoryboardCommandList.Commands => Commands;
    //     public bool HasCommands => commands.Count > 0;
    //
    //     public double StartTime { get; private set; } = double.MaxValue;
    //     public double EndTime { get; private set; } = double.MinValue;
    //
    //     public T? StartValue { get; private set; }
    //     public T? EndValue { get; private set; }
    //
    //     public void Add(StoryboardCommand<T> command)
    //     {
    //         commands.Add(command);
    //
    //         if (command.StartTime < StartTime)
    //         {
    //             StartValue = command.StartValue;
    //             StartTime = command.StartTime;
    //         }
    //
    //         if (command.EndTime > EndTime)
    //         {
    //             EndValue = command.EndValue;
    //             EndTime = command.EndTime;
    //         }
    //     }
    //
    //     public override string ToString() => $"{commands.Count} command(s)";
    // }
}
