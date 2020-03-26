// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneFailingLayer : OsuTestScene
    {
        private readonly FailingLayer layer;

        public TestSceneFailingLayer()
        {
            Child = layer = new FailingLayer();
        }

        [Test]
        public void TestLayerFading()
        {
            AddSliderStep("current health", 0.0, 1.0, 1.0, val =>
            {
                layer.Current.Value = val;
            });

            AddStep("set health to 0.10", () => layer.Current.Value = 0.10);
            AddWaitStep("wait for fade to finish", 5);
            AddStep("set health to 1", () => layer.Current.Value = 1f);
        }
    }
}
