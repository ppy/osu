// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Rulesets.UI
{
    [Cached(typeof(IPooledHitObjectProvider))]
    [Cached(typeof(IPooledSampleProvider))]
    [Cached]
    public abstract partial class Playfield : CompositeDrawable, IPooledHitObjectProvider, IPooledSampleProvider
    {
        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> is judged.
        /// </summary>
        public event Action<DrawableHitObject, JudgementResult> NewResult;

        /// <summary>
        /// Invoked when a judgement result is reverted.
        /// </summary>
        public event Action<JudgementResult> RevertResult;

        /// <summary>
        /// The <see cref="DrawableHitObject"/> contained in this Playfield.
        /// </summary>
        public HitObjectContainer HitObjectContainer => hitObjectContainerLazy.Value;

        private readonly Lazy<HitObjectContainer> hitObjectContainerLazy;

        /// <summary>
        /// A function that converts gamefield coordinates to screen space.
        /// </summary>
        public Func<Vector2, Vector2> GamefieldToScreenSpace => HitObjectContainer.ToScreenSpace;

        /// <summary>
        /// A function that converts screen space coordinates to gamefield.
        /// </summary>
        public Func<Vector2, Vector2> ScreenSpaceToGamefield => HitObjectContainer.ToLocalSpace;

        /// <summary>
        /// All the <see cref="DrawableHitObject"/>s contained in this <see cref="Playfield"/> and all <see cref="NestedPlayfields"/>.
        /// </summary>
        public IEnumerable<DrawableHitObject> AllHitObjects
        {
            get
            {
                if (HitObjectContainer == null)
                    return Enumerable.Empty<DrawableHitObject>();

                var enumerable = HitObjectContainer.Objects;

                if (nestedPlayfields.Count != 0)
                    enumerable = enumerable.Concat(NestedPlayfields.SelectMany(p => p.AllHitObjects));

                return enumerable;
            }
        }

        /// <summary>
        /// All <see cref="Playfield"/>s nested inside this <see cref="Playfield"/>.
        /// </summary>
        public IEnumerable<Playfield> NestedPlayfields => nestedPlayfields;

        private readonly List<Playfield> nestedPlayfields = new List<Playfield>();

        /// <summary>
        /// Whether this <see cref="Playfield"/> is nested in another <see cref="Playfield"/>.
        /// </summary>
        public bool IsNested { get; private set; }

        /// <summary>
        /// Whether judgements should be displayed by this and and all nested <see cref="Playfield"/>s.
        /// </summary>
        public readonly BindableBool DisplayJudgements = new BindableBool(true);

        /// <summary>
        /// A screen space draw quad which resembles the edges of the playfield for skinning purposes.
        /// This will allow users / components to snap objects to the "edge" of the playfield.
        /// </summary>
        /// <remarks>
        /// Rulesets which reduce the visible area further than the full relative playfield space itself
        /// should retarget this to the ScreenSpaceDrawQuad of the appropriate container.
        /// </remarks>
        public virtual Quad SkinnableComponentScreenSpaceDrawQuad => ScreenSpaceDrawQuad;

        [Resolved(CanBeNull = true)]
        [CanBeNull]
        protected IReadOnlyList<Mod> Mods { get; private set; }

        private readonly HitObjectEntryManager entryManager = new HitObjectEntryManager();

        private readonly Stack<HitObjectLifetimeEntry> judgedEntries;

        /// <summary>
        /// Creates a new <see cref="Playfield"/>.
        /// </summary>
        protected Playfield()
        {
            RelativeSizeAxes = Axes.Both;

            hitObjectContainerLazy = new Lazy<HitObjectContainer>(() => CreateHitObjectContainer().With(h =>
            {
                h.NewResult += onNewResult;
                h.HitObjectUsageBegan += o => HitObjectUsageBegan?.Invoke(o);
                h.HitObjectUsageFinished += o => HitObjectUsageFinished?.Invoke(o);
            }));

            entryManager.OnEntryAdded += onEntryAdded;
            entryManager.OnEntryRemoved += onEntryRemoved;

            judgedEntries = new Stack<HitObjectLifetimeEntry>();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Cursor = CreateCursor();

            if (Cursor != null)
            {
                // initial showing of the cursor will be handed by MenuCursorContainer (via DrawableRuleset's IProvideCursor implementation).
                Cursor.Hide();

                AddInternal(Cursor);
            }
        }

        private void onNewDrawableHitObject(DrawableHitObject d)
        {
            d.OnNestedDrawableCreated += onNewDrawableHitObject;

            OnNewDrawableHitObject(d);

            Debug.Assert(!d.IsInitialized);
            d.IsInitialized = true;
        }

        /// <summary>
        /// Performs post-processing tasks (if any) after all DrawableHitObjects are loaded into this Playfield.
        /// </summary>
        public virtual void PostProcess() => NestedPlayfields.ForEach(p => p.PostProcess());

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to add.</param>
        public virtual void Add(DrawableHitObject h)
        {
            if (!h.IsInitialized)
                onNewDrawableHitObject(h);

            HitObjectContainer.Add(h);
            OnHitObjectAdded(h.HitObject);
        }

        /// <summary>
        /// Remove a DrawableHitObject from this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to remove.</param>
        public virtual bool Remove(DrawableHitObject h)
        {
            if (!HitObjectContainer.Remove(h))
                return false;

            OnHitObjectRemoved(h.HitObject);
            return false;
        }

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is added to this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject">The added <see cref="HitObject"/>.</param>
        protected virtual void OnHitObjectAdded(HitObject hitObject)
        {
            preloadSamples(hitObject);
        }

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is removed from this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject">The removed <see cref="HitObject"/>.</param>
        protected virtual void OnHitObjectRemoved(HitObject hitObject)
        {
        }

        /// <summary>
        /// Invoked before a new <see cref="DrawableHitObject"/> is added to this <see cref="Playfield"/>.
        /// It is invoked only once even if the drawable is pooled and used multiple times for different <see cref="HitObject"/>s.
        /// </summary>
        /// <remarks>
        /// This is also invoked for nested <see cref="DrawableHitObject"/>s.
        /// </remarks>
        protected virtual void OnNewDrawableHitObject(DrawableHitObject drawableHitObject)
        {
        }

        /// <summary>
        /// The cursor currently being used by this <see cref="Playfield"/>. May be null if no cursor is provided.
        /// </summary>
        [CanBeNull]
        public GameplayCursorContainer Cursor { get; private set; }

        /// <summary>
        /// Provide a cursor which is to be used for gameplay.
        /// </summary>
        /// <returns>The cursor, or null to show the menu cursor.</returns>
        protected virtual GameplayCursorContainer CreateCursor() => null;

        /// <summary>
        /// Registers a <see cref="Playfield"/> as a nested <see cref="Playfield"/>.
        /// This does not add the <see cref="Playfield"/> to the draw hierarchy.
        /// </summary>
        /// <param name="otherPlayfield">The <see cref="Playfield"/> to add.</param>
        protected void AddNested(Playfield otherPlayfield)
        {
            otherPlayfield.IsNested = true;

            otherPlayfield.DisplayJudgements.BindTo(DisplayJudgements);

            otherPlayfield.NewResult += (d, r) => NewResult?.Invoke(d, r);
            otherPlayfield.RevertResult += r => RevertResult?.Invoke(r);
            otherPlayfield.HitObjectUsageBegan += h => HitObjectUsageBegan?.Invoke(h);
            otherPlayfield.HitObjectUsageFinished += h => HitObjectUsageFinished?.Invoke(h);

            nestedPlayfields.Add(otherPlayfield);
        }

        private Mod[] mods;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            mods = Mods?.ToArray();

            // in the case a consumer forgets to add the HitObjectContainer, we will add it here.
            if (HitObjectContainer.Parent == null)
                AddInternal(HitObjectContainer);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsNested && mods != null)
            {
                foreach (Mod mod in mods)
                {
                    if (mod is IUpdatableByPlayfield updatable)
                        updatable.Update(this);
                }
            }

            // When rewinding, revert future judgements in the reverse order.
            while (judgedEntries.Count > 0)
            {
                var result = judgedEntries.Peek().Result;
                Debug.Assert(result?.RawTime != null);

                if (Time.Current >= result.RawTime.Value)
                    break;

                revertResult(judgedEntries.Pop());
            }
        }

        /// <summary>
        /// Creates the container that will be used to contain the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectContainer CreateHitObjectContainer() => new HitObjectContainer();

        /// <summary>
        /// Adds an analysis container to internal children for replays.
        /// </summary>
        /// <param name="analysisContainer"></param>
        public virtual void AddAnalysisContainer(AnalysisContainer analysisContainer) => AddInternal(analysisContainer);

        #region Pooling support

        private readonly Dictionary<Type, IDrawablePool> pools = new Dictionary<Type, IDrawablePool>();

        /// <summary>
        /// Adds a <see cref="HitObjectLifetimeEntry"/> for a pooled <see cref="HitObject"/> to this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject"></param>
        public virtual void Add(HitObject hitObject)
        {
            var entry = CreateLifetimeEntry(hitObject);
            entryManager.Add(entry, null);
        }

        private void preloadSamples(HitObject hitObject)
        {
            // prepare sample pools ahead of time so we're not initialising at runtime.
            foreach (var sample in hitObject.Samples)
                prepareSamplePool(sample);

            foreach (var sample in hitObject.AuxiliarySamples)
                prepareSamplePool(sample);

            foreach (var nestedObject in hitObject.NestedHitObjects)
                preloadSamples(nestedObject);
        }

        /// <summary>
        /// Removes a <see cref="HitObjectLifetimeEntry"/> for a pooled <see cref="HitObject"/> from this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject"></param>
        /// <returns>Whether the <see cref="HitObject"/> was successfully removed.</returns>
        public virtual bool Remove(HitObject hitObject)
        {
            if (entryManager.TryGet(hitObject, out var entry))
            {
                entryManager.Remove(entry);
                return true;
            }

            return nestedPlayfields.Any(p => p.Remove(hitObject));
        }

        private void onEntryAdded(HitObjectLifetimeEntry entry, [CanBeNull] HitObject parentHitObject)
        {
            if (parentHitObject != null) return;

            HitObjectContainer.Add(entry);
            OnHitObjectAdded(entry.HitObject);
        }

        private void onEntryRemoved(HitObjectLifetimeEntry entry, [CanBeNull] HitObject parentHitObject)
        {
            if (parentHitObject != null) return;

            HitObjectContainer.Remove(entry);
            OnHitObjectRemoved(entry.HitObject);
        }

        /// <summary>
        /// Creates the <see cref="HitObjectLifetimeEntry"/> for a given <see cref="HitObject"/>.
        /// </summary>
        /// <remarks>
        /// This may be overridden to provide custom lifetime control (e.g. via <see cref="HitObjectLifetimeEntry.InitialLifetimeOffset"/>.
        /// </remarks>
        /// <param name="hitObject">The <see cref="HitObject"/> to create the entry for.</param>
        /// <returns>The <see cref="HitObjectLifetimeEntry"/>.</returns>
        [NotNull]
        protected virtual HitObjectLifetimeEntry CreateLifetimeEntry([NotNull] HitObject hitObject) => new HitObjectLifetimeEntry(hitObject);

        /// <summary>
        /// Registers a default <see cref="DrawableHitObject"/> pool with this <see cref="DrawableRuleset"/> which is to be used whenever
        /// <see cref="DrawableHitObject"/> representations are requested for the given <typeparamref name="TObject"/> type.
        /// </summary>
        /// <param name="initialSize">The number of <see cref="DrawableHitObject"/>s to be initially stored in the pool.</param>
        /// <param name="maximumSize">
        /// The maximum number of <see cref="DrawableHitObject"/>s that can be stored in the pool.
        /// If this limit is exceeded, every subsequent <see cref="DrawableHitObject"/> will be created anew instead of being retrieved from the pool,
        /// until some of the existing <see cref="DrawableHitObject"/>s are returned to the pool.
        /// </param>
        /// <typeparam name="TObject">The <see cref="HitObject"/> type.</typeparam>
        /// <typeparam name="TDrawable">The <see cref="DrawableHitObject"/> receiver for <typeparamref name="TObject"/>s.</typeparam>
        public void RegisterPool<TObject, TDrawable>(int initialSize, int? maximumSize = null)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
            => RegisterPool<TObject, TDrawable>(new DrawablePool<TDrawable>(initialSize, maximumSize));

        /// <summary>
        /// Registers a custom <see cref="DrawableHitObject"/> pool with this <see cref="DrawableRuleset"/> which is to be used whenever
        /// <see cref="DrawableHitObject"/> representations are requested for the given <typeparamref name="TObject"/> type.
        /// </summary>
        /// <param name="pool">The <see cref="DrawablePool{T}"/> to register.</param>
        /// <typeparam name="TObject">The <see cref="HitObject"/> type.</typeparam>
        /// <typeparam name="TDrawable">The <see cref="DrawableHitObject"/> receiver for <typeparamref name="TObject"/>s.</typeparam>
        protected void RegisterPool<TObject, TDrawable>([NotNull] DrawablePool<TDrawable> pool)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
        {
            pools[typeof(TObject)] = pool;
            AddInternal(pool);
        }

        DrawableHitObject IPooledHitObjectProvider.GetPooledDrawableRepresentation(HitObject hitObject, DrawableHitObject parent)
        {
            var pool = prepareDrawableHitObjectPool(hitObject);

            return (DrawableHitObject)pool?.Get(d =>
            {
                var dho = (DrawableHitObject)d;

                if (!dho.IsInitialized)
                {
                    onNewDrawableHitObject(dho);

                    // If this is the first time this DHO is being used, then apply the DHO mods.
                    // This is done before Apply() so that the state is updated once when the hitobject is applied.
                    if (mods != null)
                    {
                        foreach (Mod mod in mods)
                        {
                            if (mod is IApplicableToDrawableHitObject applicable)
                                applicable.ApplyToDrawableHitObject(dho);
                        }
                    }
                }

                if (!entryManager.TryGet(hitObject, out var entry))
                {
                    entry = CreateLifetimeEntry(hitObject);
                    entryManager.Add(entry, parent?.HitObject);
                }

                dho.ParentHitObject = parent;
                dho.Apply(entry);
            });
        }

        private IDrawablePool prepareDrawableHitObjectPool(HitObject hitObject)
        {
            var lookupType = hitObject.GetType();

            IDrawablePool pool;

            // Tests may add derived hitobject instances for which pools don't exist. Try to find any applicable pool and dynamically assign the type if the pool exists.
            if (!pools.TryGetValue(lookupType, out pool))
            {
                foreach (var (t, p) in pools)
                {
                    if (!t.IsInstanceOfType(hitObject))
                        continue;

                    pools[lookupType] = pool = p;
                    break;
                }
            }

            return pool;
        }

        private readonly Dictionary<ISampleInfo, DrawablePool<PoolableSkinnableSample>> samplePools = new Dictionary<ISampleInfo, DrawablePool<PoolableSkinnableSample>>();

        public PoolableSkinnableSample GetPooledSample(ISampleInfo sampleInfo) => prepareSamplePool(sampleInfo).Get();

        private DrawablePool<PoolableSkinnableSample> prepareSamplePool(ISampleInfo sampleInfo)
        {
            if (samplePools.TryGetValue(sampleInfo, out var pool)) return pool;

            AddInternal(samplePools[sampleInfo] = pool = new DrawableSamplePool(sampleInfo, 1));

            return pool;
        }

        private partial class DrawableSamplePool : DrawablePool<PoolableSkinnableSample>
        {
            private readonly ISampleInfo sampleInfo;

            public DrawableSamplePool(ISampleInfo sampleInfo, int initialSize, int? maximumSize = null)
                : base(initialSize, maximumSize)
            {
                this.sampleInfo = sampleInfo;
            }

            protected override PoolableSkinnableSample CreateNewDrawable() => base.CreateNewDrawable().With(d => d.Apply(sampleInfo));
        }

        #endregion

        private void onNewResult(DrawableHitObject drawable, JudgementResult result)
        {
            Debug.Assert(result != null && drawable.Entry?.Result == result && result.RawTime != null);
            judgedEntries.Push(drawable.Entry.AsNonNull());

            NewResult?.Invoke(drawable, result);
        }

        private void revertResult(HitObjectLifetimeEntry entry)
        {
            var result = entry.Result;
            Debug.Assert(result != null);

            RevertResult?.Invoke(result);
            entry.OnRevertResult();

            result.Reset();
        }

        #region Editor logic

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> becomes used by a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// If this <see cref="HitObjectContainer"/> uses pooled objects, this represents the time when the <see cref="HitObject"/>s become alive.
        /// </remarks>
        internal event Action<HitObject> HitObjectUsageBegan;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> becomes unused by a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// If this <see cref="HitObjectContainer"/> uses pooled objects, this represents the time when the <see cref="HitObject"/>s become dead.
        /// </remarks>
        internal event Action<HitObject> HitObjectUsageFinished;

        /// <summary>
        /// Sets whether to keep a given <see cref="HitObject"/> always alive within this or any nested <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to set.</param>
        /// <param name="keepAlive">Whether to keep <paramref name="hitObject"/> always alive.</param>
        internal void SetKeepAlive(HitObject hitObject, bool keepAlive)
        {
            if (entryManager.TryGet(hitObject, out var entry))
            {
                entry.KeepAlive = keepAlive;
                return;
            }

            foreach (var p in nestedPlayfields)
                p.SetKeepAlive(hitObject, keepAlive);
        }

        /// <summary>
        /// Keeps all <see cref="HitObject"/>s alive within this and all nested <see cref="Playfield"/>s.
        /// </summary>
        internal void KeepAllAlive()
        {
            foreach (var entry in entryManager.AllEntries)
                entry.KeepAlive = true;

            foreach (var p in nestedPlayfields)
                p.KeepAllAlive();
        }

        /// <summary>
        /// The amount of time prior to the current time within which <see cref="HitObject"/>s should be considered alive.
        /// </summary>
        internal double PastLifetimeExtension
        {
            get => HitObjectContainer.PastLifetimeExtension;
            set
            {
                HitObjectContainer.PastLifetimeExtension = value;

                foreach (var nested in nestedPlayfields)
                    nested.PastLifetimeExtension = value;
            }
        }

        /// <summary>
        /// The amount of time after the current time within which <see cref="HitObject"/>s should be considered alive.
        /// </summary>
        internal double FutureLifetimeExtension
        {
            get => HitObjectContainer.FutureLifetimeExtension;
            set
            {
                HitObjectContainer.FutureLifetimeExtension = value;

                foreach (var nested in nestedPlayfields)
                    nested.FutureLifetimeExtension = value;
            }
        }

        #endregion
    }
}
