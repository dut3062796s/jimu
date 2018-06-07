﻿using Autofac;
using Jimu.Common.Discovery.ConsulIntegration;
using Jimu.Server;

namespace Jimu.Server
{
    public static class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder UseConsulForDiscovery(this IServiceHostServerBuilder serviceHostBuilder, string ip, int port, string serviceCategory, string serverAddress = null)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<ConsulServiceDiscovery>().As<IServiceDiscovery>().WithParameter("ip", ip).WithParameter("port", port).WithParameter("serviceCategory", serviceCategory).SingleInstance();
            });

            serviceHostBuilder.AddRunner(async container =>
            {
                if (container.IsRegistered<IServer>())
                {
                    IServer server = container.Resolve<IServer>();
                    var routes = server.GetServiceRoutes();
                    //IServiceDiscovery serviceDiscovery = container.Resolve<IServiceDiscovery>();
                    //serviceDiscovery.SetRoutesAsync(routes);

                    var discovery = container.Resolve<IServiceDiscovery>();
                    if (!string.IsNullOrEmpty(serverAddress))
                    {
                        await discovery.ClearAsync(serverAddress);
                    }
                    await discovery.SetRoutesAsync(routes);
                }

            });
            return serviceHostBuilder;
        }



    }
}

namespace Jimu.Client
{
    public static class ServiceHostBuilderExtension
    {
        public static IServiceHostClientBuilder UseConsulForDiscovery(this IServiceHostClientBuilder serviceHostBuilder, string ip, int port, string serviceCategory)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<ConsulServiceDiscovery>().As<IServiceDiscovery>().WithParameter("ip", ip).WithParameter("port", port).WithParameter("serviceCategory", serviceCategory).SingleInstance();
            });
            serviceHostBuilder.AddInitializer(container =>
            {
                var clientDiscovery = container.Resolve<IClientServiceDiscovery>();
                var serverDiscovery = container.Resolve<IServiceDiscovery>();
                clientDiscovery.AddRoutesGetter(serverDiscovery.GetRoutesAsync);
            });

            return serviceHostBuilder;
        }
    }
}