﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.IO;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboard : Container<DrawableStoryboardLayer>
    {
        public Storyboard Storyboard { get; private set; }

        private readonly Background background;
        public Texture BackgroundTexture
        {
            get { return background.Texture; }
            set { background.Texture = value; }
        }

        private readonly Container<DrawableStoryboardLayer> content;
        protected override Container<DrawableStoryboardLayer> Content => content;

        protected override Vector2 DrawScale => new Vector2(Parent.DrawHeight / 480);
        public override bool HandleInput => false;

        private bool passing = true;
        public bool Passing
        {
            get { return passing; }
            set
            {
                if (passing == value) return;
                passing = value;
                updateLayerVisibility();
            }
        }

        private DependencyContainer dependencies;
        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        public DrawableStoryboard(Storyboard storyboard)
        {
            Storyboard = storyboard;
            Size = new Vector2(640, 480);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(background = new Background
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
            AddInternal(content = new Container<DrawableStoryboardLayer>
            {
                Size = new Vector2(640, 480),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load(FileStore fileStore)
        {
            dependencies.Cache(new TextureStore(new RawTextureLoaderStore(fileStore.Store), false) { ScaleAdjust = 1, });

            foreach (var layer in Storyboard.Layers)
                Add(layer.CreateDrawable());
        }

        private void updateLayerVisibility()
        {
            foreach (var layer in Children)
                layer.Enabled = passing ? layer.Layer.EnabledWhenPassing : layer.Layer.EnabledWhenFailing;
        }

        private class Background : Sprite
        {
            protected override Vector2 DrawScale => Texture != null ? new Vector2(Parent.DrawHeight / Texture.DisplayHeight) : base.DrawScale;
        }
    }
}
