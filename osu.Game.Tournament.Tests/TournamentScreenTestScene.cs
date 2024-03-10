// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Tournament.Tests
{
    public abstract partial class TournamentScreenTestScene : TournamentTestScene
    {
        protected override Container<Drawable> Content { get; } = new TournamentScalingContainer();

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.Add(Content);
        }

        private partial class TournamentScalingContainer : DrawSizePreservingFillContainer
        {
            public TournamentScalingContainer()
            {
                TargetDrawSize = new Vector2(1024, 768);
                RelativeSizeAxes = Axes.Both;
            }

            protected override void Update()
            {
                base.Update();

                Scale = new Vector2(Math.Min(1, Content.DrawWidth / (1920 + TournamentSceneManager.CONTROL_AREA_WIDTH)));
            }
        }
    }
}
