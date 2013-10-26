using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cirrious.MvvmCross.Plugins.NetworkAsync
{
    public static class TaskFactoryExtensions
    {
        public static async Task<TResult> FromAsync<TResult, TSource>(this TaskFactory factory,
            Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TSource state, Action<TSource> abortMethod, CancellationToken ct)
        {
            using (CancellationTokenRegistration register = ct.Register(() => abortMethod(state)))
            {
                return await factory.FromAsync(beginMethod, endMethod, state);
            }
        }
    }
}
