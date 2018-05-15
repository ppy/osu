// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Overlays;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using OpenTK;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Base RulesetContainer. Doesn't hold objects.
    /// <para>
    /// Should not be derived - derive <see cref="RulesetContainer{TObject}"/> instead.
    /// </para>
    /// </summary>
    public abstract class RulesetContainer : Container
    {
        /// <summary>
        /// The selected variant.
        /// </summary>
        public virtual int Variant => 0;

        /// <summary>
        /// The input manager for this RulesetContainer.
        /// </summary>
        internal IHasReplayHandler ReplayInputManager => KeyBindingInputManager as IHasReplayHandler;

        /// <summary>
        /// The key conversion input manager for this RulesetContainer.
        /// </summary>
        public PassThroughInputManager KeyBindingInputManager;

        /// <summary>
        /// Whether a replay is currently loaded.
        /// </summary>
        public readonly BindableBool HasReplayLoaded = new BindableBool();

        public abstract IEnumerable<HitObject> Objects { get; }

        private readonly Lazy<Playfield> playfield;
        /// <summary>
        /// The playfield.
        /// </summary>
        public Playfield Playfield => playfield.Value;

        /// <summary>
        /// The cursor provided by this <see cref="RulesetContainer"/>. May be null if no cursor is provided.
        /// </summary>
        public readonly CursorContainer Cursor;

        protected readonly Ruleset Ruleset;

        private IRulesetConfigManager rulesetConfig;
        private OnScreenDisplay onScreenDisplay;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        /// <summary>
        /// A visual representation of a <see cref="Rulesets.Ruleset"/>.
        /// </summary>
        /// <param name="ruleset">The ruleset being repesented.</param>
        protected RulesetContainer(Ruleset ruleset)
        {
            Ruleset = ruleset;
            playfield = new Lazy<Playfield>(CreatePlayfield);

            Cursor = CreateCursor();
        }

        [BackgroundDependencyLoader(true)]
        private void load(OnScreenDisplay onScreenDisplay, SettingsStore settings)
        {
            this.onScreenDisplay = onScreenDisplay;

            rulesetConfig = CreateConfig(Ruleset, settings);

            if (rulesetConfig != null)
            {
                dependencies.Cache(rulesetConfig);
                onScreenDisplay?.BeginTracking(this, rulesetConfig);
            }
        }

        public abstract ScoreProcessor CreateScoreProcessor();

        /// <summary>
        /// Creates a key conversion input manager. An exception will be thrown if a valid <see cref="RulesetInputManager{T}"/> is not returned.
        /// </summary>
        /// <returns>The input manager.</returns>
        public abstract PassThroughInputManager CreateInputManager();

        protected virtual ReplayInputHandler CreateReplayInputHandler(Replay replay) => null;

        public Replay Replay { get; private set; }

        /// <summary>
        /// Sets a replay to be used, overriding local input.
        /// </summary>
        /// <param name="replay">The replay, null for local input.</param>
        public virtual void SetReplay(Replay replay)
        {
            if (ReplayInputManager == null)
                throw new InvalidOperationException($"A {nameof(KeyBindingInputManager)} which supports replay loading is not available");

            Replay = replay;
            ReplayInputManager.ReplayInputHandler = replay != null ? CreateReplayInputHandler(replay) : null;

            HasReplayLoaded.Value = ReplayInputManager.ReplayInputHandler != null;
        }


        /// <summary>
        /// Creates the cursor. May be null if the <see cref="RulesetContainer"/> doesn't provide a custom cursor.
        /// </summary>
        protected virtual CursorContainer CreateCursor() => null;

        protected virtual IRulesetConfigManager CreateConfig(Ruleset ruleset, SettingsStore settings) => null;

        /// <summary>
        /// Creates a Playfield.
        /// </summary>
        /// <returns>The Playfield.</returns>
        protected abstract Playfield CreatePlayfield();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesetConfig != null)
            {
                onScreenDisplay?.StopTracking(this, rulesetConfig);
                rulesetConfig = null;
            }
        }
    }

    /// <summary>
    /// RulesetContainer that applies conversion to Beatmaps. Does not contain a Playfield
    /// and does not load drawable hit objects.
    /// <para>
    /// Should not be derived - derive <see cref="RulesetContainer{TObject}"/> instead.
    /// </para>
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained by this RulesetContainer.</typeparam>
    public abstract class RulesetContainer<TObject> : RulesetContainer
        where TObject : HitObject
    {
        public event Action<Judgement> OnJudgement;
        public event Action<Judgement> OnJudgementRemoved;

        /// <summary>
        /// The Beatmap
        /// </summary>
        public Beatmap<TObject> Beatmap;

        /// <summary>
        /// All the converted hit objects contained by this hit renderer.
        /// </summary>
        public override IEnumerable<HitObject> Objects => Beatmap.HitObjects;

        /// <summary>
        /// The mods which are to be applied.
        /// </summary>
        protected IEnumerable<Mod> Mods;

        /// <summary>
        /// The <see cref="WorkingBeatmap"/> this <see cref="RulesetContainer{TObject}"/> was created with.
        /// </summary>
        protected readonly WorkingBeatmap WorkingBeatmap;

        public override ScoreProcessor CreateScoreProcessor() => new ScoreProcessor<TObject>(this);

        protected override Container<Drawable> Content => content;
        private Container content;

        /// <summary>
        /// Whether to assume the beatmap passed into this <see cref="RulesetContainer{TObject}"/> is for the current ruleset.
        /// Creates a hit renderer for a beatmap.
        /// </summary>
        /// <param name="ruleset">The ruleset being repesented.</param>
        /// <param name="workingBeatmap">The beatmap to create the hit renderer for.</param>
        protected RulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap)
            : base(ruleset)
        {
            Debug.Assert(workingBeatmap != null, "RulesetContainer initialized with a null beatmap.");

            WorkingBeatmap = workingBeatmap;
            // ReSharper disable once PossibleNullReferenceException
            Mods = workingBeatmap.Mods.Value;

            RelativeSizeAxes = Axes.Both;

            Beatmap = (Beatmap<TObject>)workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo);

            KeyBindingInputManager = CreateInputManager();
            KeyBindingInputManager.RelativeSizeAxes = Axes.Both;

            // Add mods, should always be the last thing applied to give full control to mods
            applyMods(Mods);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            KeyBindingInputManager.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddInternal(KeyBindingInputManager);
            KeyBindingInputManager.Add(Playfield);

            if (Cursor != null)
                KeyBindingInputManager.Add(Cursor);

            loadObjects();
        }

        /// <summary>
        /// Applies the active mods to this RulesetContainer.
        /// </summary>
        /// <param name="mods"></param>
        private void applyMods(IEnumerable<Mod> mods)
        {
            if (mods == null)
                return;

            foreach (var mod in mods.OfType<IApplicableToRulesetContainer<TObject>>())
                mod.ApplyToRulesetContainer(this);
        }

        public override void SetReplay(Replay replay)
        {
            base.SetReplay(replay);

            if (ReplayInputManager?.ReplayInputHandler != null)
                ReplayInputManager.ReplayInputHandler.GamefieldToScreenSpace = Playfield.GamefieldToScreenSpace;
        }

        /// <summary>
        /// Creates and adds drawable representations of hit objects to the play field.
        /// </summary>
        private void loadObjects()
        {
            foreach (TObject h in Beatmap.HitObjects)
            {
                var drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null)
                    continue;

                drawableObject.OnJudgement += (d, j) => OnJudgement?.Invoke(j);
                drawableObject.OnJudgementRemoved += (d, j) => OnJudgementRemoved?.Invoke(j);

                Playfield.Add(drawableObject);
            }

            Playfield.PostProcess();

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(Playfield.HitObjects.Objects);
        }

        protected override void Update()
        {
            base.Update();

            Playfield.Size = GetAspectAdjustedSize() * PlayfieldArea;
        }

        /// <summary>
        /// Computes the size of the <see cref="Playfield"/> in relative coordinate space after aspect adjustments.
        /// </summary>
        /// <returns>The aspect-adjusted size.</returns>
        protected virtual Vector2 GetAspectAdjustedSize() => Vector2.One;

        /// <summary>
        /// The area of this <see cref="RulesetContainer"/> that is available for the <see cref="Playfield"/> to use.
        /// Must be specified in relative coordinate space to this <see cref="RulesetContainer"/>.
        /// This affects the final size of the <see cref="Playfield"/> but does not affect the <see cref="Playfield"/>'s scale.
        /// </summary>
        protected virtual Vector2 PlayfieldArea => new Vector2(0.75f); // A sane default

        /// <summary>
        /// Creates a DrawableHitObject from a HitObject.
        /// </summary>
        /// <param name="h">The HitObject to make drawable.</param>
        /// <returns>The DrawableHitObject.</returns>
        protected abstract DrawableHitObject<TObject> GetVisualRepresentation(TObject h);
    }

    /// <summary>
    /// A derivable RulesetContainer that manages the Playfield and HitObjects.
    /// </summary>
    /// <typeparam name="TPlayfield">The type of Playfield contained by this RulesetContainer.</typeparam>
    /// <typeparam name="TObject">The type of HitObject contained by this RulesetContainer.</typeparam>
    public abstract class RulesetContainer<TPlayfield, TObject> : RulesetContainer<TObject>
        where TObject : HitObject
        where TPlayfield : Playfield
    {
        /// <summary>
        /// The playfield.
        /// </summary>
        protected new TPlayfield Playfield => (TPlayfield)base.Playfield;

        /// <summary>
        /// Creates a hit renderer for a beatmap.
        /// </summary>
        /// <param name="ruleset">The ruleset being repesented.</param>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        protected RulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }
    }

    public class BeatmapInvalidForRulesetException : ArgumentException
    {
        public BeatmapInvalidForRulesetException(string text)
            : base(text)
        {
        }
    }
}
