using System;
using System.Threading.Tasks;

namespace Core
{
    public static class TaskExtension
    {
        public static void Forget(this Task task, Action<Exception> errorHandler = null)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted && errorHandler != null)
                {
                    errorHandler(t.Exception);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}