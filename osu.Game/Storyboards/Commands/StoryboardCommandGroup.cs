// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardCommandGroup
    {
        private readonly SortedList<StoryboardCommand<float>> x = new SortedList<StoryboardCommand<float>>();

        public IReadOnlyList<StoryboardCommand<float>> X => x;

        private readonly SortedList<StoryboardCommand<float>> y = new SortedList<StoryboardCommand<float>>();

        public IReadOnlyList<StoryboardCommand<float>> Y => y;

        private readonly SortedList<StoryboardCommand<float>> scale = new SortedList<StoryboardCommand<float>>();

        public IReadOnlyList<StoryboardCommand<float>> Scale => scale;

        private readonly SortedList<StoryboardCommand<Vector2>> vectorScale = new SortedList<StoryboardCommand<Vector2>>();

        public IReadOnlyList<StoryboardCommand<Vector2>> VectorScale => vectorScale;

        private readonly SortedList<StoryboardCommand<float>> rotation = new SortedList<StoryboardCommand<float>>();

        public IReadOnlyList<StoryboardCommand<float>> Rotation => rotation;

        private readonly SortedList<StoryboardCommand<Color4>> colour = new SortedList<StoryboardCommand<Color4>>();

        public IReadOnlyList<StoryboardCommand<Color4>> Colour => colour;

        private readonly SortedList<StoryboardCommand<float>> alpha = new SortedList<StoryboardCommand<float>>();

        public IReadOnlyList<StoryboardCommand<float>> Alpha => alpha;

        private readonly SortedList<StoryboardCommand<BlendingParameters>> blendingParameters = new SortedList<StoryboardCommand<BlendingParameters>>();

        public IReadOnlyList<StoryboardCommand<BlendingParameters>> BlendingParameters => blendingParameters;

        private readonly SortedList<StoryboardCommand<bool>> flipH = new SortedList<StoryboardCommand<bool>>();

        public IReadOnlyList<StoryboardCommand<bool>> FlipH => flipH;

        private readonly SortedList<StoryboardCommand<bool>> flipV = new SortedList<StoryboardCommand<bool>>();

        public IReadOnlyList<StoryboardCommand<bool>> FlipV => flipV;

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

        private readonly IReadOnlyList<IStoryboardCommand>[] lists;

        public IEnumerable<IStoryboardCommand> AllCommands => lists.SelectMany(g => g);

        public StoryboardCommandGroup()
        {
            lists = new IReadOnlyList<IStoryboardCommand>[] { X, Y, Scale, VectorScale, Rotation, Colour, Alpha, BlendingParameters, FlipH, FlipV };
        }

        public void AddX(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(x, new StoryboardXCommand(easing, startTime, endTime, startValue, endValue));

        public void AddY(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(y, new StoryboardYCommand(easing, startTime, endTime, startValue, endValue));

        public void AddScale(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(scale, new StoryboardScaleCommand(easing, startTime, endTime, startValue, endValue));

        public void AddVectorScale(Easing easing, double startTime, double endTime, Vector2 startValue, Vector2 endValue)
            => AddCommand(vectorScale, new StoryboardVectorScaleCommand(easing, startTime, endTime, startValue, endValue));

        public void AddRotation(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(rotation, new StoryboardRotationCommand(easing, startTime, endTime, startValue, endValue));

        public void AddColour(Easing easing, double startTime, double endTime, Color4 startValue, Color4 endValue)
            => AddCommand(colour, new StoryboardColourCommand(easing, startTime, endTime, startValue, endValue));

        public void AddAlpha(Easing easing, double startTime, double endTime, float startValue, float endValue)
            => AddCommand(alpha, new StoryboardAlphaCommand(easing, startTime, endTime, startValue, endValue));

        public void AddBlendingParameters(Easing easing, double startTime, double endTime, BlendingParameters startValue, BlendingParameters endValue)
            => AddCommand(blendingParameters, new StoryboardBlendingParametersCommand(easing, startTime, endTime, startValue, endValue));

        public void AddFlipH(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
            => AddCommand(flipH, new StoryboardFlipHCommand(easing, startTime, endTime, startValue, endValue));

        public void AddFlipV(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
            => AddCommand(flipV, new StoryboardFlipVCommand(easing, startTime, endTime, startValue, endValue));

        /// <summary>
        /// Adds the given storyboard <paramref name="command"/> to the target <paramref name="list"/>.
        /// Can be overriden to apply custom effects to the given command before adding it to the list (e.g. looping or time offsets).
        /// </summary>
        /// <typeparam name="T">The value type of the target property affected by this storyboard command.</typeparam>
        protected virtual void AddCommand<T>(ICollection<StoryboardCommand<T>> list, StoryboardCommand<T> command)
        {
            list.Add(command);
            HasCommands = true;

            if (command.StartTime < StartTime)
                StartTime = command.StartTime;

            if (command.EndTime > EndTime)
                EndTime = command.EndTime;
        }
    }
}
