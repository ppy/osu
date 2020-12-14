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
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
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
using osuTK;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Displays an interactive ruleset gameplay instance.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained by this DrawableRuleset.</typeparam>
    public abstract class DrawableRuleset<TObject> : DrawableRuleset, IProvideCursor, ICanAttachKeyCounter
        where TObject : HitObject
    {
        public override event Action<JudgementResult> NewResult;
        public override event Action<JudgementResult> RevertResult;

        /// <summary>
        /// The selected variant.
        /// </summary>
        public virtual int Variant => 0;

        /// <summary>
        /// The key conversion input manager for this DrawableRuleset.
        /// </summary>
        public PassThroughInputManager KeyBindingInputManager;

        public override double GameplayStartTime => Objects.FirstOrDefault()?.StartTime - 2000 ?? 0;

        private readonly Lazy<Playfield> playfield;

        /// <summary>
        /// The playfield.
        /// </summary>
        public override Playfield Playfield => playfield.Value;

        public override Container Overlays { get; } = new Container { RelativeSizeAxes = Axes.Both };

        public override Container FrameStableComponents { get; } = new Container { RelativeSizeAxes = Axes.Both };

        public override IFrameStableClock FrameStableClock => frameStabilityContainer.FrameStableClock;

        private bool frameStablePlayback = true;

        /// <summary>
        /// Whether to enable frame-stable playback.
        /// </summary>
        internal bool FrameStablePlayback
        {
            get => frameStablePlayback;
            set
            {
                frameStablePlayback = value;
                if (frameStabilityContainer != null)
                    frameStabilityContainer.FrameStablePlayback = value;
            }
        }

        /// <summary>
        /// The beatmap.
        /// </summary>
        public readonly Beatmap<TObject> Beatmap;

        public override IEnumerable<HitObject> Objects => Beatmap.HitObjects;

        protected IRulesetConfigManager Config { get; private set; }

        [Cached(typeof(IReadOnlyList<Mod>))]
        protected override IReadOnlyList<Mod> Mods { get; }

        private FrameStabilityContainer frameStabilityContainer;

        private OnScreenDisplay onScreenDisplay;

        private DrawableRulesetDependencies dependencies;

        /// <summary>
        /// Creates a ruleset visualisation for the provided ruleset and beatmap.
        /// </summary>
        /// <param name="ruleset">The ruleset being represented.</param>
        /// <param name="beatmap">The beatmap to create the hit renderer for.</param>
        /// <param name="mods">The <see cref="Mod"/>s to apply.</param>
        protected DrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap), "Beatmap cannot be null.");

            if (!(beatmap is Beatmap<TObject> tBeatmap))
                throw new ArgumentException($"{GetType()} expected the beatmap to contain hitobjects of type {typeof(TObject)}.", nameof(beatmap));

            Beatmap = tBeatmap;
            Mods = mods?.ToArray() ?? Array.Empty<Mod>();

            RelativeSizeAxes = Axes.Both;

            KeyBindingInputManager = CreateInputManager();
            playfield = new Lazy<Playfield>(() => CreatePlayfield().With(p =>
            {
                p.NewResult += (_, r) => NewResult?.Invoke(r);
                p.RevertResult += (_, r) => RevertResult?.Invoke(r);
            }));

            IsPaused.ValueChanged += paused =>
            {
                if (HasReplayLoaded.Value)
                    return;

                KeyBindingInputManager.UseParentInput = !paused.NewValue;
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DrawableRulesetDependencies(Ruleset, base.CreateChildDependencies(parent));

            Config = dependencies.RulesetConfigManager;

            onScreenDisplay = dependencies.Get<OnScreenDisplay>();
            if (Config != null)
                onScreenDisplay?.BeginTracking(this, Config);

            return dependencies;
        }

        public virtual PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new PlayfieldAdjustmentContainer();

        [Resolved]
        private OsuConfigManager config { get; set; }

        [BackgroundDependencyLoader]
        private void load(CancellationToken? cancellationToken)
        {
            InternalChildren = new Drawable[]
            {
                frameStabilityContainer = new FrameStabilityContainer(GameplayStartTime)
                {
                    FrameStablePlayback = FrameStablePlayback,
                    Children = new Drawable[]
                    {
                        FrameStableComponents,
                        KeyBindingInputManager
                            .WithChild(CreatePlayfieldAdjustmentContainer()
                                .WithChild(Playfield)
                            ),
                        Overlays,
                    }
                },
            };

            if ((ResumeOverlay = CreateResumeOverlay()) != null)
            {
                AddInternal(CreateInputManager()
                    .WithChild(CreatePlayfieldAdjustmentContainer()
                        .WithChild(ResumeOverlay)));
            }

            RegenerateAutoplay();

            loadObjects(cancellationToken ?? default);
        }

        public void RegenerateAutoplay()
        {
            // for now this is applying mods which aren't just autoplay.
            // we'll need to reconsider this flow in the future.
            applyRulesetMods(Mods, config);
        }

        /// <summary>
        /// Creates and adds drawable representations of hit objects to the play field.
        /// </summary>
        private void loadObjects(CancellationToken cancellationToken)
        {
            foreach (TObject h in Beatmap.HitObjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddHitObject(h);
            }

            cancellationToken.ThrowIfCancellationRequested();

            Playfield.PostProcess();

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(Playfield.AllHitObjects);
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

        public override void CancelResume()
        {
            // called if the user pauses while the resume overlay is open
            ResumeOverlay?.Hide();
        }

        /// <summary>
        /// Adds a <see cref="HitObject"/> to this <see cref="DrawableRuleset"/>.
        /// </summary>
        /// <remarks>
        /// This does not add the <see cref="HitObject"/> to the beatmap.
        /// </remarks>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void AddHitObject(TObject hitObject)
        {
            var drawableRepresentation = CreateDrawableRepresentation(hitObject);

            // If a drawable representation exists, use it, otherwise assume the hitobject is being pooled.
            if (drawableRepresentation != null)
                Playfield.Add(drawableRepresentation);
            else
                Playfield.Add(hitObject);
        }

        /// <summary>
        /// Removes a <see cref="HitObject"/> from this <see cref="DrawableRuleset"/>.
        /// </summary>
        /// <remarks>
        /// This does not remove the <see cref="HitObject"/> from the beatmap.
        /// </remarks>
        /// <param name="hitObject">The <see cref="HitObject"/> to remove.</param>
        public bool RemoveHitObject(TObject hitObject)
        {
            if (Playfield.Remove(hitObject))
                return true;

            // If the entry was not removed from the playfield, assume the hitobject is not being pooled and attempt a direct drawable removal.
            var drawableObject = Playfield.AllHitObjects.SingleOrDefault(d => d.HitObject == hitObject);
            if (drawableObject != null)
                return Playfield.Remove(drawableObject);

            return false;
        }

        public sealed override void SetRecordTarget(Score score)
        {
            if (!(KeyBindingInputManager is IHasRecordingHandler recordingInputManager))
                throw new InvalidOperationException($"A {nameof(KeyBindingInputManager)} which supports recording is not available");

            var recorder = CreateReplayRecorder(score);

            if (recorder == null)
                return;

            recorder.ScreenSpaceToGamefield = Playfield.ScreenSpaceToGamefield;

            recordingInputManager.Recorder = recorder;
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

            if (!ProvidingUserCursor)
            {
                // The cursor is hidden by default (see Playfield.load()), but should be shown when there's a replay
                Playfield.Cursor?.Show();
            }
        }

        /// <summary>
        /// Creates a <see cref="DrawableHitObject{TObject}"/> to represent a <see cref="HitObject"/>.
        /// </summary>
        /// <remarks>
        /// If this method returns <c>null</c>, then this <see cref="DrawableRuleset"/> will assume the requested <see cref="HitObject"/> type is being pooled inside the <see cref="Playfield"/>,
        /// and will instead attempt to retrieve the <see cref="DrawableHitObject"/>s at the point they should become alive via pools registered in the <see cref="Playfield"/>.
        /// </remarks>
        /// <param name="h">The <see cref="HitObject"/> to represent.</param>
        /// <returns>The representing <see cref="DrawableHitObject{TObject}"/>.</returns>
        public abstract DrawableHitObject<TObject> CreateDrawableRepresentation(TObject h);

        public void Attach(KeyCounterDisplay keyCounter) =>
            (KeyBindingInputManager as ICanAttachKeyCounter)?.Attach(keyCounter);

        /// <summary>
        /// Creates a key conversion input manager. An exception will be thrown if a valid <see cref="RulesetInputManager{T}"/> is not returned.
        /// </summary>
        /// <returns>The input manager.</returns>
        protected abstract PassThroughInputManager CreateInputManager();

        protected virtual ReplayInputHandler CreateReplayInputHandler(Replay replay) => null;

        protected virtual ReplayRecorder CreateReplayRecorder(Score score) => null;

        /// <summary>
        /// Creates a Playfield.
        /// </summary>
        /// <returns>The Playfield.</returns>
        protected abstract Playfield CreatePlayfield();

        /// <summary>
        /// Applies the active mods to this DrawableRuleset.
        /// </summary>
        /// <param name="mods">The <see cref="Mod"/>s to apply.</param>
        /// <param name="config">The <see cref="OsuConfigManager"/> to apply.</param>
        private void applyRulesetMods(IReadOnlyList<Mod> mods, OsuConfigManager config)
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

        // only show the cursor when within the playfield, by default.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Playfield.ReceivePositionalInputAt(screenSpacePos);

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

            // Dispose the components created by this dependency container.
            dependencies?.Dispose();
        }
    }

    /// <summary>
    /// Displays an interactive ruleset gameplay instance.
    /// <remarks>
    /// This type is required only for adding non-generic type to the draw hierarchy.
    /// </remarks>
    /// </summary>
    [Cached(typeof(DrawableRuleset))]
    public abstract class DrawableRuleset : CompositeDrawable
    {
        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> has been applied by a <see cref="DrawableHitObject"/>.
        /// </summary>
        public abstract event Action<JudgementResult> NewResult;

        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> is being reverted by a <see cref="DrawableHitObject"/>.
        /// </summary>
        public abstract event Action<JudgementResult> RevertResult;

        /// <summary>
        /// Whether a replay is currently loaded.
        /// </summary>
        public readonly BindableBool HasReplayLoaded = new BindableBool();

        /// <summary>
        /// Whether the game is paused. Used to block user input.
        /// </summary>
        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The playfield.
        /// </summary>
        public abstract Playfield Playfield { get; }

        /// <summary>
        /// Content to be placed above hitobjects. Will be affected by frame stability.
        /// </summary>
        public abstract Container Overlays { get; }

        /// <summary>
        /// Components to be run potentially multiple times in line with frame-stable gameplay.
        /// </summary>
        public abstract Container FrameStableComponents { get; }

        /// <summary>
        /// The frame-stable clock which is being used for playfield display.
        /// </summary>
        public abstract IFrameStableClock FrameStableClock { get; }

        /// <summary>
        /// The mods which are to be applied.
        /// </summary>
        protected abstract IReadOnlyList<Mod> Mods { get; }

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
        /// An optional overlay used when resuming gameplay from a paused state.
        /// </summary>
        public ResumeOverlay ResumeOverlay { get; protected set; }

        /// <summary>
        /// Returns first available <see cref="HitWindows"/> provided by a <see cref="HitObject"/>.
        /// </summary>
        [CanBeNull]
        public HitWindows FirstAvailableHitWindows
        {
            get
            {
                foreach (var h in Objects)
                {
                    if (h.HitWindows.WindowFor(HitResult.Miss) > 0)
                        return h.HitWindows;

                    foreach (var n in h.NestedHitObjects)
                    {
                        if (h.HitWindows.WindowFor(HitResult.Miss) > 0)
                            return n.HitWindows;
                    }
                }

                return null;
            }
        }

        protected virtual ResumeOverlay CreateResumeOverlay() => null;

        /// <summary>
        /// Whether to display gameplay overlays, such as <see cref="HUDOverlay"/> and <see cref="BreakOverlay"/>.
        /// </summary>
        public virtual bool AllowGameplayOverlays => true;

        /// <summary>
        /// Sets a replay to be used, overriding local input.
        /// </summary>
        /// <param name="replayScore">The replay, null for local input.</param>
        public abstract void SetReplayScore(Score replayScore);

        /// <summary>
        /// Sets a replay to be used to record gameplay.
        /// </summary>
        /// <param name="score">The target to be recorded to.</param>
        public abstract void SetRecordTarget(Score score);

        /// <summary>
        /// Invoked when the interactive user requests resuming from a paused state.
        /// Allows potentially delaying the resume process until an interaction is performed.
        /// </summary>
        /// <param name="continueResume">The action to run when resuming is to be completed.</param>
        public abstract void RequestResume(Action continueResume);

        /// <summary>
        /// Invoked when the user requests to pause while the resume overlay is active.
        /// </summary>
        public abstract void CancelResume();
    }

    public class BeatmapInvalidForRulesetException : ArgumentException
    {
        public BeatmapInvalidForRulesetException(string text)
            : base(text)
        {
        }
    }
}
