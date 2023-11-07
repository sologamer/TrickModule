using System;
using System.Threading.Tasks;

namespace TrickModule.Core
{
    public static class TrickTaskExtensions
    {
        public static Task<T> ExecuteSynchronously<T>(this Func<T> action) => TrickTask.ExecuteSynchronously(action);
        public static Task ExecuteSynchronously(this Action action) => TrickTask.ExecuteSynchronously(action);
    }
}