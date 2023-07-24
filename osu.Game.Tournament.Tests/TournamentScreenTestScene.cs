// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Tournament.Tests
{
    public abstract partial class TournamentScreenTestScene : TournamentTestScene
    {
        protected override Container<Drawable> Content { get; } = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new Vector2(1920, 1080),
            Scale = new Vector2(1920 / (float)(1920 + TournamentSceneManager.CONTROL_AREA_WIDTH)),
            RelativeSizeAxes = Axes.Both
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.Add(Content);
        }
    }
}
