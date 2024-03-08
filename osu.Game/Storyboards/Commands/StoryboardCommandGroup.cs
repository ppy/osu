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

        public void AddX(double startTime, double endTime, float startValue, float endValue, Easing easing)
            => AddCommand(X, new StoryboardXCommand(startTime, endTime, startValue, endValue, easing));

        public void AddY(double startTime, double endTime, float startValue, float endValue, Easing easing)
            => AddCommand(Y, new StoryboardYCommand(startTime, endTime, startValue, endValue, easing));

        public void AddScale(double startTime, double endTime, float startValue, float endValue, Easing easing)
            => AddCommand(Scale, new StoryboardScaleCommand(startTime, endTime, startValue, endValue, easing));

        public void AddVectorScale(double startTime, double endTime, Vector2 startValue, Vector2 endValue, Easing easing)
            => AddCommand(VectorScale, new StoryboardVectorScaleCommand(startTime, endTime, startValue, endValue, easing));

        public void AddRotation(double startTime, double endTime, float startValue, float endValue, Easing easing)
            => AddCommand(Rotation, new StoryboardRotationCommand(startTime, endTime, startValue, endValue, easing));

        public void AddColour(double startTime, double endTime, Color4 startValue, Color4 endValue, Easing easing)
            => AddCommand(Colour, new StoryboardColourCommand(startTime, endTime, startValue, endValue, easing));

        public void AddAlpha(double startTime, double endTime, float startValue, float endValue, Easing easing)
            => AddCommand(Alpha, new StoryboardAlphaCommand(startTime, endTime, startValue, endValue, easing));

        public void AddBlendingParameters(double startTime, double endTime, BlendingParameters startValue, BlendingParameters endValue, Easing easing)
            => AddCommand(BlendingParameters, new StoryboardBlendingParametersCommand(startTime, endTime, startValue, endValue, easing));

        public void AddFlipH(double startTime, double endTime, bool startValue, bool endValue, Easing easing)
            => AddCommand(FlipH, new StoryboardFlipHCommand(startTime, endTime, startValue, endValue, easing));

        public void AddFlipV(double startTime, double endTime, bool startValue, bool endValue, Easing easing)
            => AddCommand(FlipV, new StoryboardFlipVCommand(startTime, endTime, startValue, endValue, easing));

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
