using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrickModule.Core
{
    public static class TrickTask
    {
        private static CancellationTokenSource _globalCancellationTokenSource = new CancellationTokenSource();

        private static async Task<KeyValuePair<System.Threading.Tasks.Task, TResult>> SubTaskWrapper<TResult>(Func<TResult> subTask)
        {
            TResult result = default(TResult);

            try
            {
                result = subTask();
                if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(typeof(TResult)))
                {
                    System.Threading.Tasks.Task castedTask = (System.Threading.Tasks.Task)(object)result;
                    await castedTask;
                    return new KeyValuePair<System.Threading.Tasks.Task, TResult>(castedTask, result);
                }
            }
            catch (TaskCanceledException e)
            {
                // Task cancelled, don't do anything
                Logger.Logger.Core.LogException(e);
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
                result = await subTask();
                if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(typeof(TResult)))
                {
                    System.Threading.Tasks.Task castedTask = (System.Threading.Tasks.Task)(object)result;
                    await castedTask;
                    return new KeyValuePair<System.Threading.Tasks.Task, TResult>(castedTask, result);
                }
            }
            catch (TaskCanceledException e)
            {
                // Task cancelled, don't do anything
                Logger.Logger.Core.LogException(e);
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);

                if (e is AggregateException ae)
                {
                    foreach (Exception exception in ae.Flatten().InnerExceptions)
                    {
                        Logger.Logger.Core.LogException(exception);
                    }
                }
            }

            return new KeyValuePair<System.Threading.Tasks.Task, TResult>(null, result);
        }

        private static async Task<TResult> InternalNewTaskAsync<TResult>(Func<TResult> subTask, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            KeyValuePair<System.Threading.Tasks.Task, TResult> innerSubTask = new KeyValuePair<System.Threading.Tasks.Task, TResult>();

            try
            {
                innerSubTask = await System.Threading.Tasks.Task.Run(() => { return SubTaskWrapper(subTask); }, cancellationToken);

                /*innerSubTask = await Task.Factory.StartNew(async () => await SubTaskWrapper(subTask), cancellationToken, creationOptions, TaskScheduler.Default).Unwrap();
            if (innerSubTask.Key == null) return innerSubTask.Value;
            await innerSubTask.Key;*/
            }
            catch (TaskCanceledException e)
            {
                // Task cancelled, don't do anything
                Logger.Logger.Core.LogException(e);
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);

                if (e is AggregateException ae)
                {
                    foreach (Exception exception in ae.Flatten().InnerExceptions)
                    {
                        Logger.Logger.Core.LogException(exception);
                    }
                }
            }

            return innerSubTask.Value;
        }

        private static async Task<TResult> InternalNewTaskAsync<TResult>(Func<Task<TResult>> subTask, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            KeyValuePair<System.Threading.Tasks.Task, TResult> innerSubTask = new KeyValuePair<System.Threading.Tasks.Task, TResult>();

            try
            {
                innerSubTask = await System.Threading.Tasks.Task.Run(() => { return SubTaskWrapper(subTask); }, cancellationToken);

                /*innerSubTask = await Task.Factory.StartNew(async () => innerSubTask = await SubTaskWrapper(subTask), cancellationToken, creationOptions, TaskScheduler.Default).Unwrap();
            if (innerSubTask.Key == null) return innerSubTask.Value;
            await innerSubTask.Key;*/
            }
            catch (TaskCanceledException e)
            {
                // Task cancelled, don't do anything
                Logger.Logger.Core.LogException(e);
            }
            catch (Exception e)
            {
                Logger.Logger.Core.LogException(e);

                if (e is AggregateException ae)
                {
                    foreach (Exception exception in ae.Flatten().InnerExceptions)
                    {
                        Logger.Logger.Core.LogException(exception);
                    }
                }
            }

            return innerSubTask.Value;
        }

        public static Task<TResult> StartNewTask<TResult>(Func<TResult> subTask, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            return InternalNewTaskAsync(subTask, cancellationToken, creationOptions);
        }

        public static Task<TResult> StartNewTask<TResult>(Func<TResult> subTask, TaskCreationOptions creationOptions = TaskCreationOptions.None)
        {
            return InternalNewTaskAsync(subTask, _globalCancellationTokenSource.Token, creationOptions);
        }

        public static Task<TResult> StartNewTask<TResult>(Func<TResult> subTask, CancellationToken cancellationToken)
        {
            return InternalNewTaskAsync(subTask, cancellationToken);
        }

        public static Task<TResult> StartNewTask<TResult>(Func<Task<TResult>> subTask)
        {
            return InternalNewTaskAsync(subTask, _globalCancellationTokenSource.Token);
        }

        public static IEnumerator ExecuteSynchronouslyCoroutine(Action func)
        {
            bool? isDone = false;

            void WrappedAction()
            {
                func?.Invoke();
                isDone = true;
            }

            if (!TrickEngine.IsMainThread)
                TrickEngine.SimpleDispatch(WrappedAction);
            else
            {
                WrappedAction();
            }

#if NO_UNITY
            yield return new TrickWaitUntil(() => isDone != null);
#else
            yield return new UnityEngine.WaitUntil(() => isDone != null);
#endif
        }

        public static Task<T> ExecuteSynchronously<T>(Func<T> func)
        {
            TaskCompletionSource<T> result = new TaskCompletionSource<T>();

            if (func != null)
            {
                if (!TrickEngine.IsMainThread)
                    TrickEngine.SimpleDispatch(() =>
                    {
                        var res = func();
                        result.SetResult(res);
                    });
                else
                {
                    var res = func();
                    result.SetResult(res);
                }
            }
            else
                result.SetResult(default(T));

            return result.Task;
        }

        public static System.Threading.Tasks.Task ExecuteSynchronously(Action func)
        {
            TaskCompletionSource<bool> result = new TaskCompletionSource<bool>();

            if (func != null)
            {
                if (!TrickEngine.IsMainThread)
                    TrickEngine.SimpleDispatch(() =>
                    {
                        func();
                        result.SetResult(true);
                    });
                else
                {
                    func();
                    result.SetResult(true);
                }
            }
            else
                result.SetResult(false);

            return result.Task;
        }

        public static async Task<TResult[]> WhenAllTimeout<TResult>(IEnumerable<Task<TResult>> tasks, TimeSpan timeout)
        {
            var timeoutTask = System.Threading.Tasks.Task.Delay(timeout).ContinueWith(_ => default(TResult));
            var completedTasks =
                (await System.Threading.Tasks.Task.WhenAll(tasks.Select(task => System.Threading.Tasks.Task.WhenAny(task, timeoutTask)))).Where(task => task != timeoutTask);
            return await System.Threading.Tasks.Task.WhenAll(completedTasks);
        }

        public static async Task<int> WhenAllTimeout(IEnumerable<System.Threading.Tasks.Task> tasks, TimeSpan timeout)
        {
            var timeoutTask = System.Threading.Tasks.Task.Delay(timeout);
            var completedTasks =
                (await System.Threading.Tasks.Task.WhenAll(tasks.Select(task => System.Threading.Tasks.Task.WhenAny(task, timeoutTask)))).Where(task => task != timeoutTask).ToList();
            await System.Threading.Tasks.Task.WhenAll(completedTasks);
            return completedTasks.Count;
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

        /// <summary>
        /// Cleanup all tasks
        /// </summary>
        internal static void InternalCleanup()
        {
            _globalCancellationTokenSource.Cancel();
            _globalCancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Update all tasks
        /// </summary>
        internal static void InternalUpdateAllTasks()
        {
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