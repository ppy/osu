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

namespace osu.Game.Modes.UI
{
    public abstract class HitRenderer : Container
    {
        public event Action<JudgementInfo> OnJudgement;

        public event Action OnAllJudged;

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
        private List<TObject> objects;

        public PlayerInputManager InputManager;

        protected Playfield<TObject> Playfield;

        public override bool AllObjectsJudged => Playfield.HitObjects.Children.First()?.Judgement.Result != null; //reverse depth sort means First() instead of Last().

        public IEnumerable<DrawableHitObject> DrawableObjects => Playfield.HitObjects.Children;

        public Beatmap Beatmap
        {
            set
            {
                objects = Convert(value);
                if (IsLoaded)
                    loadObjects();
            }
        }

        protected abstract Playfield<TObject> CreatePlayfield();

        protected abstract HitObjectConverter<TObject> Converter { get; }

        protected virtual List<TObject> Convert(Beatmap beatmap) => Converter.Convert(beatmap);

        protected HitRenderer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Playfield = CreatePlayfield();
            Playfield.InputManager = InputManager;

            Add(Playfield);

            loadObjects();
        }

        private void loadObjects()
        {
            if (objects == null) return;
            foreach (TObject h in objects)
            {
                DrawableHitObject<TObject> drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null) continue;

                drawableObject.OnJudgement += onJudgement;

                Playfield.Add(drawableObject);
            }
            Playfield.PostProcess();
        }

        private void onJudgement(DrawableHitObject<TObject> o, JudgementInfo j) => TriggerOnJudgement(j);

        protected abstract DrawableHitObject<TObject> GetVisualRepresentation(TObject h);
    }
}
