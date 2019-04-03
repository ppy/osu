// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Framework.Bindables;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Handlers;
using osu.Game.Overlays;
using osu.Game.Replays;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Displays an interactive ruleset gameplay instance.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained by this DrawableRuleset.</typeparam>
    public abstract class DrawableRuleset<TObject> : DrawableRuleset, IProvideCursor, ICanAttachKeyCounter
        where TObject : HitObject
    {
        /// <summary>
        /// The selected variant.
        /// </summary>
        public virtual int Variant => 0;

        /// <summary>
        /// The key conversion input manager for this DrawableRuleset.
        /// </summary>
        public PassThroughInputManager KeyBindingInputManager;

        public override double GameplayStartTime => Objects.First().StartTime - 2000;

        private readonly Lazy<Playfield> playfield;

        /// <summary>
        /// The playfield.
        /// </summary>
        public Playfield Playfield => playfield.Value;

        /// <summary>
        /// Place to put drawables above hit objects but below UI.
        /// </summary>
        public Container Overlays { get; private set; }

        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> has been applied by a <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<JudgementResult> OnNewResult;

        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> is being reverted by a <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<JudgementResult> OnRevertResult;

        /// <summary>
        /// The beatmap.
        /// </summary>
        public Beatmap<TObject> Beatmap;

        public override IEnumerable<HitObject> Objects => Beatmap.HitObjects;

        protected IRulesetConfigManager Config { get; private set; }

        /// <summary>
        /// The mods which are to be applied.
        /// </summary>
        private readonly IEnumerable<Mod> mods;

        private FrameStabilityContainer frameStabilityContainer;

        private OnScreenDisplay onScreenDisplay;

        /// <summary>
        /// Creates a ruleset visualisation for the provided ruleset and beatmap.
        /// </summary>
        /// <param name="ruleset">The ruleset being represented.</param>
        /// <param name="workingBeatmap">The beatmap to create the hit renderer for.</param>
        protected DrawableRuleset(Ruleset ruleset, WorkingBeatmap workingBeatmap)
            : base(ruleset)
        {
            Debug.Assert(workingBeatmap != null, "DrawableRuleset initialized with a null beatmap.");

            RelativeSizeAxes = Axes.Both;

            Beatmap = (Beatmap<TObject>)workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo);

            mods = workingBeatmap.Mods.Value;
            applyBeatmapMods(mods);

            KeyBindingInputManager = CreateInputManager();
            playfield = new Lazy<Playfield>(CreatePlayfield);

            IsPaused.ValueChanged += paused =>
            {
                if (HasReplayLoaded.Value)
                    return;

                KeyBindingInputManager.UseParentInput = !paused.NewValue;
            };
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

        public virtual PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new PlayfieldAdjustmentContainer();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                frameStabilityContainer = new FrameStabilityContainer
                {
                    Child = KeyBindingInputManager
                        .WithChild(CreatePlayfieldAdjustmentContainer()
                            .WithChild(Playfield)
                        )
                },
                Overlays = new Container { RelativeSizeAxes = Axes.Both }
            };

            if ((ResumeOverlay = CreateResumeOverlay()) != null)
            {
                AddInternal(CreateInputManager()
                    .WithChild(CreatePlayfieldAdjustmentContainer()
                        .WithChild(ResumeOverlay)));
            }

            applyRulesetMods(mods, config);

            loadObjects();
        }

        /// <summary>
        /// Creates and adds drawable representations of hit objects to the play field.
        /// </summary>
        private void loadObjects()
        {
            foreach (TObject h in Beatmap.HitObjects)
                addHitObject(h);

            Playfield.PostProcess();

            foreach (var mod in mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(Playfield.HitObjectContainer.Objects);
        }

        public override void RequestResume(Action continueResume)
        {
            if (ResumeOverlay != null && (Cursor == null || (Cursor.LastFrameState == Visibility.Visible && Contains(Cursor.ActiveCursor.ScreenSpaceDrawQuad.Centre))))
            {
                ResumeOverlay.GameplayCursor = Cursor;
                ResumeOverlay.ResumeAction = continueResume;
                ResumeOverlay.Show();
            }
            else
                continueResume();
        }

        public ResumeOverlay ResumeOverlay { get; private set; }

        protected virtual ResumeOverlay CreateResumeOverlay() => null;

        /// <summary>
        /// Creates and adds the visual representation of a <see cref="TObject"/> to this <see cref="DrawableRuleset{TObject}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="TObject"/> to add the visual representation for.</param>
        private void addHitObject(TObject hitObject)
        {
            var drawableObject = CreateDrawableRepresentation(hitObject);

            if (drawableObject == null)
                return;

            drawableObject.OnNewResult += (_, r) => OnNewResult?.Invoke(r);
            drawableObject.OnRevertResult += (_, r) => OnRevertResult?.Invoke(r);

            Playfield.Add(drawableObject);
        }

        public override void SetReplayScore(Score replayScore)
        {
            if (!(KeyBindingInputManager is IHasReplayHandler replayInputManager))
                throw new InvalidOperationException($"A {nameof(KeyBindingInputManager)} which supports replay loading is not available");

            var handler = (ReplayScore = replayScore) != null ? CreateReplayInputHandler(replayScore.Replay) : null;

            replayInputManager.ReplayInputHandler = handler;
            frameStabilityContainer.ReplayInputHandler = handler;

            HasReplayLoaded.Value = replayInputManager.ReplayInputHandler != null;

            if (replayInputManager.ReplayInputHandler != null)
                replayInputManager.ReplayInputHandler.GamefieldToScreenSpace = Playfield.GamefieldToScreenSpace;
        }

        /// <summary>
        /// Creates a DrawableHitObject from a HitObject.
        /// </summary>
        /// <param name="h">The HitObject to make drawable.</param>
        /// <returns>The DrawableHitObject.</returns>
        public abstract DrawableHitObject<TObject> CreateDrawableRepresentation(TObject h);

        public void Attach(KeyCounterDisplay keyCounter) =>
            (KeyBindingInputManager as ICanAttachKeyCounter)?.Attach(keyCounter);

        /// <summary>
        /// Creates a key conversion input manager. An exception will be thrown if a valid <see cref="RulesetInputManager{T}"/> is not returned.
        /// </summary>
        /// <returns>The input manager.</returns>
        protected abstract PassThroughInputManager CreateInputManager();

        protected virtual ReplayInputHandler CreateReplayInputHandler(Replay replay) => null;

        /// <summary>
        /// Creates a Playfield.
        /// </summary>
        /// <returns>The Playfield.</returns>
        protected abstract Playfield CreatePlayfield();

        public override ScoreProcessor CreateScoreProcessor() => new ScoreProcessor<TObject>(this);

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
        /// Applies the active mods to this DrawableRuleset.
        /// </summary>
        /// <param name="mods"></param>
        private void applyRulesetMods(IEnumerable<Mod> mods, OsuConfigManager config)
        {
            if (mods == null)
                return;

            foreach (var mod in mods.OfType<IApplicableToDrawableRuleset<TObject>>())
                mod.ApplyToDrawableRuleset(this);

            foreach (var mod in mods.OfType<IReadFromConfig>())
                mod.ReadFromConfig(config);
        }

        #region IProvideCursor

        protected override bool OnHover(HoverEvent e) => true; // required for IProvideCursor

        CursorContainer IProvideCursor.Cursor => Playfield.Cursor;

        public override GameplayCursorContainer Cursor => Playfield.Cursor;

        public bool ProvidingUserCursor => Playfield.Cursor != null && !HasReplayLoaded.Value;

        #endregion

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
    /// Displays an interactive ruleset gameplay instance.
    /// <remarks>
    /// This type is required only for adding non-generic type to the draw hierarchy.
    /// Once IDrawable is a thing, this can also become an interface.
    /// </remarks>
    /// </summary>
    public abstract class DrawableRuleset : CompositeDrawable
    {
        /// <summary>
        /// Whether a replay is currently loaded.
        /// </summary>
        public readonly BindableBool HasReplayLoaded = new BindableBool();

        /// <summary>
        /// Whether the game is paused. Used to block user input.
        /// </summary>
        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>~
        /// The associated ruleset.
        /// </summary>
        public readonly Ruleset Ruleset;

        /// <summary>
        /// Creates a ruleset visualisation for the provided ruleset.
        /// </summary>
        /// <param name="ruleset">The ruleset.</param>
        internal DrawableRuleset(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        /// <summary>
        /// All the converted hit objects contained by this hit renderer.
        /// </summary>
        public abstract IEnumerable<HitObject> Objects { get; }

        /// <summary>
        /// The point in time at which gameplay starts, including any required lead-in for display purposes.
        /// Defaults to two seconds before the first <see cref="HitObject"/>. Override as necessary.
        /// </summary>
        public abstract double GameplayStartTime { get; }

        /// <summary>
        /// The currently loaded replay. Usually null in the case of a local player.
        /// </summary>
        public Score ReplayScore { get; protected set; }

        /// <summary>
        /// The cursor being displayed by the <see cref="Playfield"/>. May be null if no cursor is provided.
        /// </summary>
        public abstract GameplayCursorContainer Cursor { get; }

        /// <summary>
        /// Sets a replay to be used, overriding local input.
        /// </summary>
        /// <param name="replayScore">The replay, null for local input.</param>
        public abstract void SetReplayScore(Score replayScore);

        /// <summary>
        /// Invoked when the interactive user requests resuming from a paused state.
        /// Allows potentially delaying the resume process until an interaction is performed.
        /// </summary>
        /// <param name="continueResume">The action to run when resuming is to be completed.</param>
        public abstract void RequestResume(Action continueResume);

        /// <summary>
        /// Create a <see cref="ScoreProcessor"/> for the associated ruleset  and link with this
        /// <see cref="DrawableRuleset"/>.
        /// </summary>
        /// <returns>A score processor.</returns>
        public abstract ScoreProcessor CreateScoreProcessor();
    }

    public class BeatmapInvalidForRulesetException : ArgumentException
    {
        public BeatmapInvalidForRulesetException(string text)
            : base(text)
        {
        }
    }
}
