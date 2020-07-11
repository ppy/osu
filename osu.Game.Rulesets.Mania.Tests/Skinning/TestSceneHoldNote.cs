// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneHoldNote : ManiaHitObjectTestScene
    {
        public TestSceneHoldNote()
        {
            AddToggleStep("toggle hitting", v =>
            {
                foreach (var holdNote in CreatedDrawables.SelectMany(d => d.ChildrenOfType<DrawableHoldNote>()))
                {
                    ((Bindable<bool>)holdNote.IsHitting).Value = v;
                }
            });
        }

        protected override DrawableManiaHitObject CreateHitObject()
        {
            var note = new HoldNote { Duration = 1000 };
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return new DrawableHoldNote(note);
        }
    }
}
