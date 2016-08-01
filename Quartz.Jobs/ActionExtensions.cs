using System;

namespace Quartz.Jobs
{
    static class ActionExtensions
    {
        public static Func<TResult> RetryIfFailed<TResult>(this Func<TResult> func, int maxRetry)
        {
            return () => {
                int t = 0;
                do
                {
                    try
                    {
                        return func();
                    }
                    catch (Exception)
                    {
                        if (++t > maxRetry)
                        {
                            throw;
                        }
                    }
                } while (true);
            };
        }
    }
}
