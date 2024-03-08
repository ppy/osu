// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardCommandGroup
    {
        public SortedList<StoryboardCommand<float>> X = new SortedList<StoryboardCommand<float>>();
        public SortedList<StoryboardCommand<float>> Y = new SortedList<StoryboardCommand<float>>();
        public SortedList<StoryboardCommand<float>> Scale = new SortedList<StoryboardCommand<float>>();
        public SortedList<StoryboardCommand<Vector2>> VectorScale = new SortedList<StoryboardCommand<Vector2>>();
        public SortedList<StoryboardCommand<float>> Rotation = new SortedList<StoryboardCommand<float>>();
        public SortedList<StoryboardCommand<Color4>> Colour = new SortedList<StoryboardCommand<Color4>>();
        public SortedList<StoryboardCommand<float>> Alpha = new SortedList<StoryboardCommand<float>>();
        public SortedList<StoryboardCommand<BlendingParameters>> BlendingParameters = new SortedList<StoryboardCommand<BlendingParameters>>();
        public SortedList<StoryboardCommand<bool>> FlipH = new SortedList<StoryboardCommand<bool>>();
        public SortedList<StoryboardCommand<bool>> FlipV = new SortedList<StoryboardCommand<bool>>();

        public IReadOnlyList<IStoryboardCommand> AllCommands => allCommands;

        private readonly List<IStoryboardCommand> allCommands = new List<IStoryboardCommand>();

        /// <summary>
        /// Returns the earliest start time of the commands added to this group.
        /// </summary>
        [JsonIgnore]
        public double StartTime { get; private set; } = double.MaxValue;

        /// <summary>
        /// Returns the latest end time of the commands added to this group.
        /// </summary>
        [JsonIgnore]
        public double EndTime { get; private set; } = double.MinValue;

        [JsonIgnore]
        public double Duration => EndTime - StartTime;

        [JsonIgnore]
        public bool HasCommands { get; private set; }

        public void AddX(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(X, new StoryboardXCommand(easing, startTime, endTime, startValue, endValue));

        public void AddY(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(Y, new StoryboardYCommand(easing, startTime, endTime, startValue, endValue));

        public void AddScale(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(Scale, new StoryboardScaleCommand(easing, startTime, endTime, startValue, endValue));

        public void AddVectorScale(Easing easing, double startTime, double endTime, Vector2 startValue, Vector2 endValue)
            => AddCommand(VectorScale, new StoryboardVectorScaleCommand(easing, startTime, endTime, startValue, endValue));

        public void AddRotation(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(Rotation, new StoryboardRotationCommand(easing, startTime, endTime, startValue, endValue));

        public void AddColour(Easing easing, double startTime, double endTime, Color4 startValue, Color4 endValue)
            => AddCommand(Colour, new StoryboardColourCommand(easing, startTime, endTime, startValue, endValue));

        public void AddAlpha(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(Alpha, new StoryboardAlphaCommand(easing, startTime, endTime, startValue, endValue));

        public void AddBlendingParameters(Easing easing, double startTime, double endTime, BlendingParameters startValue, BlendingParameters endValue)
            => AddCommand(BlendingParameters, new StoryboardBlendingParametersCommand(easing, startTime, endTime, startValue, endValue));

        public void AddFlipH(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
            => AddCommand(FlipH, new StoryboardFlipHCommand(easing, startTime, endTime, startValue, endValue));

        public void AddFlipV(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
            => AddCommand(FlipV, new StoryboardFlipVCommand(easing, startTime, endTime, startValue, endValue));

        /// <summary>
        /// Adds the given storyboard <paramref name="command"/> to the target <paramref name="list"/>.
        /// Can be overriden to apply custom effects to the given command before adding it to the list (e.g. looping or time offsets).
        /// </summary>
        /// <typeparam name="T">The value type of the target property affected by this storyboard command.</typeparam>
        protected virtual void AddCommand<T>(ICollection<StoryboardCommand<T>> list, StoryboardCommand<T> command)
        {
            list.Add(command);
            allCommands.Add(command);
            HasCommands = true;

            if (command.StartTime < StartTime)
                StartTime = command.StartTime;

            if (command.EndTime > EndTime)
                EndTime = command.EndTime;
        }
    }
}
