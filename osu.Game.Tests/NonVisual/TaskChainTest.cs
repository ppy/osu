// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class TaskChainTest
    {
        private TaskChain taskChain;
        private int currentTask;
        private CancellationTokenSource globalCancellationToken;

        [SetUp]
        public void Setup()
        {
            globalCancellationToken = new CancellationTokenSource();
            taskChain = new TaskChain();
            currentTask = 0;
        }

        [TearDown]
        public void TearDown()
        {
            globalCancellationToken?.Cancel();
        }

        [Test]
        public async Task TestChainedTasksRunSequentially()
        {
            var task1 = addTask();
            var task2 = addTask();
            var task3 = addTask();

            task3.mutex.Set();
            task2.mutex.Set();
            task1.mutex.Set();

            await Task.WhenAll(task1.task, task2.task, task3.task);

            Assert.That(task1.task.GetResultSafely(), Is.EqualTo(1));
            Assert.That(task2.task.GetResultSafely(), Is.EqualTo(2));
            Assert.That(task3.task.GetResultSafely(), Is.EqualTo(3));
        }

        [Test]
        public async Task TestChainedTaskWithIntermediateCancelRunsInSequence()
        {
            var task1 = addTask();
            var task2 = addTask();
            var task3 = addTask();

            // Cancel task2, allow task3 to complete.
            task2.cancellation.Cancel();
            task2.mutex.Set();
            task3.mutex.Set();

            // Allow task3 to potentially complete.
            Thread.Sleep(1000);

            // Allow task1 to complete.
            task1.mutex.Set();

            // Wait on both tasks.
            await Task.WhenAll(task1.task, task3.task);

            Assert.That(task1.task.GetResultSafely(), Is.EqualTo(1));
            Assert.That(task2.task.IsCompleted, Is.False);
            Assert.That(task3.task.GetResultSafely(), Is.EqualTo(2));
        }

        [Test]
        public async Task TestChainedTaskDoesNotCompleteBeforeChildTasks()
        {
            var mutex = new ManualResetEventSlim(false);

            var task = taskChain.Add(async () => await Task.Run(() => mutex.Wait(globalCancellationToken.Token)));

            // Allow task to potentially complete
            Thread.Sleep(1000);

            Assert.That(task.IsCompleted, Is.False);

            // Allow the task to complete.
            mutex.Set();

            await task;
        }

        private (Task<int> task, ManualResetEventSlim mutex, CancellationTokenSource cancellation) addTask()
        {
            var mutex = new ManualResetEventSlim(false);
            var completionSource = new TaskCompletionSource<int>();

            var cancellationSource = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationSource.Token, globalCancellationToken.Token);

            taskChain.Add(() =>
            {
                mutex.Wait(globalCancellationToken.Token);
                completionSource.SetResult(Interlocked.Increment(ref currentTask));
            }, token.Token);

            return (completionSource.Task, mutex, cancellationSource);
        }
    }
}
