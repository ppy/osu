// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Storyboards
{
    public class CommandTimeline<T> : ICommandTimeline
    {
        private readonly List<TypedCommand> commands = new List<TypedCommand>();

        public IEnumerable<TypedCommand> Commands => commands.OrderBy(c => c.StartTime);

        public bool HasCommands => commands.Count > 0;

        public double StartTime { get; private set; } = double.MaxValue;
        public double EndTime { get; private set; } = double.MinValue;

        public T StartValue { get; private set; }
        public T EndValue { get; private set; }

        public void Add(Easing easing, double startTime, double endTime, T startValue, T endValue)
        {
            if (endTime < startTime)
                return;

            commands.Add(new TypedCommand { Easing = easing, StartTime = startTime, EndTime = endTime, StartValue = startValue, EndValue = endValue });

            if (startTime < StartTime)
            {
                StartValue = startValue;
                StartTime = startTime;
            }

            if (endTime > EndTime)
            {
                EndValue = endValue;
                EndTime = endTime;
            }
        }

        public override string ToString()
            => $"{commands.Count} command(s)";

        public class TypedCommand : ICommand
        {
            public Easing Easing { get; set; }
            public double StartTime { get; set; }
            public double EndTime { get; set; }
            public double Duration => EndTime - StartTime;

            public T StartValue;
            public T EndValue;

            public int CompareTo(ICommand other)
            {
                int result = StartTime.CompareTo(other.StartTime);
                if (result != 0) return result;

                return EndTime.CompareTo(other.EndTime);
            }

            public override string ToString()
                => $"{StartTime} -> {EndTime}, {StartValue} -> {EndValue} {Easing}";
        }
    }

    public interface ICommandTimeline
    {
        double StartTime { get; }
        double EndTime { get; }
        bool HasCommands { get; }
    }

    public interface ICommand : IComparable<ICommand>
    {
        Easing Easing { get; set; }
        double StartTime { get; set; }
        double EndTime { get; set; }
        double Duration { get; }
    }
}
