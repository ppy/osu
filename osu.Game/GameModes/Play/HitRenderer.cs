﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Objects;
using osu.Framework;
using System;

namespace osu.Game.GameModes.Play
{
    public abstract class HitRenderer : Container
    {
        public Action<HitObject> OnHit;
        public Action<HitObject> OnMiss;
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

        private Playfield playfield;

        protected abstract Playfield CreatePlayfield();

        protected abstract HitObjectConverter<T> Converter { get; }

        protected virtual List<T> Convert(List<HitObject> objects) => Converter.Convert(objects);

        public override void Load(BaseGame game)
        {
            base.Load(game);

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                playfield = CreatePlayfield()
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

                drawableObject.OnHit = onHit;
                drawableObject.OnMiss = onMiss;

                playfield.Add(drawableObject);
            }
        }

        private void onMiss(DrawableHitObject obj)
        {
            OnMiss?.Invoke(obj.HitObject);
        }

        private void onHit(DrawableHitObject obj)
        {
            OnHit?.Invoke(obj.HitObject);
        }

        protected abstract DrawableHitObject GetVisualRepresentation(T h);
    }
}
