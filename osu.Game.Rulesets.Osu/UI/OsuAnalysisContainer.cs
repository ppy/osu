// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuAnalysisContainer : AnalysisContainer
    {
        public Bindable<bool> HitMarkerEnabled = new BindableBool();
        public Bindable<bool> AimMarkersEnabled = new BindableBool();
        public Bindable<bool> AimLinesEnabled = new BindableBool();

        private HitMarkersContainer hitMarkersContainer;
        private AimMarkersContainer aimMarkersContainer;
        private AimLinesContainer aimLinesContainer;

        public OsuAnalysisContainer(Replay replay)
            : base(replay)
        {
            InternalChildren = new Drawable[]
            {
                hitMarkersContainer = new HitMarkersContainer(),
                aimMarkersContainer = new AimMarkersContainer() { Depth = float.MinValue },
                aimLinesContainer = new AimLinesContainer() { Depth = float.MaxValue }
            };

            HitMarkerEnabled.ValueChanged += e => hitMarkersContainer.FadeTo(e.NewValue ? 1 : 0);
            AimMarkersEnabled.ValueChanged += e => aimMarkersContainer.FadeTo(e.NewValue ? 1 : 0);
            AimLinesEnabled.ValueChanged += e => aimLinesContainer.FadeTo(e.NewValue ? 1 : 0);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            hitMarkersContainer.Hide();
            aimMarkersContainer.Hide();
            aimLinesContainer.Hide();

            bool leftHeld = false;
            bool rightHeld = false;
            foreach (var frame in Replay.Frames)
            {
                var osuFrame = (OsuReplayFrame)frame;

                aimMarkersContainer.Add(new AimPointEntry(osuFrame.Time, osuFrame.Position));
                aimLinesContainer.Add(new AimPointEntry(osuFrame.Time, osuFrame.Position));

                bool leftButton = osuFrame.Actions.Contains(OsuAction.LeftButton);
                bool rightButton = osuFrame.Actions.Contains(OsuAction.RightButton);

                if (leftHeld && !leftButton)
                    leftHeld = false;
                else if (!leftHeld && leftButton)
                {
                    hitMarkersContainer.Add(new HitMarkerEntry(osuFrame.Time, osuFrame.Position, true));
                    leftHeld = true;
                }

                if (rightHeld && !rightButton)
                    rightHeld = false;
                else if (!rightHeld && rightButton)
                {
                    hitMarkersContainer.Add(new HitMarkerEntry(osuFrame.Time, osuFrame.Position, false));
                    rightHeld = true;
                }
            }
        }

        private partial class HitMarkersContainer : PooledDrawableWithLifetimeContainer<HitMarkerEntry, HitMarkerDrawable>
        {
            private readonly HitMarkerPool leftPool;
            private readonly HitMarkerPool rightPool;

            public HitMarkersContainer()
            {
                AddInternal(leftPool = new HitMarkerPool(OsuSkinComponents.HitMarkerLeft, OsuAction.LeftButton, 15));
                AddInternal(rightPool = new HitMarkerPool(OsuSkinComponents.HitMarkerRight, OsuAction.RightButton, 15));
            }

            protected override HitMarkerDrawable GetDrawable(HitMarkerEntry entry) => (entry.IsLeftMarker ? leftPool : rightPool).Get(d => d.Apply(entry));
        }

        private partial class AimMarkersContainer : PooledDrawableWithLifetimeContainer<AimPointEntry, HitMarkerDrawable>
        {
            private readonly HitMarkerPool pool;

            public AimMarkersContainer()
            {
                AddInternal(pool = new HitMarkerPool(OsuSkinComponents.AimMarker, null, 80));
            }

            protected override HitMarkerDrawable GetDrawable(AimPointEntry entry) => pool.Get(d => d.Apply(entry));
        }

        private partial class AimLinesContainer : Path
        {
            private LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();
            private SortedSet<AimPointEntry> aliveEntries = new SortedSet<AimPointEntry>(new AimLinePointComparator());

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

        private partial class HitMarkerDrawable : PoolableDrawableWithLifetime<AimPointEntry>
        {
            /// <summary>
            /// This constructor only exists to meet the <c>new()</c> type constraint of <see cref="DrawablePool{T}"/>.
            /// </summary>
            public HitMarkerDrawable()
            {
            }

            public HitMarkerDrawable(OsuSkinComponents component, OsuAction? action)
            {
                Origin = Anchor.Centre;
                InternalChild = new SkinnableDrawable(new OsuSkinComponentLookup(component), _ => new DefaultHitMarker(action));
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

        private partial class HitMarkerPool : DrawablePool<HitMarkerDrawable>
        {
            private readonly OsuSkinComponents component;
            private readonly OsuAction? action;

            public HitMarkerPool(OsuSkinComponents component, OsuAction? action, int initialSize)
                : base(initialSize)
            {
                this.component = component;
                this.action = action;
            }

            protected override HitMarkerDrawable CreateNewDrawable() => new HitMarkerDrawable(component, action);
        }

        private partial class AimPointEntry : LifetimeEntry
        {
            public Vector2 Position { get; }

            public AimPointEntry(double time, Vector2 position)
            {
                LifetimeStart = time;
                LifetimeEnd = time + 1_000;
                Position = position;
            }
        }

        private partial class HitMarkerEntry : AimPointEntry
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
