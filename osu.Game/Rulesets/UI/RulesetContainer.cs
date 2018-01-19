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
        /// Whether to apply adjustments to the child <see cref="Playfield"/> based on our own size.
        /// </summary>
        public bool AspectAdjust = true;

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

        public abstract ScoreProcessor CreateScoreProcessor();

        /// <summary>
        /// Creates a key conversion input manager. An exception will be thrown if a valid <see cref="RulesetInputManager{T}"/> is not returned.
        /// </summary>
        /// <returns>The input manager.</returns>
        public abstract PassThroughInputManager CreateInputManager();

        protected virtual FramedReplayInputHandler CreateReplayInputHandler(Replay replay) => null;

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

        /// <summary>
        /// Creates a Playfield.
        /// </summary>
        /// <returns>The Playfield.</returns>
        protected abstract Playfield CreatePlayfield();
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

        /// <summary>
        /// Whether the specified beatmap is assumed to be specific to the current ruleset.
        /// </summary>
        protected readonly bool IsForCurrentRuleset;

        public override ScoreProcessor CreateScoreProcessor() => new ScoreProcessor<TObject>(this);

        protected override Container<Drawable> Content => content;
        private Container content;

        /// <summary>
        /// Whether to assume the beatmap passed into this <see cref="RulesetContainer{TObject}"/> is for the current ruleset.
        /// Creates a hit renderer for a beatmap.
        /// </summary>
        /// <param name="ruleset">The ruleset being repesented.</param>
        /// <param name="workingBeatmap">The beatmap to create the hit renderer for.</param>
        /// <param name="isForCurrentRuleset">Whether to assume the beatmap is for the current ruleset.</param>
        protected RulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap, bool isForCurrentRuleset)
            : base(ruleset)
        {
            Debug.Assert(workingBeatmap != null, "RulesetContainer initialized with a null beatmap.");

            WorkingBeatmap = workingBeatmap;
            IsForCurrentRuleset = isForCurrentRuleset;
            Mods = workingBeatmap.Mods.Value;

            RelativeSizeAxes = Axes.Both;

            BeatmapConverter<TObject> converter = CreateBeatmapConverter();
            BeatmapProcessor<TObject> processor = CreateBeatmapProcessor();

            // Check if the beatmap can be converted
            if (!converter.CanConvert(workingBeatmap.Beatmap))
                throw new BeatmapInvalidForRulesetException($"{nameof(Beatmap)} can not be converted for the current ruleset (converter: {converter}).");

            // Apply conversion adjustments before converting
            foreach (var mod in Mods.OfType<IApplicableToBeatmapConverter<TObject>>())
                mod.ApplyToBeatmapConverter(converter);

            // Convert the beatmap
            Beatmap = converter.Convert(workingBeatmap.Beatmap);

            // Apply difficulty adjustments from mods before using Difficulty.
            foreach (var mod in Mods.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(Beatmap.BeatmapInfo.BaseDifficulty);

            // Apply defaults
            foreach (var h in Beatmap.HitObjects)
                h.ApplyDefaults(Beatmap.ControlPointInfo, Beatmap.BeatmapInfo.BaseDifficulty);

            // Post-process the beatmap
            processor.PostProcess(Beatmap);

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

            foreach (var mod in mods.OfType<IApplicableToHitObject<TObject>>())
                foreach (var obj in Beatmap.HitObjects)
                    mod.ApplyToHitObject(obj);

            foreach (var mod in mods.OfType<IApplicableToRulesetContainer<TObject>>())
                mod.ApplyToRulesetContainer(this);
        }

        public override void SetReplay(Replay replay)
        {
            base.SetReplay(replay);

            if (ReplayInputManager?.ReplayInputHandler != null)
                ReplayInputManager.ReplayInputHandler.ToScreenSpace = input => Playfield.ScaledContent.ToScreenSpace(input);
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

            Playfield.Size = AspectAdjust ? GetPlayfieldAspectAdjust() : Vector2.One;
        }

        /// <summary>
        /// Creates a processor to perform post-processing operations
        /// on HitObjects in converted Beatmaps.
        /// </summary>
        /// <returns>The Beatmap processor.</returns>
        protected virtual BeatmapProcessor<TObject> CreateBeatmapProcessor() => new BeatmapProcessor<TObject>();

        /// <summary>
        /// In some cases we want to apply changes to the relative size of our contained <see cref="Playfield"/> based on custom conditions.
        /// </summary>
        /// <returns></returns>
        protected virtual Vector2 GetPlayfieldAspectAdjust() => new Vector2(0.75f); //a sane default

        /// <summary>
        /// Creates a converter to convert Beatmap to a specific mode.
        /// </summary>
        /// <returns>The Beatmap converter.</returns>
        protected abstract BeatmapConverter<TObject> CreateBeatmapConverter();

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
        /// <param name="isForCurrentRuleset">Whether to assume the beatmap is for the current ruleset.</param>
        protected RulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
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
