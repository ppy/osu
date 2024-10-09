// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class PathControlPointsCommandProxy : CommandProxy, IList<PathControlPointCommandProxy>
    {
        public PathControlPointsCommandProxy(EditorCommandHandler? commandHandler, BindableList<PathControlPoint> controlPoints)
            : base(commandHandler)
        {
            ControlPoints = controlPoints;
        }

        public readonly BindableList<PathControlPoint> ControlPoints;

        public int IndexOf(PathControlPointCommandProxy item)
        {
            return ControlPoints.IndexOf(item.ControlPoint);
        }

        public void Insert(int index, PathControlPointCommandProxy item) => Insert(index, item.ControlPoint);

        public void Insert(int index, PathControlPoint controlPoint) => Submit(new AddControlPointCommand(ControlPoints, index, controlPoint));

        public void RemoveAt(int index) => Submit(new RemoveControlPointCommand(ControlPoints, index));

        public PathControlPointCommandProxy this[int index]
        {
            get => new PathControlPointCommandProxy(CommandHandler, ControlPoints[index]);
            set => Submit(new AddControlPointCommand(ControlPoints, index, value.ControlPoint));
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = 0; i < count; i++)
                Submit(new RemoveControlPointCommand(ControlPoints, index));
        }

        public void Add(PathControlPointCommandProxy item) => Add(item.ControlPoint);

        public void Add(PathControlPoint controlPoint) => Submit(new AddControlPointCommand(ControlPoints, ControlPoints.Count, controlPoint));

        public void Clear()
        {
            while (ControlPoints.Count > 0)
                Remove(ControlPoints[0]);
        }

        public bool Contains(PathControlPointCommandProxy item)
        {
            return ControlPoints.Any(c => c.Equals(item.ControlPoint));
        }

        public void CopyTo(PathControlPointCommandProxy[] array, int arrayIndex)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
                array[arrayIndex + i] = new PathControlPointCommandProxy(CommandHandler, ControlPoints[i]);
        }

        public bool Remove(PathControlPointCommandProxy item) => Remove(item.ControlPoint);

        public bool Remove(PathControlPoint controlPoint)
        {
            if (!ControlPoints.Contains(controlPoint))
                return false;

            Submit(new RemoveControlPointCommand(ControlPoints, controlPoint));
            return true;
        }

        public int Count => ControlPoints.Count;

        public bool IsReadOnly => ControlPoints.IsReadOnly;

        public IEnumerator<PathControlPointCommandProxy> GetEnumerator() => new PathControlPointsCommandProxyEnumerator(CommandHandler, ControlPoints.GetEnumerator());

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly struct PathControlPointsCommandProxyEnumerator : IEnumerator<PathControlPointCommandProxy>
        {
            public PathControlPointsCommandProxyEnumerator(
                EditorCommandHandler? commandHandler,
                IEnumerator<PathControlPoint> enumerator
            )
            {
                this.commandHandler = commandHandler;
                this.enumerator = enumerator;
            }

            private readonly EditorCommandHandler? commandHandler;

            private readonly IEnumerator<PathControlPoint> enumerator;

            public bool MoveNext() => enumerator.MoveNext();

            public void Reset() => enumerator.Reset();

            public PathControlPointCommandProxy Current => new PathControlPointCommandProxy(commandHandler, enumerator.Current);

            object IEnumerator.Current => Current;

            public void Dispose() => enumerator.Dispose();
        }
    }
}
