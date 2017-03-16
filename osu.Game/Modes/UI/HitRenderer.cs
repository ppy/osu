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

            // Convert + process the beatmap
            Beatmap = CreateBeatmapConverter().Convert(beatmap.Beatmap);
            Beatmap.HitObjects.ForEach(h => CreateBeatmapProcessor().SetDefaults(h, Beatmap));
            CreateBeatmapProcessor().PostProcess(Beatmap);

            applyMods(beatmap.Mods.Value);

            RelativeSizeAxes = Axes.Both;
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
        where TJudgement : JudgementInfo
    {
        public event Action<TJudgement> OnJudgement;

        protected override Container<Drawable> Content => content;
        protected override bool AllObjectsJudged => Playfield.HitObjects.Children.All(h => h.Judgement.Result.HasValue);

        /// <summary>
        /// The playfield.
        /// </summary>
        protected Playfield<TObject, TJudgement> Playfield;

        private Container content;

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
            OnJudgement?.Invoke(judgedObject.Judgement);
            Playfield.OnJudgement(judgedObject);

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
