// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public class FruitPulpFormation : PulpFormation
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            VisualRepresentation.BindValueChanged(setFormation, true);
        }

        private void setFormation(ValueChangedEvent<FruitVisualRepresentation> visualRepresentation)
        {
            Clear();

            switch (visualRepresentation.NewValue)
            {
                case FruitVisualRepresentation.Pear:
                    AddPulp(new Vector2(0, -0.33f), new Vector2(SMALL_PULP));
                    AddPulp(PositionAt(60, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    AddPulp(PositionAt(180, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    AddPulp(PositionAt(300, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    break;

                case FruitVisualRepresentation.Grape:
                    AddPulp(new Vector2(0, -0.25f), new Vector2(SMALL_PULP));
                    AddPulp(PositionAt(0, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    AddPulp(PositionAt(120, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    AddPulp(PositionAt(240, DISTANCE_FROM_CENTRE_3), new Vector2(LARGE_PULP_3));
                    break;

                case FruitVisualRepresentation.Pineapple:
                    AddPulp(new Vector2(0, -0.3f), new Vector2(SMALL_PULP));
                    AddPulp(PositionAt(45, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    AddPulp(PositionAt(135, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    AddPulp(PositionAt(225, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    AddPulp(PositionAt(315, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    break;

                case FruitVisualRepresentation.Raspberry:
                    AddPulp(new Vector2(0, -0.34f), new Vector2(SMALL_PULP));
                    AddPulp(PositionAt(0, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    AddPulp(PositionAt(90, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    AddPulp(PositionAt(180, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    AddPulp(PositionAt(270, DISTANCE_FROM_CENTRE_4), new Vector2(LARGE_PULP_4));
                    break;
            }
        }
    }
}
