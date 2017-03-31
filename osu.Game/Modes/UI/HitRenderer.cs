// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Modes.Judgements;
using osu.Game.Modes.Mods;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Screens.Play;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Modes.Replays;
using osu.Game.Modes.Scoring;

namespace osu.Game.Modes.UI
{
    /// <summary>
    /// Base HitRenderer. Doesn't hold objects.
    /// <para>
    /// Should not be derived - derive <see cref="HitRenderer{TObject, TJudgement}"/> instead.
    /// </para>
    /// </summary>
    public abstract class HitRenderer : Container
    {
        /// <summary>
        /// Invoked when all the judgeable HitObjects have been judged.
        /// </summary>
        public event Action OnAllJudged;

        /// <summary>
        /// The input manager for this HitRenderer.
        /// </summary>
        internal readonly PlayerInputManager InputManager = new PlayerInputManager();

        /// <summary>
        /// The key conversion input manager for this HitRenderer.
        /// </summary>
        protected readonly KeyConversionInputManager KeyConversionInputManager;

        /// <summary>
        /// Whether all the HitObjects have been judged.
        /// </summary>
        protected abstract bool AllObjectsJudged { get; }

        protected HitRenderer()
        {
            KeyConversionInputManager = CreateKeyConversionInputManager();
            KeyConversionInputManager.RelativeSizeAxes = Axes.Both;
        }

        /// <summary>
        /// Checks whether all HitObjects have been judged, and invokes OnAllJudged.
        /// </summary>
        protected void CheckAllJudged()
        {
            if (AllObjectsJudged)
                OnAllJudged?.Invoke();
        }

        public abstract ScoreProcessor CreateScoreProcessor();

        /// <summary>
        /// Creates a key conversion input manager.
        /// </summary>
        /// <returns>The input manager.</returns>
        protected virtual KeyConversionInputManager CreateKeyConversionInputManager() => new KeyConversionInputManager();

        protected virtual FramedReplayInputHandler CreateReplayInputHandler(Replay replay) => new FramedReplayInputHandler(replay);

        /// <summary>
        /// Sets a replay to be used, overriding local input.
        /// </summary>
        /// <param name="replay">The replay, null for local input.</param>
        public void SetReplay(Replay replay) => InputManager.ReplayInputHandler = replay != null ? CreateReplayInputHandler(replay) : null;
    }

    /// <summary>
    /// HitRenderer that applies conversion to Beatmaps. Does not contain a Playfield
    /// and does not load drawable hit objects.
    /// <para>
    /// Should not be derived - derive <see cref="HitRenderer{TObject, TJudgement}"/> instead.
    /// </para>
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained by this HitRenderer.</typeparam>
    public abstract class HitRenderer<TObject> : HitRenderer
        where TObject : HitObject
    {
        /// <summary>
        /// The Beatmap 
        /// </summary>
        public Beatmap<TObject> Beatmap;

        protected HitRenderer(WorkingBeatmap beatmap)
        {
            Debug.Assert(beatmap != null, "HitRenderer initialized with a null beatmap.");

            RelativeSizeAxes = Axes.Both;

            IBeatmapConverter<TObject> converter = CreateBeatmapConverter();
            IBeatmapProcessor<TObject> processor = CreateBeatmapProcessor();

            // Convert the beatmap
            Beatmap = converter.Convert(beatmap.Beatmap);

            // Apply defaults
            foreach (var h in Beatmap.HitObjects)
                h.ApplyDefaults(Beatmap.TimingInfo, Beatmap.BeatmapInfo.Difficulty);

            // Post-process the beatmap
            processor.PostProcess(Beatmap);

            // Add mods, should always be the last thing applied to give full control to mods
            applyMods(beatmap.Mods.Value);
        }


        /// <summary>
        /// Applies the active mods to this HitRenderer.
        /// </summary>
        /// <param name="mods"></param>
        private void applyMods(IEnumerable<Mod> mods)
        {
            if (mods == null)
                return;

            foreach (var mod in mods.OfType<IApplicableMod<TObject>>())
                mod.Apply(this);
        }

        /// <summary>
        /// Creates a converter to convert Beatmap to a specific mode.
        /// </summary>
        /// <returns>The Beatmap converter.</returns>
        protected abstract IBeatmapConverter<TObject> CreateBeatmapConverter();

        /// <summary>
        /// Creates a processor to perform post-processing operations
        /// on HitObjects in converted Beatmaps.
        /// </summary>
        /// <returns>The Beatmap processor.</returns>
        protected abstract IBeatmapProcessor<TObject> CreateBeatmapProcessor();
    }

    /// <summary>
    /// A derivable HitRenderer that manages the Playfield and HitObjects.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained by this HitRenderer.</typeparam>
    /// <typeparam name="TJudgement">The type of Judgement of DrawableHitObjects contained by this HitRenderer.</typeparam>
    public abstract class HitRenderer<TObject, TJudgement> : HitRenderer<TObject>
        where TObject : HitObject
        where TJudgement : Judgement
    {
        public event Action<TJudgement> OnJudgement;

        protected override Container<Drawable> Content => content;
        protected override bool AllObjectsJudged => Playfield.HitObjects.Children.All(h => h.Judgement.Result != HitResult.None);

        /// <summary>
        /// The playfield.
        /// </summary>
        protected Playfield<TObject, TJudgement> Playfield;

        private readonly Container content;

        protected HitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
            KeyConversionInputManager.Add(Playfield = CreatePlayfield());

            InputManager.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[] { KeyConversionInputManager }
            });

            AddInternal(InputManager);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadObjects();

            if (InputManager?.ReplayInputHandler != null)
                InputManager.ReplayInputHandler.ToScreenSpace = Playfield.ScaledContent.ToScreenSpace;
        }

        private void loadObjects()
        {
            foreach (TObject h in Beatmap.HitObjects)
            {
                var drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null)
                    continue;

                drawableObject.OnJudgement += onJudgement;

                Playfield.Add(drawableObject);
            }

            Playfield.PostProcess();
        }

        /// <summary>
        /// Triggered when an object's Judgement is updated.
        /// </summary>
        /// <param name="judgedObject">The object that Judgement has been updated for.</param>
        private void onJudgement(DrawableHitObject<TObject, TJudgement> judgedObject)
        {
            Playfield.OnJudgement(judgedObject);

            OnJudgement?.Invoke(judgedObject.Judgement);

            CheckAllJudged();
        }

        /// <summary>
        /// Creates a DrawableHitObject from a HitObject.
        /// </summary>
        /// <param name="h">The HitObject to make drawable.</param>
        /// <returns>The DrawableHitObject.</returns>
        protected abstract DrawableHitObject<TObject, TJudgement> GetVisualRepresentation(TObject h);

        /// <summary>
        /// Creates a Playfield.
        /// </summary>
        /// <returns>The Playfield.</returns>
        protected abstract Playfield<TObject, TJudgement> CreatePlayfield();
    }
}
