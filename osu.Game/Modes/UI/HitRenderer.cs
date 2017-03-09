﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;
using OpenTK;

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

        public abstract bool AllObjectsJudged { get; }

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
        protected Playfield<TObject> Playfield;

        public override Func<Vector2, Vector2> MapPlayfieldToScreenSpace => Playfield.ScaledContent.ToScreenSpace;

        public override bool AllObjectsJudged => Playfield.HitObjects.Children.All(h => h.Judgement.Result.HasValue);

        protected override Container<Drawable> Content => content;

        protected abstract HitObjectConverter<TObject> Converter { get; }

        protected virtual List<TObject> Convert(Beatmap beatmap) => Converter.Convert(beatmap);

        private Container content;

        private Beatmap beatmap;

        protected HitRenderer(Beatmap beatmap)
        {
            this.beatmap = beatmap;

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

        protected abstract Playfield<TObject> CreatePlayfield();

        [BackgroundDependencyLoader]
        private void load()
        {
            loadObjects();
        }

        private void loadObjects()
        {
            foreach (TObject h in Convert(beatmap))
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
    }
}
