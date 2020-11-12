// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.UI
{
    public abstract class Playfield : CompositeDrawable
    {
        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> is judged.
        /// </summary>
        public event Action<DrawableHitObject, JudgementResult> NewResult;

        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> judgement is reverted.
        /// </summary>
        public event Action<DrawableHitObject, JudgementResult> RevertResult;

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

                if (nestedPlayfields.IsValueCreated)
                    enumerable = enumerable.Concat(NestedPlayfields.SelectMany(p => p.AllHitObjects));

                return enumerable;
            }
        }

        /// <summary>
        /// All <see cref="Playfield"/>s nested inside this <see cref="Playfield"/>.
        /// </summary>
        public IEnumerable<Playfield> NestedPlayfields => nestedPlayfields.IsValueCreated ? nestedPlayfields.Value : Enumerable.Empty<Playfield>();

        private readonly Lazy<List<Playfield>> nestedPlayfields = new Lazy<List<Playfield>>();

        /// <summary>
        /// Whether judgements should be displayed by this and and all nested <see cref="Playfield"/>s.
        /// </summary>
        public readonly BindableBool DisplayJudgements = new BindableBool(true);

        /// <summary>
        /// Creates a new <see cref="Playfield"/>.
        /// </summary>
        protected Playfield()
        {
            RelativeSizeAxes = Axes.Both;

            hitObjectContainerLazy = new Lazy<HitObjectContainer>(() => CreateHitObjectContainer().With(h =>
            {
                h.NewResult += (d, r) => NewResult?.Invoke(d, r);
                h.RevertResult += (d, r) => RevertResult?.Invoke(d, r);
            }));
        }

        [Resolved(CanBeNull = true)]
        private IReadOnlyList<Mod> mods { get; set; }

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

        private readonly Dictionary<HitObject, HitObjectLifetimeEntry> lifetimeEntryMap = new Dictionary<HitObject, HitObjectLifetimeEntry>();

        /// <summary>
        /// Adds a <see cref="HitObjectLifetimeEntry"/> for a pooled <see cref="HitObject"/> to this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="entry">The <see cref="HitObjectLifetimeEntry"/> controlling the lifetime of the <see cref="HitObject"/>.</param>
        public virtual void Add(HitObjectLifetimeEntry entry)
        {
            HitObjectContainer.Add(entry);
            lifetimeEntryMap[entry.HitObject] = entry;
            OnHitObjectAdded(entry.HitObject);
        }

        /// <summary>
        /// Removes a <see cref="HitObjectLifetimeEntry"/> for a pooled <see cref="HitObject"/> from this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="entry">The <see cref="HitObjectLifetimeEntry"/> controlling the lifetime of the <see cref="HitObject"/>.</param>
        /// <returns>Whether the <see cref="HitObject"/> was successfully removed.</returns>
        public virtual bool Remove(HitObjectLifetimeEntry entry)
        {
            if (lifetimeEntryMap.Remove(entry.HitObject))
            {
                HitObjectContainer.Remove(entry);
                OnHitObjectRemoved(entry.HitObject);
                return true;
            }

            bool removedFromNested = false;

            if (nestedPlayfields.IsValueCreated)
                removedFromNested = nestedPlayfields.Value.Any(p => p.Remove(entry));

            return removedFromNested;
        }

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is added to this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject">The added <see cref="HitObject"/>.</param>
        protected virtual void OnHitObjectAdded(HitObject hitObject)
        {
        }

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is removed from this <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject">The removed <see cref="HitObject"/>.</param>
        protected virtual void OnHitObjectRemoved(HitObject hitObject)
        {
        }

        /// <summary>
        /// Sets whether to keep a given <see cref="HitObject"/> always alive within this or any nested <see cref="Playfield"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to set.</param>
        /// <param name="keepAlive">Whether to keep <paramref name="hitObject"/> always alive.</param>
        public void SetKeepAlive(HitObject hitObject, bool keepAlive)
        {
            if (lifetimeEntryMap.TryGetValue(hitObject, out var entry))
            {
                entry.KeepAlive = keepAlive;
                return;
            }

            if (!nestedPlayfields.IsValueCreated)
                return;

            foreach (var p in nestedPlayfields.Value)
                p.SetKeepAlive(hitObject, keepAlive);
        }

        /// <summary>
        /// Keeps all <see cref="HitObject"/>s alive within this and all nested <see cref="Playfield"/>s.
        /// </summary>
        public void KeepAllAlive()
        {
            foreach (var (_, entry) in lifetimeEntryMap)
                entry.KeepAlive = true;

            if (!nestedPlayfields.IsValueCreated)
                return;

            foreach (var p in nestedPlayfields.Value)
                p.KeepAllAlive();
        }

        /// <summary>
        /// The cursor currently being used by this <see cref="Playfield"/>. May be null if no cursor is provided.
        /// </summary>
        public GameplayCursorContainer Cursor { get; private set; }

        /// <summary>
        /// Provide a cursor which is to be used for gameplay.
        /// </summary>
        /// <remarks>
        /// The default provided cursor is invisible when inside the bounds of the <see cref="Playfield"/>.
        /// </remarks>
        /// <returns>The cursor, or null to show the menu cursor.</returns>
        protected virtual GameplayCursorContainer CreateCursor() => new InvisibleCursorContainer();

        /// <summary>
        /// Registers a <see cref="Playfield"/> as a nested <see cref="Playfield"/>.
        /// This does not add the <see cref="Playfield"/> to the draw hierarchy.
        /// </summary>
        /// <param name="otherPlayfield">The <see cref="Playfield"/> to add.</param>
        protected void AddNested(Playfield otherPlayfield)
        {
            otherPlayfield.DisplayJudgements.BindTo(DisplayJudgements);

            otherPlayfield.NewResult += (d, r) => NewResult?.Invoke(d, r);
            otherPlayfield.RevertResult += (d, r) => RevertResult?.Invoke(d, r);

            nestedPlayfields.Value.Add(otherPlayfield);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // in the case a consumer forgets to add the HitObjectContainer, we will add it here.
            if (HitObjectContainer.Parent == null)
                AddInternal(HitObjectContainer);
        }

        protected override void Update()
        {
            base.Update();

            if (mods != null)
            {
                foreach (var mod in mods)
                {
                    if (mod is IUpdatableByPlayfield updatable)
                        updatable.Update(this);
                }
            }
        }

        /// <summary>
        /// Creates the container that will be used to contain the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectContainer CreateHitObjectContainer() => new HitObjectContainer();

        public class InvisibleCursorContainer : GameplayCursorContainer
        {
            protected override Drawable CreateCursor() => new InvisibleCursor();

            private class InvisibleCursor : Drawable
            {
            }
        }
    }
}
