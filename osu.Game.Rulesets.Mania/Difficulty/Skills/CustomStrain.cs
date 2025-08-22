// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Buffers;
using System.Runtime.InteropServices;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class CustomStrain : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        private readonly int keyCount;
        private readonly double overallDifficulty;
        private readonly double timeMultiplier;
        private readonly ProcessedNote[] processedNotes;
        private readonly ProcessedNote[] longNotes;
        private readonly ProcessedNote[] tailEvents;
        private readonly int[][] noteIndicesByColumn;
        private readonly int[] columnNoteCounts;

        private bool isCalculationComplete;
        private double cachedStarRating;
        private int lastProcessedIndex = -1;

        // Pre-computed lookup tables with better cache locality
        private static readonly double[] JackLookup = InitializeJackLookup();
        private static readonly double[] AnchorLookup = InitializeAnchorLookup();
        private static readonly double[][] CrossMatrixLookup = InitializeCrossMatrix();

        // Memory pools with specific sizes
        private static readonly ArrayPool<double> DoublePool = ArrayPool<double>.Create(1024, 16);
        private static readonly ArrayPool<int> IntPool = ArrayPool<int>.Create(1024, 16);
        private static readonly ArrayPool<long> LongPool = ArrayPool<long>.Create(512, 8);

        // Cached arrays to avoid repeated allocations
        private readonly double[] reusableDoubleArray;
        private readonly int[] reusableIntArray;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private readonly struct ProcessedNote
        {
            public readonly int Column;
            public readonly long HeadTime;
            public readonly long TailTime;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessedNote(int column, long headTime, long tailTime = -1)
            {
                Column = column;
                HeadTime = headTime;
                TailTime = tailTime;
            }

            public bool IsLongNote => TailTime >= 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct StrainData
        {
            public readonly double[] Jbar;
            public readonly double[] Xbar;
            public readonly double[] Pbar;
            public readonly double[] Abar;
            public readonly double[] Rbar;
            public readonly double[] C;
            public readonly double[] Ks;

            public StrainData(double[] jbar, double[] xbar, double[] pbar, double[] abar, double[] rbar, double[] c, double[] ks)
            {
                Jbar = jbar;
                Xbar = xbar;
                Pbar = pbar;
                Abar = abar;
                Rbar = rbar;
                C = c;
                Ks = ks;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct LNRepresentation
        {
            public readonly long[] Points;
            public readonly double[] Cumsum;
            public readonly double[] Values;

            public LNRepresentation(long[] points, double[] cumsum, double[] values)
            {
                Points = points;
                Cumsum = cumsum;
                Values = values;
            }
        }

        public CustomStrain(Mod[] mods, int totalColumns, List<ManiaDifficultyHitObject> hitObjects, double overallDifficulty)
            : base(mods)
        {
            this.overallDifficulty = overallDifficulty;
            keyCount = totalColumns;
            timeMultiplier = GetTimeMultiplier(mods);

            // Pre-allocate with exact capacity to avoid resizing
            int hitObjectCount = hitObjects.Count;
            processedNotes = new ProcessedNote[hitObjectCount];
            noteIndicesByColumn = new int[keyCount][];
            columnNoteCounts = new int[keyCount];
            reusableDoubleArray = new double[Math.Max(keyCount, 1024)];
            reusableIntArray = new int[Math.Max(keyCount, 1024)];

            // First pass: count notes per column
            foreach (var hitObject in hitObjects)
                columnNoteCounts[hitObject.BaseObject.Column]++;

            // Initialize column arrays with exact sizes
            for (int i = 0; i < keyCount; i++)
                noteIndicesByColumn[i] = new int[columnNoteCounts[i]];

            int longNoteCount = PreprocessAllHitObjects(hitObjects);

            // Create long note arrays efficiently
            longNotes = new ProcessedNote[longNoteCount];
            tailEvents = new ProcessedNote[longNoteCount];
            ExtractLongNotes(longNoteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double[] InitializeJackLookup()
        {
            const int size = 2000;
            var lookup = GC.AllocateUninitializedArray<double>(size, pinned: false);

            for (int i = 0; i < size; i++)
            {
                double delta = (i + 1) * 0.001;
                double invDelta = 1.0 / delta;
                lookup[i] = invDelta * (1.0 / (delta + 0.11 * Math.Pow(0.3, 0.25)));
            }
            return lookup;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double[] InitializeAnchorLookup()
        {
            const int size = 100;
            var lookup = GC.AllocateUninitializedArray<double>(size, pinned: false);

            for (int i = 0; i < size; i++)
            {
                double val = i * 0.01;
                lookup[i] = 1 + Math.Min(val - 0.18, 5 * Math.Pow(val - 0.22, 3));
            }
            return lookup;
        }

        private static double[][] InitializeCrossMatrix()
        {
            return new double[][]
            {
                new double[] {-1},
                new[] {0.075, 0.075},
                new[] {0.125, 0.05, 0.125},
                new[] {0.125, 0.125, 0.125, 0.125},
                new[] {0.175, 0.25, 0.05, 0.25, 0.175},
                new[] {0.175, 0.25, 0.175, 0.175, 0.25, 0.175},
                new[] {0.225, 0.35, 0.25, 0.05, 0.25, 0.35, 0.225},
                new[] {0.225, 0.35, 0.25, 0.225, 0.225, 0.25, 0.35, 0.225},
                new[] {0.275, 0.45, 0.35, 0.25, 0.05, 0.25, 0.35, 0.45, 0.275},
                new[] {0.275, 0.45, 0.35, 0.25, 0.275, 0.275, 0.25, 0.35, 0.45, 0.275},
                new[] {0.325, 0.55, 0.45, 0.35, 0.25, 0.05, 0.25, 0.35, 0.45, 0.55, 0.325}
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetTimeMultiplier(ReadOnlySpan<Mod> mods)
        {
            for (int i = 0; i < mods.Length; i++)
            {
                var acronym = mods[i].Acronym.AsSpan();
                if (acronym.SequenceEqual("DT".AsSpan()))
                    return 2.0 / 3.0;
                if (acronym.SequenceEqual("HT".AsSpan()))
                    return 4.0 / 3.0;
            }
            return 1.0;
        }

        private int PreprocessAllHitObjects(List<ManiaDifficultyHitObject> hitObjects)
        {
            int longNoteCount = 0;
            Array.Clear(columnNoteCounts); // Reset for use as indices

            // Single pass processing
            for (int i = 0; i < hitObjects.Count; i++)
            {
                var maniaObject = hitObjects[i].BaseObject;
                long headTime = (long)Math.Round(maniaObject.StartTime * timeMultiplier);
                long tailTime = -1;

                if (maniaObject is HoldNote holdNote)
                {
                    tailTime = (long)Math.Round(holdNote.EndTime * timeMultiplier);
                    longNoteCount++;
                }

                var note = new ProcessedNote(maniaObject.Column, headTime, tailTime);
                processedNotes[i] = note;

                int column = maniaObject.Column;
                noteIndicesByColumn[column][columnNoteCounts[column]++] = i;
            }

            // Sort column indices by time in parallel
            Parallel.For(0, keyCount, col =>
            {
                var indices = noteIndicesByColumn[col].AsSpan(0, columnNoteCounts[col]);
                indices.Sort((a, b) => processedNotes[a].HeadTime.CompareTo(processedNotes[b].HeadTime));
            });

            return longNoteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExtractLongNotes(int longNoteCount)
        {
            int lnIndex = 0;
            var processedSpan = processedNotes.AsSpan();

            for (int i = 0; i < processedSpan.Length && lnIndex < longNoteCount; i++)
            {
                ref readonly var note = ref processedSpan[i];
                if (note.IsLongNote)
                {
                    longNotes[lnIndex] = note;
                    tailEvents[lnIndex] = note;
                    lnIndex++;
                }
            }

            Array.Sort(tailEvents, (a, b) => a.TailTime.CompareTo(b.TailTime));
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (ObjectStrains.Count == 0) return 0;

            var maniaObject = ((ManiaDifficultyHitObject)current).BaseObject;
            var currentTime = current.StartTime;
            int currentIndex = ObjectStrains.Count;
            lastProcessedIndex = currentIndex;

            return CalculateLocalStrain(maniaObject, currentTime, currentIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateLocalStrain(ManiaHitObject maniaObject, double currentTime, int currentIndex)
        {
            double strain = 0;
            int column = maniaObject.Column;
            int startIndex = Math.Max(0, currentIndex - 20);
            double minInterval = double.MaxValue;
            int maxIndex = Math.Min(currentIndex, processedNotes.Length);

            var processedSpan = processedNotes.AsSpan(startIndex, maxIndex - startIndex);

            // Vectorized processing where possible
            for (int i = 0; i < processedSpan.Length; i++)
            {
                ref readonly var note = ref processedSpan[i];
                double timeDiff = currentTime - note.HeadTime;

                if (note.Column == column)
                {
                    if (timeDiff <= 1000 && timeDiff > 0)
                        minInterval = Math.Min(minInterval, timeDiff);
                }
                else
                {
                    int columnDiff = Math.Abs(note.Column - column);
                    if (columnDiff == 1 && timeDiff <= 500 && timeDiff > 0)
                    {
                        double scaledDiff = timeDiff * 0.001;
                        strain += 0.16 / (scaledDiff * scaledDiff);
                    }
                }
            }

            if (minInterval < double.MaxValue)
            {
                double scaledInterval = minInterval * 0.001;
                strain += (1.0 / scaledInterval) * (1.0 / (scaledInterval + 0.11));
            }

            return Math.Min(strain, 100);
        }

        public override double DifficultyValue()
        {
            if (processedNotes.Length == 0) return 0;

            if (!isCalculationComplete || lastProcessedIndex >= processedNotes.Length - 1)
            {
                cachedStarRating = CalculateFullStarRating();
                isCalculationComplete = true;
            }

            return cachedStarRating;
        }

        private double CalculateFullStarRating()
        {
            long maxTime = GetMaxTime();
            var corners = GetOptimizedCorners((int)maxTime);
            double x = CalculateXParameter();

            var strainData = CalculateAllStrainComponents(corners, x, (int)maxTime);
            double sr = ComputeFinalStarRating(corners.AllCorners, strainData);

            double totalNotes = processedNotes.Length + 0.5 * SumLongNoteDensity();
            return sr * totalNotes / (totalNotes + 60) * 0.975;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetMaxTime()
        {
            long maxTime = 0;
            var processedSpan = processedNotes.AsSpan();

            for (int i = 0; i < processedSpan.Length; i++)
            {
                ref readonly var note = ref processedSpan[i];
                maxTime = Math.Max(maxTime, note.HeadTime);
                if (note.IsLongNote)
                    maxTime = Math.Max(maxTime, note.TailTime);
            }
            return maxTime + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateXParameter()
        {
            double x = 0.3 * Math.Sqrt((64.5 - Math.Ceiling(overallDifficulty * 3)) / 500.0);
            return Math.Min(x, 0.6 * (x - 0.09) + 0.09);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double SumLongNoteDensity()
        {
            double sum = 0;
            var longNoteSpan = longNotes.AsSpan();

            // Vectorize this calculation if possible
            for (int i = 0; i < longNoteSpan.Length; i++)
            {
                ref readonly var note = ref longNoteSpan[i];
                sum += Math.Min(note.TailTime - note.HeadTime, 1000) / 200.0;
            }
            return sum;
        }

        private (double[] AllCorners, double[] BaseCorners, double[] ACorners) GetOptimizedCorners(int T)
        {
            // Use more efficient data structures
            var baseSet = new SortedSet<double>();
            var aSet = new SortedSet<double>();

            var processedSpan = processedNotes.AsSpan();

            // Single pass through notes
            for (int i = 0; i < processedSpan.Length; i++)
            {
                ref readonly var note = ref processedSpan[i];
                AddTimePointsOptimized(baseSet, aSet, note.HeadTime, T);

                if (note.IsLongNote)
                    AddTimePointsOptimized(baseSet, aSet, note.TailTime, T);
            }

            baseSet.Add(0); baseSet.Add(T);
            aSet.Add(0); aSet.Add(T);

            var baseCorners = new double[baseSet.Count];
            var aCorners = new double[aSet.Count];

            baseSet.CopyTo(baseCorners);
            aSet.CopyTo(aCorners);

            // Combine arrays more efficiently
            var allSet = new SortedSet<double>(baseCorners);
            allSet.UnionWith(aCorners);

            var allCorners = new double[allSet.Count];
            allSet.CopyTo(allCorners);

            return (allCorners, baseCorners, aCorners);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddTimePointsOptimized(SortedSet<double> baseSet, SortedSet<double> aSet, long time, int T)
        {
            baseSet.Add(time);
            aSet.Add(time);

            if (time > 0)
            {
                long t499 = time - 499;
                long t1000 = time - 1000;
                if (t499 >= 0) baseSet.Add(t499);
                if (t1000 >= 0) aSet.Add(t1000);
            }

            long t501 = time + 501;
            long t1 = time + 1;
            long t1000p = time + 1000;

            if (t501 <= T) baseSet.Add(t501);
            if (t1 <= T) baseSet.Add(t1);
            if (t1000p <= T) aSet.Add(t1000p);
        }

        private StrainData CalculateAllStrainComponents(
            (double[] AllCorners, double[] BaseCorners, double[] ACorners) corners,
            double x, int T)
        {
            // Pre-calculate commonly used data in parallel where possible
            var keyUsageTask = Task.Run(() => CalculateKeyUsage(corners.BaseCorners));
            var keyUsage400Task = Task.Run(() => CalculateKeyUsage400(corners.BaseCorners, T));
            var lnBodiesTask = Task.Run(() => ComputeLNBodiesCount(T));

            Task.WaitAll(keyUsageTask, keyUsage400Task, lnBodiesTask);

            var keyUsage = keyUsageTask.GetResultSafely();
            var activeColumns = GetActiveColumns(keyUsage, corners.BaseCorners.Length);
            var keyUsage400 = keyUsage400Task.GetResultSafely();
            var lnBodies = lnBodiesTask.GetResultSafely();

            var anchor = ComputeAnchor(keyUsage400, corners.BaseCorners);
            var jackData = ComputeJackStrain(x, corners.BaseCorners);

            // Compute all strain components in parallel
            var tasks = new Task<double[]>[]
            {
                Task.Run(() => InterpolateValues(corners.AllCorners, corners.BaseCorners, jackData.Jbar)),
                Task.Run(() => InterpolateValues(corners.AllCorners, corners.BaseCorners,
                    ComputeCrossStrain(x, activeColumns, corners.BaseCorners))),
                Task.Run(() => InterpolateValues(corners.AllCorners, corners.BaseCorners,
                    ComputePatternStrain(x, lnBodies, anchor, corners.BaseCorners))),
                Task.Run(() => InterpolateValues(corners.AllCorners, corners.ACorners,
                    ComputeAccuracyStrain(x, activeColumns, jackData.DeltaKs, corners.ACorners, corners.BaseCorners))),
                Task.Run(() => InterpolateValues(corners.AllCorners, corners.BaseCorners,
                    ComputeReleaseStrain(x, corners.BaseCorners)))
            };

            var countAndKsData = ComputeCountAndKs(keyUsage, corners.BaseCorners);

            var stepTasks = new Task<double[]>[]
            {
                Task.Run(() => StepInterpolate(corners.AllCorners, corners.BaseCorners, countAndKsData.C)),
                Task.Run(() => StepInterpolate(corners.AllCorners, corners.BaseCorners, countAndKsData.Ks))
            };

            Task.WaitAll(tasks);
            Task.WaitAll(stepTasks);

            return new StrainData(tasks[0].GetResultSafely(), tasks[1].GetResultSafely(), tasks[2].GetResultSafely(),
                                tasks[3].GetResultSafely(), tasks[4].GetResultSafely(), stepTasks[0].GetResultSafely(), stepTasks[1].GetResultSafely());
        }

        private bool[][] CalculateKeyUsage(double[] baseCorners)
        {
            var keyUsage = new bool[keyCount][];
            for (int k = 0; k < keyCount; k++)
                keyUsage[k] = new bool[baseCorners.Length];

            var processedSpan = processedNotes.AsSpan();

            for (int i = 0; i < processedSpan.Length; i++)
            {
                ref readonly var note = ref processedSpan[i];
                long start = Math.Max(note.HeadTime - 150, 0);
                long end = note.IsLongNote ? Math.Min(note.TailTime + 150, (long)baseCorners[^1]) : note.HeadTime + 150;

                SetKeyUsageRange(keyUsage[note.Column], baseCorners, start, end);
            }

            return keyUsage;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetKeyUsageRange(bool[] keyUsageColumn, ReadOnlySpan<double> baseCorners, long start, long end)
        {
            int left = BinarySearchSpan(baseCorners, start);
            if (left < 0) left = ~left;
            int right = BinarySearchSpan(baseCorners, end);
            if (right < 0) right = ~right;

            for (int i = left; i < Math.Min(right, baseCorners.Length); i++)
                keyUsageColumn[i] = true;
        }

        private List<int>[] GetActiveColumns(bool[][] keyUsage, int length)
        {
            var activeColumns = new List<int>[length];
            for (int i = 0; i < length; i++)
            {
                var active = new List<int>(keyCount);
                for (int k = 0; k < keyCount; k++)
                {
                    if (keyUsage[k][i]) active.Add(k);
                }
                activeColumns[i] = active;
            }
            return activeColumns;
        }

        private double[][] CalculateKeyUsage400(double[] baseCorners, int T)
        {
            var keyUsage400 = new double[keyCount][];
            for (int k = 0; k < keyCount; k++)
                keyUsage400[k] = new double[baseCorners.Length];

            var processedSpan = processedNotes.AsSpan();

            for (int i = 0; i < processedSpan.Length; i++)
            {
                ref readonly var note = ref processedSpan[i];
                ProcessKeyUsage400(keyUsage400[note.Column], baseCorners, note, T);
            }

            return keyUsage400;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessKeyUsage400(double[] keyUsageColumn, ReadOnlySpan<double> baseCorners, in ProcessedNote note, int T)
        {
            long start = Math.Max(note.HeadTime, 0);
            long end = note.IsLongNote ? Math.Min(note.TailTime, T - 1) : note.HeadTime;

            int left = BinarySearchFloor(baseCorners, start);
            int right = BinarySearchFloor(baseCorners, end);

            double value = 3.75 + Math.Min(end - start, 1500) / 150.0;

            // Main range
            for (int i = left; i < right; i++)
                keyUsageColumn[i] += value;

            // Decay ranges
            ProcessDecayRange(keyUsageColumn, baseCorners, start, -400, left, true);
            ProcessDecayRange(keyUsageColumn, baseCorners, end, 400, right, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchFloor(ReadOnlySpan<double> array, long value)
        {
            int index = BinarySearchSpan(array, value);
            return index < 0 ? ~index : index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessDecayRange(double[] keyUsageColumn, ReadOnlySpan<double> baseCorners, long center, int offset, int boundaryIndex, bool isLeft)
        {
            long target = center + offset;
            int targetIndex = BinarySearchFloor(baseCorners, target);

            int start = isLeft ? targetIndex : boundaryIndex;
            int end = isLeft ? boundaryIndex : targetIndex;

            for (int i = start; i < end; i++)
            {
                double diff = baseCorners[i] - center;
                keyUsageColumn[i] += 3.75 - 3.75 / 160000 * diff * diff;
            }
        }

        private double[] ComputeAnchor(double[][] keyUsage400, double[] baseCorners)
        {
            double[] anchor = new double[baseCorners.Length];
            double[] counts = DoublePool.Rent(keyCount);

            try
            {
                for (int i = 0; i < baseCorners.Length; i++)
                {
                    // Copy and sort counts
                    for (int k = 0; k < keyCount; k++)
                        counts[k] = keyUsage400[k][i];

                    Array.Sort(counts, 0, keyCount, Comparer<double>.Create((a, b) => b.CompareTo(a)));

                    int nonzero = 0;
                    while (nonzero < keyCount && counts[nonzero] > 0)
                        nonzero++;

                    if (nonzero < 2)
                    {
                        anchor[i] = 0;
                        continue;
                    }

                    double walk = 0, maxWalk = 0;
                    for (int j = 0; j < nonzero - 1; j++)
                    {
                        double ratio = counts[j + 1] / counts[j];
                        double factor = 1 - 4 * (0.5 - ratio) * (0.5 - ratio);
                        walk += counts[j] * factor;
                        maxWalk += counts[j];
                    }

                    double val = walk / maxWalk;
                    int lookupIndex = Math.Min((int)(val * 100), AnchorLookup.Length - 1);
                    anchor[i] = AnchorLookup[lookupIndex];
                }
            }
            finally
            {
                DoublePool.Return(counts);
            }

            return anchor;
        }

        private (double[] Jbar, double[][] DeltaKs) ComputeJackStrain(double x, double[] baseCorners)
        {
            double[][] deltaKs = new double[keyCount][];
            double[][] JKs = new double[keyCount][];
            double[][] JbarKs = new double[keyCount][];

            for (int k = 0; k < keyCount; k++)
            {
                deltaKs[k] = new double[baseCorners.Length];
                JKs[k] = new double[baseCorners.Length];
                Array.Fill(deltaKs[k], 1000000);
            }

            double xPow025 = Math.Pow(x, 0.25);

            Parallel.For(0, keyCount, k =>
            {
                var notes = noteIndicesByColumn[k];
                int noteCount = columnNoteCounts[k];

                for (int i = 0; i < noteCount - 1; i++)
                {
                    long start = processedNotes[notes[i]].HeadTime;
                    long end = processedNotes[notes[i + 1]].HeadTime;

                    int left = BinarySearchFloor(baseCorners, start);
                    int right = BinarySearchFloor(baseCorners, end);

                    if (left >= right) continue;

                    double deltaMs = end - start;
                    double delta = 0.001 * deltaMs;

                    // Use lookup table for jack calculation
                    int lookupIndex = Math.Min((int)deltaMs - 1, JackLookup.Length - 1);
                    double val = lookupIndex >= 0 ? JackLookup[lookupIndex] :
                                 (1.0 / delta) * (1.0 / (delta + 0.11 * xPow025));

                    double nerf = 1 - 7e-5 * Math.Pow(0.15 + Math.Abs(delta - 0.08), -4);
                    double JVal = val * nerf;

                    for (int idx = left; idx < right; idx++)
                    {
                        JKs[k][idx] = JVal;
                        deltaKs[k][idx] = delta;
                    }
                }

                JbarKs[k] = SmoothOnCorners(baseCorners, JKs[k], 500, 0.001, true);
            });

            double[] Jbar = new double[baseCorners.Length];
            for (int i = 0; i < baseCorners.Length; i++)
            {
                double num = 0, den = 0;
                for (int k = 0; k < keyCount; k++)
                {
                    double val = Math.Max(JbarKs[k][i], 0);
                    double weight = 1.0 / deltaKs[k][i];
                    num += Math.Pow(val, 5) * weight;
                    den += weight;
                }
                Jbar[i] = Math.Pow(num / Math.Max(den, 1e-9), 0.2);
            }

            return (Jbar, deltaKs);
        }

        private double[] ComputeCrossStrain(double x, List<int>[] activeColumns, double[] baseCorners)
        {
            double[] crossCoeff = CrossMatrixLookup[keyCount];
            double[][] XKs = new double[keyCount + 1][];
            double[][] fastCross = new double[keyCount + 1][];

            for (int k = 0; k <= keyCount; k++)
            {
                XKs[k] = new double[baseCorners.Length];
                fastCross[k] = new double[baseCorners.Length];
            }

            Parallel.For(0, keyCount + 1, k =>
            {
                ComputeCrossStrainForColumnPair(k, XKs[k], fastCross[k], baseCorners, activeColumns, crossCoeff[k], x);
            });

            double[] XBase = new double[baseCorners.Length];
            for (int i = 0; i < baseCorners.Length; i++)
            {
                double sum = 0;
                for (int k = 0; k <= keyCount; k++)
                    sum += XKs[k][i] * crossCoeff[k];

                for (int k = 0; k < keyCount; k++)
                    sum += Math.Sqrt(fastCross[k][i] * crossCoeff[k] * fastCross[k + 1][i] * crossCoeff[k + 1]);

                XBase[i] = sum;
            }

            return SmoothOnCorners(baseCorners, XBase, 500, 0.001, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComputeCrossStrainForColumnPair(int k, double[] XK, double[] fastCrossK,
            double[] baseCorners, List<int>[] activeColumns, double crossCoeff, double x)
        {
            List<int> relevantNotes;

            if (k == 0)
            {
                // Create list from first column's note indices
                relevantNotes = new List<int>(columnNoteCounts[0]);
                for (int i = 0; i < columnNoteCounts[0]; i++)
                    relevantNotes.Add(noteIndicesByColumn[0][i]);
            }
            else if (k == keyCount)
            {
                // Create list from last column's note indices
                relevantNotes = new List<int>(columnNoteCounts[keyCount - 1]);
                for (int i = 0; i < columnNoteCounts[keyCount - 1]; i++)
                    relevantNotes.Add(noteIndicesByColumn[keyCount - 1][i]);
            }
            else
            {
                var notes1 = noteIndicesByColumn[k - 1];
                var notes2 = noteIndicesByColumn[k];
                int count1 = columnNoteCounts[k - 1];
                int count2 = columnNoteCounts[k];

                relevantNotes = new List<int>(count1 + count2);

                // Merge sorted lists efficiently
                int i1 = 0, i2 = 0;
                while (i1 < count1 && i2 < count2)
                {
                    if (processedNotes[notes1[i1]].HeadTime <= processedNotes[notes2[i2]].HeadTime)
                        relevantNotes.Add(notes1[i1++]);
                    else
                        relevantNotes.Add(notes2[i2++]);
                }
                while (i1 < count1) relevantNotes.Add(notes1[i1++]);
                while (i2 < count2) relevantNotes.Add(notes2[i2++]);
            }

            for (int i = 1; i < relevantNotes.Count; i++)
            {
                long start = processedNotes[relevantNotes[i - 1]].HeadTime;
                long end = processedNotes[relevantNotes[i]].HeadTime;

                int left = BinarySearchFloor(baseCorners, start);
                int right = BinarySearchFloor(baseCorners, end);

                if (left >= right) continue;

                double delta = 0.001 * (end - start);
                double val = 0.16 * Math.Pow(Math.Max(x, delta), -2);

                bool col1Inactive = k > 0 && !activeColumns[left].Contains(k - 1) && !activeColumns[right].Contains(k - 1);
                bool col2Inactive = k < keyCount && !activeColumns[left].Contains(k) && !activeColumns[right].Contains(k);

                if (col1Inactive || col2Inactive)
                    val *= 1 - crossCoeff;

                for (int idx = left; idx < right; idx++)
                {
                    XK[idx] = val;
                    fastCrossK[idx] = Math.Max(0, 0.4 * Math.Pow(Math.Max(Math.Max(delta, 0.06), 0.75 * x), -2) - 80);
                }
            }
        }

        private LNRepresentation ComputeLNBodiesCount(int T)
        {
            var diffDict = new Dictionary<long, double>(longNotes.Length * 3);

            var longNoteSpan = longNotes.AsSpan();
            for (int i = 0; i < longNoteSpan.Length; i++)
            {
                ref readonly var note = ref longNoteSpan[i];
                long t0 = Math.Min(note.HeadTime + 60, note.TailTime);
                long t1 = Math.Min(note.HeadTime + 120, note.TailTime);

                diffDict.TryGetValue(t0, out double current);
                diffDict[t0] = current + 1.3;

                diffDict.TryGetValue(t1, out current);
                diffDict[t1] = current - 0.3;

                diffDict.TryGetValue(note.TailTime, out current);
                diffDict[note.TailTime] = current - 1;
            }

            var pointsList = new List<long>(diffDict.Keys) { 0, T };
            pointsList.Sort();

            var points = pointsList.ToArray();
            var values = new double[points.Length - 1];
            var cumsum = new double[points.Length];
            cumsum[0] = 0.0;

            double currentSum = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (diffDict.TryGetValue(points[i], out double delta))
                    currentSum += delta;

                double val = Math.Min(currentSum, 2.5 + 0.5 * currentSum);
                values[i] = val;
                long length = points[i + 1] - points[i];
                cumsum[i + 1] = cumsum[i] + val * length;
            }

            return new LNRepresentation(points, cumsum, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double LnSum(long a, long b, in LNRepresentation rep)
        {
            int i = Array.BinarySearch(rep.Points, a);
            if (i < 0) i = ~i - 1;
            i = Math.Max(0, i);

            int j = Array.BinarySearch(rep.Points, b);
            if (j < 0) j = ~j - 1;
            j = Math.Max(0, j);

            if (i == j) return (b - a) * rep.Values[i];

            double total = (rep.Points[i + 1] - a) * rep.Values[i];
            total += rep.Cumsum[j] - rep.Cumsum[i + 1];
            total += (b - rep.Points[j]) * rep.Values[j];
            return total;
        }

        private double[] ComputePatternStrain(double x, LNRepresentation lnRep, double[] anchor, double[] baseCorners)
        {
            double[] PStep = new double[baseCorners.Length];
            double invX = 1.0 / x;
            double x24 = 24 * invX;
            double xHalf = x * 0.5;
            double twoThirdsX = 2 * x / 3;
            double xSixth = x / 6;

            var processedSpan = processedNotes.AsSpan();
            for (int i = 0; i < processedSpan.Length - 1; i++)
            {
                ref readonly var note1 = ref processedSpan[i];
                ref readonly var note2 = ref processedSpan[i + 1];

                long hL = note1.HeadTime;
                long hR = note2.HeadTime;
                long deltaTime = hR - hL;

                if (hL == hR)
                {
                    double spike = 1000 * Math.Pow(0.02 * (4 * invX - 24), 0.25);
                    int idx = BinarySearchFloor(baseCorners, hL);
                    if (idx < PStep.Length) PStep[idx] += spike;
                    continue;
                }

                int left = BinarySearchFloor(baseCorners, hL);
                int right = BinarySearchFloor(baseCorners, hR);

                if (left >= right) continue;

                double delta = 0.001 * deltaTime;
                double lnSum = LnSum(hL, hR, in lnRep);
                double v = 1 + 0.006 * lnSum;

                double bVal = 1;
                double ratio = 7.5 / delta;
                if (ratio > 160 && ratio < 360)
                    bVal = 1 + 1.7e-7 * (ratio - 160) * (ratio - 360) * (ratio - 360);

                double baseValue = 0.08 * invX * (1 - x24 * (delta - xHalf) * (delta - xHalf));
                if (delta >= twoThirdsX)
                    baseValue = 0.08 * invX * (1 - x24 * xSixth * xSixth);

                double inc = (1 / delta) * Math.Pow(baseValue, 0.25) * Math.Max(bVal, v);
                double incVal = Math.Min(inc * anchor[left], Math.Max(inc, inc * 2 - 10));

                for (int idx = left; idx < right; idx++)
                    PStep[idx] += incVal;
            }

            return SmoothOnCorners(baseCorners, PStep, 500, 0.001, true);
        }

        private double[] ComputeAccuracyStrain(double x, List<int>[] activeColumns, double[][] deltaKs,
            double[] aCorners, double[] baseCorners)
        {
            double[][] dks = new double[keyCount - 1][];
            for (int k = 0; k < keyCount - 1; k++)
                dks[k] = new double[baseCorners.Length];

            // Pre-compute column differences
            for (int i = 0; i < baseCorners.Length; i++)
            {
                var cols = activeColumns[i];
                for (int j = 0; j < cols.Count - 1; j++)
                {
                    int k0 = cols[j];
                    if (k0 >= keyCount - 1) continue;

                    int k1 = cols[j + 1];
                    double diff = Math.Abs(deltaKs[k0][i] - deltaKs[k1][i]);
                    double maxDelta = Math.Max(deltaKs[k0][i], deltaKs[k1][i]);
                    dks[k0][i] = diff + 0.4 * Math.Max(0, maxDelta - 0.11);
                }
            }

            double[] AStep = new double[aCorners.Length];
            Array.Fill(AStep, 1.0);

            for (int i = 0; i < aCorners.Length; i++)
            {
                double s = aCorners[i];
                int baseIdx = BinarySearchFloor(baseCorners, (long)s) - 1;
                baseIdx = Math.Clamp(baseIdx, 0, baseCorners.Length - 1);

                var cols = activeColumns[baseIdx];
                for (int j = 0; j < cols.Count - 1; j++)
                {
                    int k0 = cols[j];
                    if (k0 >= keyCount - 1) continue;

                    double dVal = dks[k0][baseIdx];
                    double maxDelta = Math.Max(deltaKs[k0][baseIdx], deltaKs[cols[j + 1]][baseIdx]);

                    if (dVal < 0.02)
                        AStep[i] *= Math.Min(0.75 + 0.5 * maxDelta, 1);
                    else if (dVal < 0.07)
                        AStep[i] *= Math.Min(0.65 + 5 * dVal + 0.5 * maxDelta, 1);
                }
            }

            return SmoothOnCorners(aCorners, AStep, 250, 1.0, false);
        }

        private double[] ComputeReleaseStrain(double x, double[] baseCorners)
        {
            double[] RStep = new double[baseCorners.Length];
            double[] IList = new double[tailEvents.Length];
            double invX = 1.0 / x;

            var tailEventsSpan = tailEvents.AsSpan();

            // Pre-compute I values
            for (int i = 0; i < tailEventsSpan.Length; i++)
            {
                ref readonly var note = ref tailEventsSpan[i];
                var nextNote = FindNextNoteInColumn(note, noteIndicesByColumn[note.Column]);

                double IH = 0.001 * Math.Abs(note.TailTime - note.HeadTime - 80) * invX;
                double IT = 0.001 * Math.Abs(nextNote.HeadTime - note.TailTime - 80) * invX;

                double exp1 = Math.Exp(Math.Max(-5 * (IH - 0.75), -50));
                double exp2 = Math.Exp(Math.Max(-5 * (IT - 0.75), -50));
                IList[i] = 2.0 / (2 + exp1 + exp2);
            }

            for (int i = 0; i < tailEventsSpan.Length - 1; i++)
            {
                long tStart = tailEventsSpan[i].TailTime;
                long tEnd = tailEventsSpan[i + 1].TailTime;

                int left = BinarySearchFloor(baseCorners, tStart);
                int right = BinarySearchFloor(baseCorners, tEnd);

                if (left >= right) continue;

                double deltaR = 0.001 * (tEnd - tStart);
                double rVal = 0.08 * Math.Pow(deltaR, -0.5) * invX * (1 + 0.8 * (IList[i] + IList[i + 1]));

                for (int idx = left; idx < right; idx++)
                    RStep[idx] = rVal;
            }

            return SmoothOnCorners(baseCorners, RStep, 500, 0.001, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ProcessedNote FindNextNoteInColumn(in ProcessedNote note, int[] columnNoteIndices)
        {
            int count = columnNoteCounts[note.Column];
            for (int i = 0; i < count; i++)
            {
                var n = processedNotes[columnNoteIndices[i]];
                if (n.HeadTime > note.HeadTime)
                    return n;
            }
            return new ProcessedNote(0, long.MaxValue, long.MaxValue);
        }

        private (double[] C, double[] Ks) ComputeCountAndKs(bool[][] keyUsage, double[] baseCorners)
        {
            double[] C = new double[baseCorners.Length];
            double[] Ks = new double[baseCorners.Length];

            // Pre-sort note times for binary search
            var sortedTimes = LongPool.Rent(processedNotes.Length);

            try
            {
                var processedSpan = processedNotes.AsSpan();
                for (int i = 0; i < processedSpan.Length; i++)
                    sortedTimes[i] = processedSpan[i].HeadTime;

                Array.Sort(sortedTimes, 0, processedNotes.Length);

                for (int i = 0; i < baseCorners.Length; i++)
                {
                    double s = baseCorners[i];
                    long low = (long)(s - 500);
                    long high = (long)(s + 500);

                    int lowIdx = BinarySearchFloorLong(sortedTimes.AsSpan(0, processedNotes.Length), low);
                    int highIdx = BinarySearchFloorLong(sortedTimes.AsSpan(0, processedNotes.Length), high);

                    C[i] = highIdx - lowIdx;

                    int count = 0;
                    for (int k = 0; k < keyCount; k++)
                    {
                        if (keyUsage[k][i]) count++;
                    }
                    Ks[i] = Math.Max(count, 1);
                }
            }
            finally
            {
                LongPool.Return(sortedTimes);
            }

            return (C, Ks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchFloorLong(ReadOnlySpan<long> array, long value)
        {
            int index = array.BinarySearch(value);
            return index < 0 ? ~index : index;
        }

        private double ComputeFinalStarRating(double[] allCorners, StrainData s)
        {
            int length = allCorners.Length;
            double[] DAll = DoublePool.Rent(length);
            double[] gaps = DoublePool.Rent(length);
            double[] weights = DoublePool.Rent(length);
            int[] indices = IntPool.Rent(length);

            try
            {
                // Compute gaps efficiently
                if (length > 0)
                {
                    gaps[0] = length > 1 ? (allCorners[1] - allCorners[0]) * 0.5 : 1.0;
                    if (length > 1)
                        gaps[length - 1] = (allCorners[length - 1] - allCorners[length - 2]) * 0.5;

                    for (int i = 1; i < length - 1; i++)
                        gaps[i] = (allCorners[i + 1] - allCorners[i - 1]) * 0.5;
                }

                // Compute DAll and weights
                double totalWeight = 0;
                for (int i = 0; i < length; i++)
                {
                    double s1 = Math.Pow(s.Abar[i], 3.0 / s.Ks[i]) * Math.Min(s.Jbar[i], 8 + 0.85 * s.Jbar[i]);
                    double s2 = Math.Pow(s.Abar[i], 2.0 / 3.0) * (0.8 * s.Pbar[i] + s.Rbar[i] * 35 / (s.C[i] + 8));
                    double S = Math.Pow(0.4 * Math.Pow(s1, 1.5) + 0.6 * Math.Pow(s2, 1.5), 2.0 / 3.0);
                    double T = (Math.Pow(s.Abar[i], 3.0 / s.Ks[i]) * s.Xbar[i]) / (s.Xbar[i] + S + 1);
                    DAll[i] = 2.7 * Math.Sqrt(S) * Math.Pow(T, 1.5) + S * 0.27;

                    weights[i] = s.C[i] * gaps[i];
                    totalWeight += weights[i];
                    indices[i] = i;
                }

                // Sort by DAll values
                Array.Sort(DAll, indices, 0, length);

                // Reorder weights according to sorted indices
                var sortedWeights = DoublePool.Rent(length);
                try
                {
                    for (int i = 0; i < length; i++)
                        sortedWeights[i] = weights[indices[i]];

                    // Compute cumulative weights efficiently
                    var cumWeights = DoublePool.Rent(length);
                    try
                    {
                        cumWeights[0] = sortedWeights[0];
                        for (int i = 1; i < length; i++)
                            cumWeights[i] = cumWeights[i - 1] + sortedWeights[i];

                        double invTotalWeight = 1.0 / totalWeight;
                        for (int i = 0; i < length; i++)
                            cumWeights[i] *= invTotalWeight;

                        // Find percentiles efficiently
                        double percentile93 = CalculatePercentile(DAll.AsSpan(0, length), cumWeights.AsSpan(0, length),
                            new[] { 0.945, 0.935, 0.925, 0.915 });
                        double percentile83 = CalculatePercentile(DAll.AsSpan(0, length), cumWeights.AsSpan(0, length),
                            new[] { 0.845, 0.835, 0.825, 0.815 });

                        // Weighted mean calculation
                        double weightedSum = 0;
                        for (int i = 0; i < length; i++)
                            weightedSum += Math.Pow(DAll[i], 5) * sortedWeights[i];
                        double weightedMean = Math.Pow(weightedSum * invTotalWeight, 0.2);

                        double SR = 0.88 * percentile93 * 0.25 + 0.94 * percentile83 * 0.2 + weightedMean * 0.55;

                        return SR <= 9 ? SR : 9 + (SR - 9) / 1.2;
                    }
                    finally
                    {
                        DoublePool.Return(cumWeights);
                    }
                }
                finally
                {
                    DoublePool.Return(sortedWeights);
                }
            }
            finally
            {
                DoublePool.Return(DAll);
                DoublePool.Return(gaps);
                DoublePool.Return(weights);
                IntPool.Return(indices);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalculatePercentile(ReadOnlySpan<double> sortedValues, ReadOnlySpan<double> cumWeights, ReadOnlySpan<double> targets)
        {
            double sum = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                int idx = cumWeights.BinarySearch(targets[i]);
                if (idx < 0) idx = ~idx;
                sum += sortedValues[Math.Min(idx, sortedValues.Length - 1)];
            }
            return sum / targets.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double[] SmoothOnCorners(ReadOnlySpan<double> x, ReadOnlySpan<double> f, double window, double scale, bool isSum)
        {
            double[] F = CumulativeSum(x, f);
            double[] g = new double[x.Length];
            double invScale = isSum ? scale : 1.0;

            for (int i = 0; i < x.Length; i++)
            {
                double s = x[i];
                double a = Math.Max(s - window, x[0]);
                double b = Math.Min(s + window, x[x.Length - 1]);
                double val = QueryCumsum(b, x, F, f) - QueryCumsum(a, x, F, f);

                if (!isSum && (b - a) > 0)
                    g[i] = val / (b - a);
                else
                    g[i] = invScale * val;
            }
            return g;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double[] CumulativeSum(ReadOnlySpan<double> x, ReadOnlySpan<double> f)
        {
            double[] F = new double[x.Length];
            for (int i = 1; i < x.Length; i++)
                F[i] = F[i - 1] + f[i - 1] * (x[i] - x[i - 1]);
            return F;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double QueryCumsum(double q, ReadOnlySpan<double> x, ReadOnlySpan<double> F, ReadOnlySpan<double> f)
        {
            if (q <= x[0]) return 0.0;
            if (q >= x[x.Length - 1]) return F[F.Length - 1];

            int i = BinarySearchSpan(x, q);
            if (i >= 0) return F[i];

            i = ~i - 1;
            return F[i] + f[i] * (q - x[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double[] InterpolateValues(ReadOnlySpan<double> newX, ReadOnlySpan<double> oldX, ReadOnlySpan<double> oldVals)
        {
            var newVals = GC.AllocateUninitializedArray<double>(newX.Length);
            var newValsSpan = newVals.AsSpan();

            for (int i = 0; i < newX.Length; i++)
            {
                int idx = BinarySearchSpan(oldX, newX[i]);
                if (idx >= 0)
                {
                    newValsSpan[i] = oldVals[idx];
                    continue;
                }

                idx = ~idx;
                if (idx == 0)
                    newValsSpan[i] = oldVals[0];
                else if (idx == oldX.Length)
                    newValsSpan[i] = oldVals[oldVals.Length - 1];
                else
                {
                    double x0 = oldX[idx - 1], x1 = oldX[idx];
                    double y0 = oldVals[idx - 1], y1 = oldVals[idx];
                    double t = (newX[i] - x0) / (x1 - x0);
                    newValsSpan[i] = y0 + t * (y1 - y0);
                }
            }
            return newVals;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double[] StepInterpolate(ReadOnlySpan<double> newX, ReadOnlySpan<double> oldX, ReadOnlySpan<double> oldVals)
        {
            var newVals = GC.AllocateUninitializedArray<double>(newX.Length);
            var newValsSpan = newVals.AsSpan();

            for (int i = 0; i < newX.Length; i++)
            {
                int idx = BinarySearchSpan(oldX, newX[i]);
                if (idx < 0) idx = ~idx - 1;
                idx = Math.Clamp(idx, 0, oldVals.Length - 1);
                newValsSpan[i] = oldVals[idx];
            }
            return newVals;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchSpan(ReadOnlySpan<double> span, double value)
        {
            return span.BinarySearch(value);
        }
    }
}
