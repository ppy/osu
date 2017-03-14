// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Screens.Play;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Modes.UI
{
    public abstract class HitRenderer : Container
    {
        public event Action<JudgementInfo> OnJudgement;
        public event Action OnAllJudged;

        internal readonly PlayerInputManager InputManager = new PlayerInputManager();

        /// <summary>
        /// A function to convert coordinates from gamefield to screen space.
        /// </summary>
        public abstract Func<Vector2, Vector2> MapPlayfieldToScreenSpace { get; }

        /// <summary>
        /// Whether all the HitObjects have been judged.
        /// </summary>
        protected abstract bool AllObjectsJudged { get; }

        protected void TriggerOnJudgement(JudgementInfo j)
        {
            OnJudgement?.Invoke(j);

            if (AllObjectsJudged)
                OnAllJudged?.Invoke();
        }
    }

    public abstract class HitRenderer<TObject> : HitRenderer
        where TObject : HitObject
    {
        internal readonly KeyConversionInputManager KeyConversionInputManager;

        public override Func<Vector2, Vector2> MapPlayfieldToScreenSpace => Playfield.ScaledContent.ToScreenSpace;
        public IEnumerable<DrawableHitObject> DrawableObjects => Playfield.HitObjects.Children;

        protected override Container<Drawable> Content => content;
        protected override bool AllObjectsJudged => Playfield.HitObjects.Children.All(h => h.Judgement.Result.HasValue);

        protected Playfield<TObject> Playfield;
        protected Beatmap<TObject> Beatmap;

        private Container content;

        protected HitRenderer(Beatmap beatmap)
        {
            Beatmap = CreateBeatmapConverter().Convert(beatmap);

            RelativeSizeAxes = Axes.Both;

            KeyConversionInputManager = CreateKeyConversionInputManager();
            KeyConversionInputManager.RelativeSizeAxes = Axes.Both;
            KeyConversionInputManager.Add(Playfield = CreatePlayfield());

            InputManager.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    KeyConversionInputManager
                }
            });

            AddInternal(InputManager);
        }


        [BackgroundDependencyLoader]
        private void load()
        {
            loadObjects();
        }

        private void loadObjects()
        {
            foreach (TObject h in Beatmap.HitObjects)
            {
                DrawableHitObject<TObject> drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null)
                    continue;

                drawableObject.OnJudgement += onJudgement;

                Playfield.Add(drawableObject);
            }

            Playfield.PostProcess();
        }

        private void onJudgement(DrawableHitObject<TObject> o, JudgementInfo j) => TriggerOnJudgement(j);

        protected abstract DrawableHitObject<TObject> GetVisualRepresentation(TObject h);
        protected abstract Playfield<TObject> CreatePlayfield();
        protected abstract IBeatmapConverter<TObject> CreateBeatmapConverter();
        protected virtual KeyConversionInputManager CreateKeyConversionInputManager() => new KeyConversionInputManager();
    }
}
