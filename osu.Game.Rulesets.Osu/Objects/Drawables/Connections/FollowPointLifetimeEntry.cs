// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPointLifetimeEntry : LifetimeEntry
    {
        public event Action Invalidated;
        public readonly OsuHitObject Start;

        public FollowPointLifetimeEntry(OsuHitObject start)
        {
            Start = start;
            LifetimeStart = Start.StartTime;

            bindEvents();
        }

        private OsuHitObject end;

        public OsuHitObject End
        {
            get => end;
            set
            {
                UnbindEvents();

                end = value;

                bindEvents();

                refreshLifetimes();
            }
        }

        private void bindEvents()
        {
            UnbindEvents();

            // Note: Positions are bound for instantaneous feedback from positional changes from the editor, before ApplyDefaults() is called on hitobjects.
            Start.DefaultsApplied += onDefaultsApplied;
            Start.PositionBindable.ValueChanged += onPositionChanged;

            if (End != null)
            {
                End.DefaultsApplied += onDefaultsApplied;
                End.PositionBindable.ValueChanged += onPositionChanged;
            }
        }

        public void UnbindEvents()
        {
            if (Start != null)
            {
                Start.DefaultsApplied -= onDefaultsApplied;
                Start.PositionBindable.ValueChanged -= onPositionChanged;
            }

            if (End != null)
            {
                End.DefaultsApplied -= onDefaultsApplied;
                End.PositionBindable.ValueChanged -= onPositionChanged;
            }
        }

        private void onDefaultsApplied(HitObject obj) => refreshLifetimes();

        private void onPositionChanged(ValueChangedEvent<Vector2> obj) => refreshLifetimes();

        private void refreshLifetimes()
        {
            if (End == null || End.NewCombo || Start is Spinner || End is Spinner)
            {
                LifetimeEnd = LifetimeStart;
                return;
            }

            Vector2 startPosition = Start.StackedEndPosition;
            Vector2 endPosition = End.StackedPosition;
            Vector2 distanceVector = endPosition - startPosition;

            // The lifetime start will match the fade-in time of the first follow point.
            float fraction = (int)(FollowPointConnection.SPACING * 1.5) / distanceVector.Length;
            FollowPointConnection.GetFadeTimes(Start, End, fraction, out var fadeInTime, out _);

            LifetimeStart = fadeInTime;
            LifetimeEnd = double.MaxValue; // This will be set by the connection.

            Invalidated?.Invoke();
        }
    }
}
