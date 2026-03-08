using System;

namespace Cavrnus.SpatialConnector.Core
{
    public class CavrnusDeferredDisposable : IDisposable
    {
        private IDisposable disposable;
        
        public void Set(IDisposable disposable) => this.disposable = disposable;
        public void Dispose() => disposable?.Dispose();
    }
}