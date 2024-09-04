// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayAnalysisOverlay : CompositeDrawable
    {
        protected readonly HitMarkersContainer HitMarkers;
        protected readonly AimMarkersContainer AimMarkers;
        protected readonly AimLinesContainer AimLines;

        public ReplayAnalysisSettings Settings = null!;

        private readonly Replay replay;
        private readonly DrawableRuleset drawableRuleset;

        public ReplayAnalysisOverlay(Replay replay, DrawableRuleset drawableRuleset)
        {
            this.replay = replay;
            this.drawableRuleset = drawableRuleset;

            InternalChildren = new Drawable[]
            {
                HitMarkers = new HitMarkersContainer(),
                AimLines = new AimLinesContainer(),
                AimMarkers = new AimMarkersContainer(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(ReplayPlayer replayPlayer)
        {
            replayPlayer.AddSettings(Settings = new ReplayAnalysisSettings());

            LoadReplay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Settings.HitMarkersEnabled.BindValueChanged(e => toggleHitMarkers(e.NewValue), true);
            Settings.AimMarkersEnabled.BindValueChanged(e => toggleAimMarkers(e.NewValue), true);
            Settings.AimLinesEnabled.BindValueChanged(e => toggleAimLines(e.NewValue), true);
            Settings.CursorHideEnabled.BindValueChanged(e => toggleCursorHidden(e.NewValue), true);
        }

        private void toggleHitMarkers(bool value) => HitMarkers.FadeTo(value ? 1 : 0);

        private void toggleAimMarkers(bool value) => AimMarkers.FadeTo(value ? 1 : 0);

        private void toggleAimLines(bool value) => AimLines.FadeTo(value ? 1 : 0);

        private void toggleCursorHidden(bool value) => drawableRuleset.Playfield.Cursor.FadeTo(value ? 0 : 1);

        protected void LoadReplay()
        {
            bool leftHeld = false;
            bool rightHeld = false;

            foreach (var frame in replay.Frames)
            {
                var osuFrame = (OsuReplayFrame)frame;

                AimMarkers.Add(new AimPointEntry(osuFrame.Time, osuFrame.Position));
                AimLines.Add(new AimPointEntry(osuFrame.Time, osuFrame.Position));

                bool leftButton = osuFrame.Actions.Contains(OsuAction.LeftButton);
                bool rightButton = osuFrame.Actions.Contains(OsuAction.RightButton);

                if (leftHeld && !leftButton)
                    leftHeld = false;
                else if (!leftHeld && leftButton)
                {
                    HitMarkers.Add(new HitMarkerEntry(osuFrame.Time, osuFrame.Position, true));
                    leftHeld = true;
                }

                if (rightHeld && !rightButton)
                    rightHeld = false;
                else if (!rightHeld && rightButton)
                {
                    HitMarkers.Add(new HitMarkerEntry(osuFrame.Time, osuFrame.Position, false));
                    rightHeld = true;
                }
            }
        }

        protected partial class HitMarkersContainer : PooledDrawableWithLifetimeContainer<HitMarkerEntry, HitMarkerDrawable>
        {
            private readonly HitMarkerPool leftPool;
            private readonly HitMarkerPool rightPool;

            public HitMarkersContainer()
            {
                AddInternal(leftPool = new HitMarkerPool(OsuAction.LeftButton, 15));
                AddInternal(rightPool = new HitMarkerPool(OsuAction.RightButton, 15));
            }

            protected override HitMarkerDrawable GetDrawable(HitMarkerEntry entry) => (entry.IsLeftMarker ? leftPool : rightPool).Get(d => d.Apply(entry));
        }

        protected partial class AimMarkersContainer : PooledDrawableWithLifetimeContainer<AimPointEntry, HitMarkerDrawable>
        {
            private readonly HitMarkerPool pool;

            public AimMarkersContainer()
            {
                AddInternal(pool = new HitMarkerPool(null, 80));
            }

            protected override HitMarkerDrawable GetDrawable(AimPointEntry entry) => pool.Get(d => d.Apply(entry));
        }

        protected partial class AimLinesContainer : Path
        {
            private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();
            private readonly SortedSet<AimPointEntry> aliveEntries = new SortedSet<AimPointEntry>(new AimLinePointComparator());

            public AimLinesContainer()
            {
                lifetimeManager.EntryBecameAlive += entryBecameAlive;
                lifetimeManager.EntryBecameDead += entryBecameDead;

                PathRadius = 1f;
                Colour = new Color4(255, 255, 255, 127);
            }

            protected override void Update()
            {
                base.Update();

                lifetimeManager.Update(Time.Current);
            }

            public void Add(AimPointEntry entry) => lifetimeManager.AddEntry(entry);

            public void Clear() => lifetimeManager.ClearEntries();

            private void entryBecameAlive(LifetimeEntry entry)
            {
                aliveEntries.Add((AimPointEntry)entry);
                updateVertices();
            }

            private void entryBecameDead(LifetimeEntry entry)
            {
                aliveEntries.Remove((AimPointEntry)entry);
                updateVertices();
            }

            private void updateVertices()
            {
                ClearVertices();

                foreach (var entry in aliveEntries)
                {
                    AddVertex(entry.Position);
                }
            }

            private sealed class AimLinePointComparator : IComparer<AimPointEntry>
            {
                public int Compare(AimPointEntry? x, AimPointEntry? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    return x.LifetimeStart.CompareTo(y.LifetimeStart);
                }
            }
        }

        protected partial class HitMarkerDrawable : PoolableDrawableWithLifetime<AimPointEntry>
        {
            /// <summary>
            /// This constructor only exists to meet the <c>new()</c> type constraint of <see cref="DrawablePool{T}"/>.
            /// </summary>
            public HitMarkerDrawable()
            {
            }

            public HitMarkerDrawable(OsuAction? action)
            {
                Origin = Anchor.Centre;
                InternalChild = new HitMarker(action);
            }

            protected override void OnApply(AimPointEntry entry)
            {
                Position = entry.Position;

                using (BeginAbsoluteSequence(LifetimeStart))
                    Show();

                using (BeginAbsoluteSequence(LifetimeEnd - 200))
                    this.FadeOut(200);
            }
        }

        protected partial class HitMarkerPool : DrawablePool<HitMarkerDrawable>
        {
            private readonly OsuAction? action;

            public HitMarkerPool(OsuAction? action, int initialSize)
                : base(initialSize)
            {
                this.action = action;
            }

            protected override HitMarkerDrawable CreateNewDrawable() => new HitMarkerDrawable(action);
        }

        protected partial class AimPointEntry : LifetimeEntry
        {
            public Vector2 Position { get; }

            public AimPointEntry(double time, Vector2 position)
            {
                LifetimeStart = time;
                LifetimeEnd = time + 1_000;
                Position = position;
            }
        }

        protected partial class HitMarkerEntry : AimPointEntry
        {
            public bool IsLeftMarker { get; }

            public HitMarkerEntry(double lifetimeStart, Vector2 position, bool isLeftMarker)
                : base(lifetimeStart, position)
            {
                IsLeftMarker = isLeftMarker;
            }
        }
    }
}
