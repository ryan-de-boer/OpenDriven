// ***********************************************************************
// Copyright (c) 2011 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using NUnit.Engine.Services;

namespace NUnit.Engine
{
    /// <summary>
    /// The ServiceContext is used by services, runners and
    /// external clients to locate the services they need through
    /// the IServiceLocator interface.
    /// </summary>
    public class ServiceContext : IServiceLocator
    {
        #region Constructor

        public ServiceContext()
        {
            ServiceManager = new ServiceManager();
        }

        #endregion

        #region Properties

        public ServiceManager ServiceManager { get; private set; }

        public int ServiceCount { get { return ServiceManager.ServiceCount; } }

        #endregion

        #region Methods

        public void Add(IService service)
        {
            ServiceManager.AddService(service);
            service.ServiceContext = this;
        }

        #endregion

        #region IServiceLocator Implementation

        public T GetService<T>() where T : class
        {
            return ServiceManager.GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return ServiceManager.GetService(serviceType);
        }

        #endregion
    }
}
