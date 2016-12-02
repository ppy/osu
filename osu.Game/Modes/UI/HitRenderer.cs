//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.UI
{
    public abstract class HitRenderer : Container
    {
        public event Action<JudgementInfo> OnJudgement;

        public event Action OnAllJudged;

        protected void TriggerOnJudgement(JudgementInfo j)
        {
            OnJudgement?.Invoke(j);
            if (AllObjectsJudged)
                OnAllJudged?.Invoke();
        }

        protected Playfield Playfield;

        public bool AllObjectsJudged => Playfield.HitObjects.Children.First()?.Judgement.Result != null; //reverse depth sort means First() instead of Last().

        public IEnumerable<DrawableHitObject> DrawableObjects => Playfield.HitObjects.Children;
    }

    public abstract class HitRenderer<T> : HitRenderer
        where T : HitObject
    {
        private List<T> objects;

        public List<HitObject> Objects
        {
            set
            {
                objects = Convert(value);
                if (IsLoaded)
                    loadObjects();
            }
        }

        protected abstract Playfield CreatePlayfield();

        protected abstract HitObjectConverter<T> Converter { get; }

        protected virtual List<T> Convert(List<HitObject> objects) => Converter.Convert(objects);

        public HitRenderer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                Playfield = CreatePlayfield()
            };

            loadObjects();
        }

        private void loadObjects()
        {
            if (objects == null) return;
            foreach (T h in objects)
            {
                var drawableObject = GetVisualRepresentation(h);

                if (drawableObject == null) continue;

                drawableObject.OnJudgement += onJudgement;

                Playfield.Add(drawableObject);
            }
        }

        private void onJudgement(DrawableHitObject o, JudgementInfo j) => TriggerOnJudgement(j);

        protected abstract DrawableHitObject GetVisualRepresentation(T h);
    }
}
