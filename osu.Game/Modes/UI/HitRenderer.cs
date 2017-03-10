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
        /// The number of Judgements required to be triggered
        /// before the game enters post-play routines.
        /// </summary>
        protected abstract int JudgementCount { get; }

        /// <summary>
        /// The beatmap this HitRenderer is initialized with.
        /// </summary>
        protected readonly Beatmap Beatmap;

        private int maxJudgements;
        private int countJudgements;

        protected HitRenderer(Beatmap beatmap)
        {
            Beatmap = beatmap;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            maxJudgements = JudgementCount;
        }

        protected void TriggerOnJudgement(JudgementInfo j)
        {
            countJudgements++;

            OnJudgement?.Invoke(j);

            if (countJudgements == maxJudgements)
                OnAllJudged?.Invoke();
        }
    }

    public abstract class HitRenderer<TObject> : HitRenderer
        where TObject : HitObject
    {
        public override Func<Vector2, Vector2> MapPlayfieldToScreenSpace => Playfield.ScaledContent.ToScreenSpace;
        public IEnumerable<DrawableHitObject> DrawableObjects => Playfield.HitObjects.Children;

        protected abstract HitObjectConverter<TObject> Converter { get; }
        protected virtual List<TObject> Convert(Beatmap beatmap) => Converter.Convert(beatmap);

        protected override Container<Drawable> Content => content;

        private int judgementCount;
        protected override int JudgementCount => judgementCount;

        protected Playfield<TObject> Playfield;

        private Container content;

        protected HitRenderer(Beatmap beatmap)
            : base(beatmap)
        {
            RelativeSizeAxes = Axes.Both;

            InputManager.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    Playfield = CreatePlayfield(),
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
            foreach (TObject h in Convert(Beatmap))
            {
                DrawableHitObject<TObject> drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null)
                    continue;

                drawableObject.OnJudgement += onJudgement;

                Playfield.Add(drawableObject);

                judgementCount++;
            }

            Playfield.PostProcess();
        }

        private void onJudgement(DrawableHitObject<TObject> o, JudgementInfo j) => TriggerOnJudgement(j);

        protected abstract DrawableHitObject<TObject> GetVisualRepresentation(TObject h);
        protected abstract Playfield<TObject> CreatePlayfield();
    }
}
