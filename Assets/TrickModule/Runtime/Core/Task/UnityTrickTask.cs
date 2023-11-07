using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TrickModule.Core
{
    public static class UnityTrickTask
    {
        public static int MaxThreads = 4;

        private static TrickTaskScheduler GetAvailableTaskScheduler()
        {
            lock (TrickTaskScheduler.TaskSchedulers)
            {
                int numSchedulers = TrickTaskScheduler.TaskSchedulers.Count;
                if (numSchedulers == MaxThreads)
                {
                    // Take the least busy scheduler. We order by the queue count, and then by if it's executing a task
                    return TrickTaskScheduler.TaskSchedulers.OrderBy(scheduler => scheduler.QueueCount).ThenBy(scheduler => scheduler.IsExecutingTask).First();
                }

                // Create a new scheduler
                return new TrickTaskScheduler(_globalCancellationTokenSource.Token);
            }
        }

        private static CancellationTokenSource _globalCancellationTokenSource = new CancellationTokenSource();


        private static async Task<KeyValuePair<System.Threading.Tasks.Task, TResult>> SubTaskWrapper<TResult>(Func<TResult> subTask)
        {
            TResult result = default(TResult);

            try
            {
                System.Threading.Tasks.Task castedTask = null;
                await TrickTask.ExecuteSynchronously(async () =>
                {
                    result = subTask();
                    if (typeof(TResult) == typeof(System.Threading.Tasks.Task))
                    {
                        castedTask = (System.Threading.Tasks.Task)(object)result;
                        await castedTask;
                    }
                });
                return new KeyValuePair<System.Threading.Tasks.Task, TResult>(castedTask, result);
            }
            catch (TaskCanceledException)
            {
                // Task cancelled, don't do anything
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);
            }

            return new KeyValuePair<System.Threading.Tasks.Task, TResult>(null, result);
        }

        private static async Task<KeyValuePair<System.Threading.Tasks.Task, TResult>> SubTaskWrapper<TResult>(Func<Task<TResult>> subTask)
        {
            TResult result = default(TResult);

            try
            {
                System.Threading.Tasks.Task castedTask = null;
                await TrickTask.ExecuteSynchronously(async () =>
                {
                    result = await subTask();
                    if (typeof(TResult) == typeof(System.Threading.Tasks.Task))
                    {
                        castedTask = (System.Threading.Tasks.Task)(object)result;
                        await castedTask;
                    }
                });
                return new KeyValuePair<System.Threading.Tasks.Task, TResult>(castedTask, result);
            }
            catch (TaskCanceledException)
            {
                // Task cancelled, don't do anything
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);
            }

            return new KeyValuePair<System.Threading.Tasks.Task, TResult>(null, result);
        }

        private static async Task<TResult> InternalStartNewTask<TResult>(Func<TResult> subTask, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            KeyValuePair<System.Threading.Tasks.Task, TResult> innerSubTask = new KeyValuePair<System.Threading.Tasks.Task, TResult>();

            try
            {
                System.Threading.Tasks.Task task = await System.Threading.Tasks.Task.Factory.StartNew(
                    // Handle child task exceptions correctly
                    async () => innerSubTask = await SubTaskWrapper(subTask),
                    cancellationToken,
                    creationOptions,
                    // Using our custom task scheduler
                    GetAvailableTaskScheduler());

                await task;

                if (innerSubTask.Key == null) return innerSubTask.Value;
                try
                {
                    await innerSubTask.Key;
                }
                catch (Exception e)
                {
                    Logger.Logger.Core.LogException(e);
                }
            }
            catch (TaskCanceledException)
            {
                // Task cancelled, don't do anything
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);
            }

            return innerSubTask.Value;
        }

        private static async Task<TResult> InternalStartNewTask<TResult>(Func<Task<TResult>> subTask, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            KeyValuePair<System.Threading.Tasks.Task, TResult> innerSubTask = new KeyValuePair<System.Threading.Tasks.Task, TResult>();

            try
            {
                System.Threading.Tasks.Task task =
                    await System.Threading.Tasks.Task.Factory.StartNew(async () => innerSubTask = await SubTaskWrapper(subTask), cancellationToken, creationOptions, GetAvailableTaskScheduler());
                await task;

                if (innerSubTask.Key == null) return innerSubTask.Value;
                try
                {
                    await innerSubTask.Key;
                }
                catch (Exception e)
                {
                    Logger.Logger.Core.LogException(e);
                }
            }
            catch (TaskCanceledException)
            {
                // Task cancelled, don't do anything
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);
            }

            return innerSubTask.Value;
        }


        public static Task<TResult> StartNewTask<TResult>(Func<TResult> subTask, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            return InternalStartNewTask(subTask, _globalCancellationTokenSource.Token, creationOptions);
        }

        public static Task<TResult> StartNewTask<TResult>(Func<TResult> subTask, CancellationToken cancellationToken)
        {
            return InternalStartNewTask(subTask, cancellationToken);
        }

        public static Task<TResult> StartNewTask<TResult>(Func<Task<TResult>> subTask)
        {
            return InternalStartNewTask(subTask, _globalCancellationTokenSource.Token);
        }

        public static void InternalCleanup()
        {
            _globalCancellationTokenSource.Cancel();
            _globalCancellationTokenSource = new CancellationTokenSource();

            TrickTaskScheduler.Cleanup();
        }

        public static TaskAwaiter WaitForSeconds(float seconds)
        {
            return System.Threading.Tasks.Task.Delay((int)(seconds * 1000)).GetAwaiter();
        }

        public static TaskAwaiter WaitForTimeSpan(TimeSpan timeSpan)
        {
            return System.Threading.Tasks.Task.Delay(timeSpan).GetAwaiter();
        }


        public static System.Threading.Tasks.Task WhenAll(IEnumerable<Func<System.Threading.Tasks.Task>> tasks)
        {
            return System.Threading.Tasks.Task.WhenAll(tasks.Select(task => (System.Threading.Tasks.Task)StartNewTask(async () => await task())));
        }

        public static System.Threading.Tasks.Task WhenAny(IEnumerable<Func<System.Threading.Tasks.Task>> tasks)
        {
            return System.Threading.Tasks.Task.WhenAny(tasks.Select(task => (System.Threading.Tasks.Task)StartNewTask(async () => await task())));
        }

        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Func<Task<TResult>>> tasks)
        {
            return System.Threading.Tasks.Task.WhenAll(tasks.Select(task => StartNewTask(async () => await task())));
        }

        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Func<Task<TResult>>> tasks)
        {
            return System.Threading.Tasks.Task.WhenAny(tasks.Select(task => StartNewTask(async () => await task())));
        }

        public static System.Threading.Tasks.Task WhenAll(params Func<System.Threading.Tasks.Task>[] tasks)
        {
            return System.Threading.Tasks.Task.WhenAll(tasks.Select(task => (System.Threading.Tasks.Task)StartNewTask(async () => await task())));
        }

        public static System.Threading.Tasks.Task WhenAny(params Func<System.Threading.Tasks.Task>[] tasks)
        {
            return System.Threading.Tasks.Task.WhenAny(tasks.Select(task => (System.Threading.Tasks.Task)StartNewTask(async () => await task())));
        }

        public static Task<TResult[]> WhenAll<TResult>(params Func<Task<TResult>>[] tasks)
        {
            return System.Threading.Tasks.Task.WhenAll(tasks.Select(task => StartNewTask(async () => await task())));
        }

        public static Task<Task<TResult>> WhenAny<TResult>(params Func<Task<TResult>>[] tasks)
        {
            return System.Threading.Tasks.Task.WhenAny(tasks.Select(task => StartNewTask(async () => await task())));
        }

        public static async System.Threading.Tasks.Task WaitUntil(Func<bool> predicate, int spinSleep = 1)
        {
            while (predicate != null && !predicate())
            {
                await System.Threading.Tasks.Task.Delay(spinSleep);
            }
        }

        public static async System.Threading.Tasks.Task WaitWhile(Func<bool> predicate)
        {
            while (predicate != null && predicate())
            {
                await System.Threading.Tasks.Task.Delay(1);
            }
        }

        public static System.Threading.Tasks.Task WaitForEndOfFrame()
        {
            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            TrickEngine.ContainerDispatch(DispatchContainerType.WaitForEndOfFrame, () => { source.SetResult(true); });
            return source.Task;
        }

        public static System.Threading.Tasks.Task WaitForFixedUpdate()
        {
            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            TrickEngine.ContainerDispatch(DispatchContainerType.WaitForFixedUpdate, () => { source.SetResult(true); });
            return source.Task;
        }

        public static System.Threading.Tasks.Task WaitForNewFrame()
        {
            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            TrickEngine.ContainerDispatch(DispatchContainerType.WaitForNewFrame, () => { source.SetResult(true); });
            return source.Task;
        }

        /// <summary>
        /// Creates a new linked <see cref="CancellationTokenSource"/> which is linked to the Global cancellationTokenSource from TrickEngine.
        /// </summary>
        /// <returns>Returns the linked token source</returns>
        public static CancellationTokenSource NewLinkedCancellationTokenSource()
        {
            return CancellationTokenSource.CreateLinkedTokenSource(_globalCancellationTokenSource.Token);
        }
    }
}