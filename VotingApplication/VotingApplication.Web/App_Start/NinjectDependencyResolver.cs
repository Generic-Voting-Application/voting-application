using Ninject;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http.Dependencies;

namespace VotingApplication.Web.Common
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _container;

        public IKernel Container
        {
            get { return _container; }
        }

        public NinjectDependencyResolver(IKernel container)
        {
            _container = container;
        }

        public void Dispose()
        {
            //Do Nothing
        }

        public object GetService(Type serviceType)
        {
            //TryGet to properly handle missing services
            return _container.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAll(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }
    }
}
