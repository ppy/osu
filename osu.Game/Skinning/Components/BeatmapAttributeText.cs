// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class BeatmapAttributeText : FontAdjustableSkinComponent
    {
        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute))]
        public Bindable<BeatmapAttribute> Attribute { get; } = new Bindable<BeatmapAttribute>(BeatmapAttribute.StarRating);

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private readonly OsuSpriteText text;
        private IBindable<StarDifficulty>? difficultyBindable;
        private CancellationTokenSource? difficultyCancellationSource;
        private ModSettingChangeTracker? modSettingTracker;
        private StarDifficulty? starDifficulty;

        public BeatmapAttributeText()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => updateText());
            Template.BindValueChanged(_ => updateText());

            beatmap.BindValueChanged(b =>
            {
                difficultyCancellationSource?.Cancel();
                difficultyCancellationSource = new CancellationTokenSource();

                difficultyBindable?.UnbindAll();
                difficultyBindable = difficultyCache.GetBindableDifficulty(b.NewValue.BeatmapInfo, difficultyCancellationSource.Token);
                difficultyBindable.BindValueChanged(d =>
                {
                    starDifficulty = d.NewValue;
                    updateText();
                });

                updateText();
            }, true);

            mods.BindValueChanged(m =>
            {
                modSettingTracker?.Dispose();
                modSettingTracker = new ModSettingChangeTracker(m.NewValue)
                {
                    SettingChanged = _ => updateText()
                };

                updateText();
            }, true);

            ruleset.BindValueChanged(_ => updateText());

            updateText();
        }

        private void updateText()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", "{1}");

            List<object?> values = new List<object?>
            {
                getLabelString(Attribute.Value),
                getValueString(Attribute.Value)
            };

            foreach (var type in Enum.GetValues<BeatmapAttribute>())
            {
                string replaced = numberedTemplate.Replace($@"{{{{{type}}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(getValueString(type));
                }
            }

            if (anyBracketsContainOperations(numberedTemplate))
            {
                int[] bracketStartPositions = getOperatorContainingBracketPos(numberedTemplate);

                for (int i = 0; i < bracketStartPositions.Length; i++)
                {
                    string calculatedTemplate = numberedTemplate;
                    int bracketEndPosition = calculatedTemplate.IndexOf('}', bracketStartPositions[i]);
                    //Ensure that operators, and round brackets are correct
                    //otherwise it won't be calculated
                    if (!checkOperatorCorrectness(calculatedTemplate, bracketStartPositions[i], bracketEndPosition)
                        || !checkRoundBracketCorrectness(calculatedTemplate, bracketStartPositions[i], bracketEndPosition))
                        continue;

                    replaceMinus(ref calculatedTemplate, bracketStartPositions[i], bracketEndPosition);
                    addImpliedMultiplication(ref calculatedTemplate, bracketStartPositions[i], bracketEndPosition);
                    //Recalculate endPosition since both of the above function add new characters,
                    //therefore offsetting things.
                    bracketEndPosition = calculatedTemplate.IndexOf('}', bracketStartPositions[i]);
                    int operatorCount = 0;

                    for (int j = bracketStartPositions[i]; j < bracketEndPosition; j++)
                    {
                        if (mathoperators.Contains(calculatedTemplate[j]))
                            operatorCount++;
                    }

                    string[] variableTexts = new string[operatorCount + 1];
                    string[] operatorSymbols = new string[operatorCount];
                    int[] operatorPriority = new int[operatorCount];
                    //Get the inside of brackets
                    variableTexts[0] = calculatedTemplate.Substring(bracketStartPositions[i] + 1, bracketEndPosition - bracketStartPositions[i] - 1);
                    //Split the text up into operators and variables
                    splitUpOperatorText(ref variableTexts, ref operatorSymbols, ref operatorPriority);
                    //Convert variables, values to double and do math calculation
                    double result = doConversionCalculation(variableTexts, operatorSymbols, operatorPriority);
                    if (double.IsNaN(result))
                        continue;

                    values.Add(result.ToLocalisableString(@"0.##"));
                    //Set endposition again to ensure that it replaces the right substring.
                    bracketEndPosition = numberedTemplate.IndexOf('}', bracketStartPositions[i]);
                    //Since we replace characters with a single number we have to also offset
                    //the next positions by the amount of characters replaced.
                    int changeInLength = numberedTemplate.Length;
                    numberedTemplate = string.Concat(numberedTemplate.Substring(0, bracketStartPositions[i]), values.Count - 1, numberedTemplate.Substring(bracketEndPosition + 1, numberedTemplate.Length - bracketEndPosition - 1));
                    changeInLength -= numberedTemplate.Length;
                    if (i == bracketStartPositions.Length - 1)
                        continue;

                    for (int j = i + 1; j < bracketStartPositions.Length; ++j)
                    {
                        bracketStartPositions[j] -= changeInLength;
                    }
                }
            }

            text.Text = LocalisableString.Format(numberedTemplate, values.ToArray());
        }

        private LocalisableString getLabelString(BeatmapAttribute attribute)
        {
            switch (attribute)
            {
                case BeatmapAttribute.CircleSize:
                    return BeatmapsetsStrings.ShowStatsCs;

                case BeatmapAttribute.Accuracy:
                    return BeatmapsetsStrings.ShowStatsAccuracy;

                case BeatmapAttribute.HPDrain:
                    return BeatmapsetsStrings.ShowStatsDrain;

                case BeatmapAttribute.ApproachRate:
                    return BeatmapsetsStrings.ShowStatsAr;

                case BeatmapAttribute.StarRating:
                    return BeatmapsetsStrings.ShowStatsStars;

                case BeatmapAttribute.Title:
                    return EditorSetupStrings.Title;

                case BeatmapAttribute.Artist:
                    return EditorSetupStrings.Artist;

                case BeatmapAttribute.DifficultyName:
                    return EditorSetupStrings.DifficultyHeader;

                case BeatmapAttribute.Creator:
                    return EditorSetupStrings.Creator;

                case BeatmapAttribute.Source:
                    return EditorSetupStrings.Source;

                case BeatmapAttribute.Length:
                    return ArtistStrings.TracklistLength.ToTitle();

                case BeatmapAttribute.RankedStatus:
                    return BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault;

                case BeatmapAttribute.BPM:
                    return BeatmapsetsStrings.ShowStatsBpm;

                case BeatmapAttribute.MaxPP:
                    return BeatmapAttributeTextStrings.MaxPP;

                default:
                    return string.Empty;
            }
        }

        private LocalisableString getValueString(BeatmapAttribute attribute)
        {
            switch (attribute)
            {
                case BeatmapAttribute.Title:
                    return new RomanisableString(beatmap.Value.BeatmapInfo.Metadata.TitleUnicode, beatmap.Value.BeatmapInfo.Metadata.Title);

                case BeatmapAttribute.Artist:
                    return new RomanisableString(beatmap.Value.BeatmapInfo.Metadata.ArtistUnicode, beatmap.Value.BeatmapInfo.Metadata.Artist);

                case BeatmapAttribute.DifficultyName:
                    return beatmap.Value.BeatmapInfo.DifficultyName;

                case BeatmapAttribute.Creator:
                    return beatmap.Value.BeatmapInfo.Metadata.Author.Username;

                case BeatmapAttribute.Source:
                    return beatmap.Value.BeatmapInfo.Metadata.Source;

                case BeatmapAttribute.Length:
                    return Math.Round(beatmap.Value.BeatmapInfo.Length / ModUtils.CalculateRateWithMods(mods.Value)).ToFormattedDuration();

                case BeatmapAttribute.RankedStatus:
                    return beatmap.Value.BeatmapInfo.Status.GetLocalisableDescription();

                case BeatmapAttribute.BPM:
                    return FormatUtils.RoundBPM(beatmap.Value.BeatmapInfo.BPM, ModUtils.CalculateRateWithMods(mods.Value)).ToLocalisableString(@"0.##");

                case BeatmapAttribute.CircleSize:
                    return computeDifficulty().CircleSize.ToLocalisableString(@"0.##");

                case BeatmapAttribute.HPDrain:
                    return computeDifficulty().DrainRate.ToLocalisableString(@"0.##");

                case BeatmapAttribute.Accuracy:
                    return computeDifficulty().OverallDifficulty.ToLocalisableString(@"0.##");

                case BeatmapAttribute.ApproachRate:
                    return computeDifficulty().ApproachRate.ToLocalisableString(@"0.##");

                case BeatmapAttribute.StarRating:
                    return (starDifficulty?.Stars ?? 0).FormatStarRating();

                case BeatmapAttribute.MaxPP:
                    return Math.Round(starDifficulty?.PerformanceAttributes?.Total ?? 0, MidpointRounding.AwayFromZero).ToLocalisableString();

                default:
                    return string.Empty;
            }

            BeatmapDifficulty computeDifficulty()
            {
                return ruleset.Value is RulesetInfo rulesetInfo
                    ? rulesetInfo.CreateInstance().GetAdjustedDisplayDifficulty(beatmap.Value.BeatmapInfo, mods.Value)
                    : new BeatmapDifficulty(beatmap.Value.BeatmapInfo.Difficulty);
            }
        }

        private const string mathoperators = "+-*/%";

        private bool anyBracketsContainOperations(string input)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                //If there are brackets inside of brackets then only the most inside
                //one is considered.
                //Get to opening bracket
                int openingBracketPos = input.IndexOf('{', i);
                if (openingBracketPos == -1)
                    return false;

                int closingBracketPos = input.IndexOf('}', openingBracketPos);
                if (closingBracketPos == -1)
                    return false;

                int openingResult = input.IndexOf('{', openingBracketPos);
                if (openingResult == -1)
                    return false;

                //For consistent behaviour we only calculate the most inside brackets
                //For example: if you write { {CircleSize} } then the other ones remain
                //intact. This while loop looks for such openings.
                while (openingResult < closingBracketPos && openingResult != -1)
                {
                    openingResult = input.IndexOf('{', openingBracketPos + 1);
                    if (openingResult != -1 && openingResult < closingBracketPos)
                        openingBracketPos = openingResult;
                }

                //We only check the insides of brackets, not the outside.
                string bracketInside = input.Substring(openingBracketPos, closingBracketPos - openingBracketPos - 1);
                i = closingBracketPos;

                for (int j = 0; j < mathoperators.Length; ++j)
                {
                    if (bracketInside.Contains(mathoperators[j]))
                        return true;
                }

                //Due to implied multiplication, brackets can be "valid" operators
                //though only if there is anything besides these brackets
                if ((bracketInside.Contains('(') || bracketInside.Contains(')'))
                    && bracketInside.Replace("(", "").Replace(")", "").Length > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the operator containing brackets.
        /// </summary>
        /// <param name="input">The string containing operators, brackets, values</param>
        /// <returns>Returns the position of the opening brackets of brackets which contain operators.</returns>
        private int[] getOperatorContainingBracketPos(string input)
        {
            List<int> openingBracketPositions = new List<int>();

            for (int i = 0; i < input.Length; ++i)
            {
                //If there are brackets inside of brackets then only the most inside
                //one is considered.
                //Get to opening bracket
                int openingBracketPos = input.IndexOf('{', i);
                if (openingBracketPos == -1)
                    break;

                int closingBracketPos = input.IndexOf('}', openingBracketPos);
                if (closingBracketPos == -1)
                    break;

                int openingResult = input.IndexOf('{', openingBracketPos);
                if (openingResult == -1)
                    break;

                //For consistent behaviour we only calculate the most inside brackets
                //For example: if you write { {CircleSize} } then the other ones remain
                //intact. This while loop looks for such openings.
                while (openingResult < closingBracketPos && openingResult != -1)
                {
                    openingResult = input.IndexOf('{', openingBracketPos + 1);
                    if (openingResult != -1 && openingResult < closingBracketPos)
                        openingBracketPos = openingResult;
                }

                //We only check the insides of brackets to not get false positives
                string bracketInside = input.Substring(openingBracketPos, closingBracketPos - openingBracketPos - 1);
                i = closingBracketPos;

                for (int j = 0; j < mathoperators.Length; ++j)
                {
                    if (bracketInside.Contains(mathoperators[j]))
                    {
                        openingBracketPositions.Add(openingBracketPos);
                        break;
                    }
                }

                if ((bracketInside.Contains('(') || bracketInside.Contains(')'))
                    && !openingBracketPositions.Contains(openingBracketPos))
                    openingBracketPositions.Add(openingBracketPos);
            }

            return openingBracketPositions.ToArray();
        }

        /// <summary>
        /// Checks if the operators inside of specific brackets have values between them.
        /// </summary>
        /// <param name="input">The string containing operators, brackets, values</param>
        /// <param name="start">The opening bracket's index (inclusive)</param>
        /// <param name="end">The closing bracket's index (exclusive)</param>
        /// <returns>Returns true if every operator has a value inbetween them.</returns>
        private bool checkOperatorCorrectness(string input, int start, int end)
        {
            for (int i = 0; i < mathoperators.Length; ++i)
            {
                for (int j = start; j < end; ++j)
                {
                    j = input.IndexOf(mathoperators[i], j);
                    if (j == -1 || j >= end)
                        break;

                    if (j != 0)
                        //Check if operator is next to a {
                        if ((input[j - 1] == '{'
                            || mathoperators.Contains(input[j - 1])) && input[j] != '-')
                            return false;

                    if (j != input.Length - 1)
                        //Check if operator is next to a }
                        if ((input[j + 1] == '}'
                            || mathoperators.Contains(input[j + 1])) && input[j + 1] != '-')
                            return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks the brackets are placed correctly and there are equal amounts of them.
        /// </summary>
        /// <param name="input">The string containing operators, brackets, values</param>
        /// <param name="start">The opening bracket's index (inclusive)</param>
        /// <param name="end">The closing bracket's index (exclusive)</param>
        /// <returns>Returns false if opening and closing bracket amounts not equal or if they are placed incorrectly.</returns>
        private bool checkRoundBracketCorrectness(string input, int start, int end)
        {
            //"Why not treat it as an operator?"
            //Because it would cause many headaches and also it isn't doing something
            //to two values, but just makes the operators inside of it have higher priority
            int openingBracketPos = start;
            int closingBracketPos = start;
            if (input.Contains("()"))
                return false;

            while (openingBracketPos != -1 || closingBracketPos != -1)
            {
                if (openingBracketPos != -1 && openingBracketPos < end)
                    openingBracketPos = input.IndexOf('(', openingBracketPos + 1);
                if (closingBracketPos != -1 && closingBracketPos < end)
                    closingBracketPos = input.IndexOf(')', closingBracketPos + 1);
                if ((openingBracketPos == -1 && closingBracketPos != -1)
                    || (openingBracketPos != -1 && closingBracketPos == -1))
                    return false;
                if (openingBracketPos > closingBracketPos)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Replaces '-' with '_' which are after an operator or with -1* if its before a (.
        /// </summary>
        /// <param name="input">The string containing operators, brackets, values</param>
        /// <param name="start">The opening bracket's index (inclusive)</param>
        /// <param name="end">The closing bracket's index (exclusive)</param>
        private void replaceMinus(ref string input, int start, int end)
        {
            const string numbers = "1234567890";

            for (int i = start; i < end; ++i)
            {
                end = input.IndexOf('}', start);
                i = input.IndexOf('-', i);
                if (i == -1 || i >= end)
                    break;

                if (i != 0 && i != input.Length)
                {
                    bool isAfterNumber = numbers.Contains(input[i - 1]);
                    bool isAfterMathSymbol = mathoperators.Contains(input[i - 1]) || input[i - 1] == '{' || input[i - 1] == '(';
                    bool isBeforeNumber = numbers.Contains(input[i + 1]);
                    bool isBeforeMathOperator = mathoperators.Contains(input[i + 1]);
                    if ((!isAfterNumber && isAfterMathSymbol && isBeforeNumber) || (isAfterMathSymbol && !isBeforeNumber && !isBeforeMathOperator))
                        input = string.Concat(input.Substring(0, i), "_", input.Substring(i + 1, input.Length - i - 1));
                }
            }
        }

        /// <summary>
        /// Adds * betweem )( or x( (Where x is a number)
        /// </summary>
        /// <param name="input">The string containing operators, brackets, values</param>
        /// <param name="start">The opening bracket's index (inclusive)</param>
        /// <param name="end">The closing bracket's index (exclusive)</param>
        private void addImpliedMultiplication(ref string input, int start, int end)
        {
            const string numbers = "1234567890";
            string insideBracket = input.Substring(start + 1, end - start - 1).Replace(")(", ")*(");
            input = string.Concat(input.Substring(0, start + 1), insideBracket, input.Substring(end, input.Length - end));

            for (int i = start; i < end; ++i)
            {
                end = input.IndexOf('}', start);
                i = input.IndexOf('(', i);
                if (i == -1 || i >= end)
                    break;

                if (i != 0)
                {
                    if (numbers.Contains(input[i - 1]))
                    {
                        input = string.Concat(input.Substring(0, i), "*(", input.Substring(i + 1, input.Length - i - 1));
                    }
                }
            }
        }

        private void splitUpOperatorText(ref string[] variableTexts, ref string[] operatorSymbols, ref int[] operatorPriority)
        {
            //Since variables array isn't used completely we need to keep
            //its true length in a variable
            int trueLength = 1;

            //A lot of the time only one or two operators will be used
            //so it would be better to skip the ones which aren't used
            //Note: This is future proofing for when more are added
            bool[] skipOperator = new bool[mathoperators.Length];

            for (int i = 0; i < mathoperators.Length; ++i)
            {
                skipOperator[i] = !variableTexts[0].Contains(mathoperators[i]);
            }

            for (int mathOperatorIndex = 0; mathOperatorIndex < mathoperators.Length; ++mathOperatorIndex)
            {
                if (skipOperator[mathOperatorIndex])
                    continue;

                for (int splitTextIndex = 0; splitTextIndex < trueLength; ++splitTextIndex)
                {
                    string[] split = variableTexts[splitTextIndex].Split(mathoperators[mathOperatorIndex]);
                    if (split.Length < 2)
                        continue;

                    //We first move out of the way the already stored variables
                    //It starts from the last index and store items at split length minus one lower
                    //So if 7 is the last index and it was split between 3 then the stored
                    //variable at index 5 (7 - (3 - 1)) move to 7. This way nothing is
                    //overwritten.
                    for (int movedToIndex = variableTexts.Length - 1; movedToIndex > splitTextIndex + split.Length - 2; --movedToIndex)
                    {
                        int movedFromIndex = movedToIndex - split.Length + 1;
                        variableTexts[movedToIndex] = variableTexts[movedFromIndex];

                        //operator related arrays' length is one less than variableTexts' length
                        if (movedToIndex != variableTexts.Length - 1)
                        {
                            operatorSymbols[movedToIndex] = operatorSymbols[movedFromIndex];
                            operatorPriority[movedToIndex] = operatorPriority[movedFromIndex];
                        }
                    }

                    //Then add the splits to the created empty places
                    for (int k = 0; k < split.Length; ++k)
                    {
                        variableTexts[splitTextIndex + k] = split[k];

                        if (k != split.Length - 1)
                        {
                            operatorSymbols[splitTextIndex + k] = mathoperators[mathOperatorIndex].ToString();
                            operatorPriority[splitTextIndex + k] = mathOperatorIndex;
                        }
                    }

                    //The split values don't gonna contain the operator
                    //they were split by so we can skip them.
                    trueLength += split.Length - 1;
                    splitTextIndex += split.Length - 1;
                }
            }

            int giveHigherPriority = 0;

            for (int i = 0; i < operatorSymbols.Length; ++i)
            {
                //priority is given to operators which are inside ()
                //priority is their index in the mathOperators plus
                //the length of mathOperators multiplied by the amount of brackets
                //it is in
                if (variableTexts[i].Length - variableTexts[i].TrimStart('(').Length > 0)
                    giveHigherPriority += mathoperators.Length * (variableTexts[i].Length - variableTexts[i].TrimStart('(').Length);
                if (variableTexts[i].Length - variableTexts[i].TrimEnd(')').Length > 0)
                    giveHigherPriority -= mathoperators.Length * (variableTexts[i].Length - variableTexts[i].TrimEnd(')').Length);
                operatorPriority[i] += giveHigherPriority;
            }
        }

        private double doConversionCalculation(string[] variableTexts, string[] operatorSymbols, int[] operatorPriority)
        {
            double[] variableValues = new double[variableTexts.Length];
            BeatmapAttribute[] beatmapAttributes = Enum.GetValues<BeatmapAttribute>();

            for (int i = 0; i < variableTexts.Length; ++i)
            {
                bool isValueNegative = variableTexts[i].Trim('(')[0] == '_';
                bool isBeatmapAttribute = false;
                variableTexts[i] = variableTexts[i].Trim('(', ')', '_');

                for (int j = 0; j < beatmapAttributes.Length; ++j)
                {
                    if (beatmapAttributes[j].ToString() == variableTexts[i])
                    {
                        variableValues[i] = getLabelValue(beatmapAttributes[j]);
                        isBeatmapAttribute = true;
                        if (isValueNegative)
                            variableValues[i] *= -1;
                        break;
                    }

                    if (variableTexts[i] == "Value")
                    {
                        variableValues[i] = getLabelValue(Attribute.Value);
                        isBeatmapAttribute = true;
                        if (isValueNegative)
                            variableValues[i] *= -1;
                        break;
                    }
                }

                if (isBeatmapAttribute)
                    continue;

                if (!double.TryParse(variableTexts[i], out variableValues[i]))
                {
                    variableValues[i] = double.NaN;
                }
                else
                {
                    if (isValueNegative)
                        variableValues[i] *= -1;
                }
            }

            //Ordering priorities so that everything calculated in expected order
            List<int> decreasingPriorities = new List<int>();
            int previousMaxPriority = int.MaxValue;

            for (int i = 0; i < operatorPriority.Length; ++i)
            {
                int maxPriority = 0;

                for (int j = 0; j < operatorPriority.Length; ++j)
                {
                    if (operatorPriority[j] > maxPriority && operatorPriority[j] < previousMaxPriority)
                        maxPriority = operatorPriority[j];
                }

                previousMaxPriority = maxPriority;
                decreasingPriorities.Add(maxPriority);
            }

            int prioritiesTrueLength = operatorPriority.Length;

            for (int i = 0; i < decreasingPriorities.Count; ++i)
            {
                for (int j = 0; j < prioritiesTrueLength; ++j)
                {
                    if (operatorPriority[j] != decreasingPriorities[i])
                        continue;

                    variableValues[j] = doMathOperation(operatorSymbols[j], variableValues[j], variableValues[j + 1]);
                    prioritiesTrueLength--;

                    //Overwrite already used variable, calculated operator
                    for (int k = j; k < prioritiesTrueLength; ++k)
                    {
                        operatorSymbols[k] = operatorSymbols[k + 1];
                        operatorPriority[k] = operatorPriority[k + 1];
                        variableValues[k + 1] = variableValues[k + 2];
                    }

                    --j;
                }
            }

            return variableValues[0];
        }

        private double doMathOperation(string operatorChar, double value1, double value2)
        {
            switch (operatorChar)
            {
                case "+":
                    return value1 + value2;

                case "-":
                    return value1 - value2;

                case "*":
                    return value1 * value2;

                case "/":
                    if (value2 == 0)
                        return double.NaN;

                    return value1 / value2;

                case "%":
                    return value1 % value2;
            }

            return double.NaN;
        }

        private double getLabelValue(BeatmapAttribute attribute)
        {
            switch (attribute)
            {
                case BeatmapAttribute.CircleSize:
                    return Math.Round(computeDifficulty().CircleSize, 2);

                case BeatmapAttribute.Accuracy:
                    return Math.Round(computeDifficulty().OverallDifficulty, 2);

                case BeatmapAttribute.HPDrain:
                    return Math.Round(computeDifficulty().DrainRate, 2);

                case BeatmapAttribute.ApproachRate:
                    return Math.Round(computeDifficulty().ApproachRate, 2);

                case BeatmapAttribute.StarRating:
                    return (starDifficulty?.Stars ?? 0).FloorToDecimalDigits(2);

                case BeatmapAttribute.Title:
                case BeatmapAttribute.Artist:
                case BeatmapAttribute.DifficultyName:
                case BeatmapAttribute.Creator:
                case BeatmapAttribute.Source:
                    return double.NaN;

                case BeatmapAttribute.RankedStatus:
                    return beatmap.Value.BeatmapSetInfo.StatusInt;

                case BeatmapAttribute.Length:
                    return Math.Floor(Math.Round(beatmap.Value.BeatmapInfo.Length / ModUtils.CalculateRateWithMods(mods.Value)) / 1000.0);

                case BeatmapAttribute.BPM:
                    return FormatUtils.RoundBPM(beatmap.Value.BeatmapInfo.BPM, ModUtils.CalculateRateWithMods(mods.Value));

                case BeatmapAttribute.MaxPP:
                    return Math.Round(starDifficulty?.PerformanceAttributes?.Total ?? 0, MidpointRounding.AwayFromZero);

                default:
                    return double.NaN;
            }

            BeatmapDifficulty computeDifficulty()
            {
                return ruleset.Value is RulesetInfo rulesetInfo
                    ? rulesetInfo.CreateInstance().GetAdjustedDisplayDifficulty(beatmap.Value.BeatmapInfo, mods.Value)
                    : new BeatmapDifficulty(beatmap.Value.BeatmapInfo.Difficulty);
            }
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);

        protected override void SetTextColour(Colour4 textColour) => text.Colour = textColour;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            difficultyCancellationSource?.Cancel();
            difficultyCancellationSource?.Dispose();
            difficultyCancellationSource = null;

            modSettingTracker?.Dispose();
        }
    }

    // WARNING: DO NOT ADD ANY VALUES TO THIS ENUM ANYWHERE ELSE THAN AT THE END.
    // Doing so will break existing user skins.
    public enum BeatmapAttribute
    {
        CircleSize,
        HPDrain,
        Accuracy,
        ApproachRate,
        StarRating,
        Title,
        Artist,
        DifficultyName,
        Creator,
        Length,
        RankedStatus,
        BPM,
        Source,
        MaxPP
    }
}
