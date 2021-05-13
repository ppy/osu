// using System;
// using System.Collections.Generic;
// using System.Linq;
// using osu.Game.Rulesets.Difficulty.Preprocessing;
// using osu.Game.Rulesets.Difficulty.Skills;
// using osu.Game.Rulesets.Osu.Objects;
// using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
//
// namespace osu.Game.Rulesets.Osu.Difficulty.Skills
// {
//     public class Accuracy : Skill
//     {
//         // Fake skill alert
//         protected override double SkillMultiplier => 0;
//         protected override double StrainDecayBase => 0;
//         protected override double StrainValueOf(DifficultyHitObject current) => 0;
//
//         private int circleCount;
//         private int noteIndex;
//         private bool isPreviousOffbeat;
//         private readonly List<int> previousDoubles = new List<int>();
//         private double difficultyTotal;
//
//         public new void Process(DifficultyHitObject current)
//         {
//             var osuCurrent = (OsuDifficultyHitObject)current;
//
//             if (current.BaseObject is HitCircle)
//             {
//                 difficultyTotal += calculateRhythmBonus(osuCurrent);
//                 circleCount++;
//             }
//             else
//                 isPreviousOffbeat = false;
//
//             noteIndex++;
//
//             Previous.Push(current);
//         }
//
//         public new double DifficultyValue()
//         {
//             double lengthRequirement = Math.Tanh(circleCount / 50.0);
//             return 1 + difficultyTotal / circleCount * lengthRequirement;
//         }
//
//         private double calculateRhythmBonus(OsuDifficultyHitObject current)
//         {
//             double rhythmBonus = 0.05 * current.Flow;
//
//             if (Previous.Count == 0)
//                 return rhythmBonus;
//
//             if (Previous[0].BaseObject is HitCircle)
//                 rhythmBonus += calculateCircleToCircleRhythmBonus(current);
//             else if (Previous[0].BaseObject is Slider)
//                 rhythmBonus += calculateSliderToCircleRhythmBonus(current);
//             else if (Previous[0].BaseObject is Spinner)
//                 isPreviousOffbeat = false;
//
//             return rhythmBonus;
//         }
//
//         private double calculateCircleToCircleRhythmBonus(OsuDifficultyHitObject current)
//         {
//             var previous = (OsuDifficultyHitObject)Previous[0];
//             double rhythmBonus = 0;
//
//             if (isPreviousOffbeat && Utils.IsRatioEqualGreater(1.5, current.GapTime, previous.GapTime))
//             {
//                 rhythmBonus = 5; // Doubles, Quads etc.
//                 foreach (var previousDouble in previousDoubles.Skip(Math.Max(0, previousDoubles.Count - 10)))
//                 {
//                     if (previousDouble > 0) // -1 is used to mark 1/3s
//                         rhythmBonus *= 1 - 0.5 * Math.Pow(0.9, noteIndex - previousDouble); // Reduce the value of repeated doubles.
//                     else
//                         rhythmBonus = 5;
//                 }
//
//                 previousDoubles.Add(noteIndex);
//             }
//             else if (Utils.IsRatioEqual(0.667, current.GapTime, previous.GapTime))
//             {
//                 rhythmBonus = 4 + 8 * current.Flow; // Transition to 1/3s
//                 if (current.Flow > 0.8)
//                     previousDoubles.Add(-1);
//             }
//             else if (Utils.IsRatioEqual(0.333, current.GapTime, previous.GapTime))
//                 rhythmBonus = 0.4 + 0.8 * current.Flow; // Transition to 1/6s
//             else if (Utils.IsRatioEqual(0.5, current.GapTime, previous.GapTime) || Utils.IsRatioEqualLess(0.25, current.GapTime, previous.GapTime))
//                 rhythmBonus = 0.1 + 0.2 * current.Flow; // Transition to triples, streams etc.
//
//             if (Utils.IsRatioEqualLess(0.667, current.GapTime, previous.GapTime) && current.Flow > 0.8)
//                 isPreviousOffbeat = true;
//             else if (Utils.IsRatioEqual(1, current.GapTime, previous.GapTime) && current.Flow > 0.8)
//                 isPreviousOffbeat = !isPreviousOffbeat;
//             else
//                 isPreviousOffbeat = false;
//
//             return rhythmBonus;
//         }
//
//         private double calculateSliderToCircleRhythmBonus(OsuDifficultyHitObject current)
//         {
//             double rhythmBonus = 0;
//             double sliderMS = current.StrainTime - current.GapTime;
//
//             if (Utils.IsRatioEqual(0.5, current.GapTime, sliderMS) || Utils.IsRatioEqual(0.25, current.GapTime, sliderMS))
//             {
//                 double endFlow = calculateSliderEndFlow(current);
//                 rhythmBonus = 0.3 * endFlow; // Triples, streams etc. starting with a slider end.
//
//                 if (endFlow > 0.8)
//                     isPreviousOffbeat = true;
//                 else
//                     isPreviousOffbeat = false;
//             }
//             else
//                 isPreviousOffbeat = false;
//
//             return rhythmBonus;
//         }
//
//         private static double calculateSliderEndFlow(OsuDifficultyHitObject current)
//         {
//             double streamBpm = 15000 / current.GapTime;
//             double isFlowSpeed = Utils.TransitionToTrue(streamBpm, 120, 30);
//
//             double distanceOffset = (Math.Tanh((streamBpm - 140) / 20) + 2) * OsuDifficultyHitObject.NORMALIZED_RADIUS;
//             double isFlowDistance = Utils.TransitionToFalse(current.JumpDistance, distanceOffset, OsuDifficultyHitObject.NORMALIZED_RADIUS);
//
//             return isFlowSpeed * isFlowDistance;
//         }
//     }
// }