// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Cursor;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneFormSampleSet : ThemeComparisonTestScene
    {
        public TestSceneFormSampleSet()
            : base(false)
        {
        }

        protected override Drawable CreateContent() => new PopoverContainer
        {
            RelativeSizeAxes = Axes.Both,
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FormSampleSet
                {
                    Current =
                    {
                        Value = new EditorBeatmapSkin.SampleSet(3, "Custom set #3")
                        {
                            Filenames = ["normal-hitwhistle3.wav"]
                        }
                    },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.4f,
                }
            }
        };
    }
}
