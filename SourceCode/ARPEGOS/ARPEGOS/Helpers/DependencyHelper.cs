namespace ARPEGOS.Helpers
{
    using ARPEGOS.Configuration;
    using ARPEGOS.Services;
    using ARPEGOS.Services.Interfaces;
    using ARPEGOS.ViewModels;

    using Autofac;

    public class DependencyHelper
    {
        public static IContainer Container { get; private set; }

        public static Context CurrentContext => Container.Resolve<Context>();

        public DependencyHelper()
        {
            Container = this.CreateContainer();
        }

        public IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            this.RegisterDependencies(containerBuilder);
            return containerBuilder.Build();
        }

        protected virtual void RegisterDependencies(ContainerBuilder builder)
        {
            this.RegisterServices(builder);
            this.RegisterViewModels(builder);
            builder.RegisterType<Context>().SingleInstance();
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DialogService>().As<IDialogService>();
        }

        private void RegisterViewModels(ContainerBuilder builder)
        {
            builder.RegisterType<MainViewModel>().SingleInstance();
            builder.RegisterType<SelectVersionViewModel>().SingleInstance();
        }
    }
}
