﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public partial class TestSceneHoldNote : ManiaHitObjectTestScene
    {
        [Test]
        public void TestHoldNote()
        {
            AddToggleStep("toggle hitting", v =>
            {
                foreach (var holdNote in CreatedDrawables.SelectMany(d => d.ChildrenOfType<DrawableHoldNote>()))
                {
                    ((Bindable<bool>)holdNote.IsHitting).Value = v;
                }
            });
        }

        [Test]
        public void TestFadeOnMiss()
        {
            AddStep("miss tick", () =>
            {
                foreach (var holdNote in holdNotes)
                    holdNote.ChildrenOfType<DrawableHoldNoteHead>().First().MissForcefully();
            });
        }

        [Test]
        public void TestTailOrigin()
        {
            AddStep("set tail origin to regular", () =>
            {
                foreach (var holdNote in CreatedDrawables.SelectMany(d => d.ChildrenOfType<DrawableHoldNote>()))
                    holdNote.Tail.TailOrigin = HoldNoteTailOrigin.Regular;
            });
            AddStep("change direction to down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
            AddStep("change direction to up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
            AddStep("set tail origin to inverted", () =>
            {
                foreach (var holdNote in CreatedDrawables.SelectMany(d => d.ChildrenOfType<DrawableHoldNote>()))
                    holdNote.Tail.TailOrigin = HoldNoteTailOrigin.Inverted;
            });
            AddStep("change direction to down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
            AddStep("change direction to up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
        }

        private IEnumerable<DrawableHoldNote> holdNotes => CreatedDrawables.SelectMany(d => d.ChildrenOfType<DrawableHoldNote>());

        protected override DrawableManiaHitObject CreateHitObject()
        {
            var note = new HoldNote { Duration = 1000 };
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return new DrawableHoldNote(note);
        }
    }
}
