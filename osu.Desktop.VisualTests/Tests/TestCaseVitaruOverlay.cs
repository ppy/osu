using osu.Framework.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes;
using osu.Game.Modes.Vitaru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseVitaruOverlay : TestCase
    {
        public override string Name => "Vitaru Overlay";
        public override string Description => "Vitaru Overlay";

        private PercentageCounter energy;

        public override void Reset()
        {
            base.Reset();

            Ruleset rule = new VitaruRuleset();

            Children = new Drawable[]
            {
                energy = rule.CreateScoreOverlay().AccuracyCounter,
                rule.CreateScoreOverlay().ComboCounter,
                rule.CreateScoreOverlay().KeyCounter,
                rule.CreateScoreOverlay().ScoreCounter,
            };
            AddButton("10% acc", () => addAccuracy(0.10f));
            AddButton("-10% acc", () => addAccuracy(-0.10f));
        }

        private void addAccuracy(float accuracy)
        {
            energy.Set((float)Math.Round(energy.Count + accuracy,1));
        }
    }
}
