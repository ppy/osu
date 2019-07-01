// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Components.RadioButtons;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestSceneEditorComposeRadioButtons : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DrawableRadioButton) };

        public TestSceneEditorComposeRadioButtons()
        {
            RadioButtonCollection collection;
            Add(collection = new RadioButtonCollection
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 150,
                Items = new[]
                {
                    new RadioButton("Item 1", () => { }),
                    new RadioButton("Item 2", () => { }),
                    new RadioButton("Item 3", () => { }),
                    new RadioButton("Item 4", () => { }),
                    new RadioButton("Item 5", () => { })
                }
            });

            for (int i = 0; i < collection.Items.Count; i++)
            {
                int l = i;
                AddStep($"Select item {l + 1}", () => collection.Items[l].Select());
                AddStep($"Deselect item {l + 1}", () => collection.Items[l].Deselect());
            }
        }
    }
}
