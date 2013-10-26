// IMvxRestClient.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cirrious.MvvmCross.Plugins.NetworkAsync.Rest
{
    public interface IMvxRestClient
    {
        void ClearSetting(string key);
        void SetSetting(string key, object value);

        void MakeRequest(MvxRestRequest restRequest, Action<MvxRestResponse> successAction,
                         Action<Exception> errorAction);

        void MakeRequest(MvxRestRequest restRequest, Action<MvxStreamRestResponse> successAction,
                         Action<Exception> errorAction);

        Task<MvxRestResponse> MakeRequestAsync(MvxRestRequest restRequest, CancellationToken ct);
        Task<MvxTextRestResponse> MakeRequestTextAsync(MvxRestRequest restRequest, CancellationToken ct);
    }
}