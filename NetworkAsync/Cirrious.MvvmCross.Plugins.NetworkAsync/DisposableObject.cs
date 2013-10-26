using System;

namespace Cirrious.MvvmCross.Plugins.NetworkAsync
{
    public abstract class DisposableObject : IDisposable
    {
        protected bool disposed;

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        public bool IsDisposed
        {
            get { return disposed; }
        }

        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }

        #endregion
    }
}
