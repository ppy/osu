﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Replays;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

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

        /// <summary>
        /// The point in time at which gameplay starts, including any required lead-in for display purposes.
        /// Defaults to two seconds before the first <see cref="HitObject"/>. Override as necessary.
        /// </summary>
        public virtual double GameplayStartTime => Objects.First().StartTime - 2000;

        private readonly Lazy<Playfield> playfield;

        /// <summary>
        /// The playfield.
        /// </summary>
        public Playfield Playfield => playfield.Value;

        /// <summary>
        /// Place to put drawables above hit objects but below UI.
        /// </summary>
        public Container Overlays { get; protected set; }

        /// <summary>
        /// The cursor provided by this <see cref="RulesetContainer"/>. May be null if no cursor is provided.
        /// </summary>
        public readonly CursorContainer Cursor;

        public readonly Ruleset Ruleset;

        protected IRulesetConfigManager Config { get; private set; }

        private OnScreenDisplay onScreenDisplay;

        /// <summary>
        /// A visual representation of a <see cref="Rulesets.Ruleset"/>.
        /// </summary>
        /// <param name="ruleset">The ruleset being repesented.</param>
        protected RulesetContainer(Ruleset ruleset)
        {
            Ruleset = ruleset;
            playfield = new Lazy<Playfield>(CreatePlayfield);

            IsPaused.ValueChanged += paused =>
            {
                if (HasReplayLoaded)
                    return;

                KeyBindingInputManager.UseParentInput = !paused;
            };

            Cursor = CreateCursor();
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            onScreenDisplay = dependencies.Get<OnScreenDisplay>();

            Config = dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);
            if (Config != null)
            {
                dependencies.Cache(Config);
                onScreenDisplay?.BeginTracking(this, Config);
            }

            return dependencies;
        }

        public abstract ScoreProcessor CreateScoreProcessor();

        /// <summary>
        /// Creates a key conversion input manager. An exception will be thrown if a valid <see cref="RulesetInputManager{T}"/> is not returned.
        /// </summary>
        /// <returns>The input manager.</returns>
        public abstract PassThroughInputManager CreateInputManager();

        protected virtual ReplayInputHandler CreateReplayInputHandler(Replay replay) => null;

        public Score ReplayScore { get; private set; }

        /// <summary>
        /// Whether the game is paused. Used to block user input.
        /// </summary>
        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// Sets a replay to be used, overriding local input.
        /// </summary>
        /// <param name="replayScore">The replay, null for local input.</param>
        public virtual void SetReplayScore(Score replayScore)
        {
            if (ReplayInputManager == null)
                throw new InvalidOperationException($"A {nameof(KeyBindingInputManager)} which supports replay loading is not available");

            ReplayScore = replayScore;
            ReplayInputManager.ReplayInputHandler = replayScore != null ? CreateReplayInputHandler(replayScore.Replay) : null;

            HasReplayLoaded.Value = ReplayInputManager.ReplayInputHandler != null;
        }

        /// <summary>
        /// Creates the cursor. May be null if the <see cref="RulesetContainer"/> doesn't provide a custom cursor.
        /// </summary>
        protected virtual CursorContainer CreateCursor() => null;

        /// <summary>
        /// Creates a Playfield.
        /// </summary>
        /// <returns>The Playfield.</returns>
        protected abstract Playfield CreatePlayfield();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (Config != null)
            {
                onScreenDisplay?.StopTracking(this, Config);
                Config = null;
            }
        }
    }

    /// <summary>
    /// RulesetContainer that applies conversion to Beatmaps. Does not contain a Playfield
    /// and does not load drawable hit objects.
    /// <para>
    /// Should not be derived - derive <see cref="RulesetContainer{TPlayfield, TObject}"/> instead.
    /// </para>
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained by this RulesetContainer.</typeparam>
    public abstract class RulesetContainer<TObject> : RulesetContainer
        where TObject : HitObject
    {
        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> has been applied by a <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<JudgementResult> OnNewResult;

        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> is being reverted by a <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<JudgementResult> OnRevertResult;

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

            applyBeatmapMods(Mods);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            KeyBindingInputManager.AddRange(new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                Playfield
            });

            if (Cursor != null)
                KeyBindingInputManager.Add(Cursor);

            InternalChildren = new Drawable[]
            {
                KeyBindingInputManager,
                Overlays = new Container { RelativeSizeAxes = Axes.Both }
            };

            // Apply mods
            applyRulesetMods(Mods, config);

            loadObjects();
        }

        /// <summary>
        /// Applies the active mods to the Beatmap.
        /// </summary>
        /// <param name="mods"></param>
        private void applyBeatmapMods(IEnumerable<Mod> mods)
        {
            if (mods == null)
                return;

            foreach (var mod in mods.OfType<IApplicableToBeatmap<TObject>>())
                mod.ApplyToBeatmap(Beatmap);
        }

        /// <summary>
        /// Applies the active mods to this RulesetContainer.
        /// </summary>
        /// <param name="mods"></param>
        private void applyRulesetMods(IEnumerable<Mod> mods, OsuConfigManager config)
        {
            if (mods == null)
                return;

            foreach (var mod in mods.OfType<IApplicableToRulesetContainer<TObject>>())
                mod.ApplyToRulesetContainer(this);

            foreach (var mod in mods.OfType<IReadFromConfig>())
                mod.ReadFromConfig(config);
        }

        public override void SetReplayScore(Score replayScore)
        {
            base.SetReplayScore(replayScore);

            if (ReplayInputManager?.ReplayInputHandler != null)
                ReplayInputManager.ReplayInputHandler.GamefieldToScreenSpace = Playfield.GamefieldToScreenSpace;
        }

        /// <summary>
        /// Creates and adds drawable representations of hit objects to the play field.
        /// </summary>
        private void loadObjects()
        {
            foreach (TObject h in Beatmap.HitObjects)
                AddRepresentation(h);

            Playfield.PostProcess();

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(Playfield.HitObjectContainer.Objects);
        }

        /// <summary>
        /// Creates and adds the visual representation of a <see cref="TObject"/> to this <see cref="RulesetContainer{TObject}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="TObject"/> to add the visual representation for.</param>
        internal void AddRepresentation(TObject hitObject)
        {
            var drawableObject = GetVisualRepresentation(hitObject);

            if (drawableObject == null)
                return;

            drawableObject.OnNewResult += (_, r) => OnNewResult?.Invoke(r);
            drawableObject.OnRevertResult += (_, r) => OnRevertResult?.Invoke(r);

            Playfield.Add(drawableObject);
        }

        /// <summary>
        /// Creates a DrawableHitObject from a HitObject.
        /// </summary>
        /// <param name="h">The HitObject to make drawable.</param>
        /// <returns>The DrawableHitObject.</returns>
        public abstract DrawableHitObject<TObject> GetVisualRepresentation(TObject h);
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
