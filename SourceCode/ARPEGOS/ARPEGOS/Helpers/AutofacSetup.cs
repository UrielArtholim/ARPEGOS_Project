﻿namespace ARPEGOS.Helpers
{
    using ARPEGOS.ViewModels;

    using Autofac;

    public class AutofacSetup
    {
        public IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            this.RegisterDependencies(containerBuilder);
            return containerBuilder.Build();
        }

        protected virtual void RegisterDependencies(ContainerBuilder builder)
        {
            this.RegisterViewModels(builder);
        }

        private void RegisterViewModels(ContainerBuilder builder)
        {
            builder.RegisterType<SidebarViewModel>();
            builder.RegisterType<CreationViewModel>();
        }
    }
}
