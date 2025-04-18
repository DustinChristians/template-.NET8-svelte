﻿using System.Reflection;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using CompanyName.ProjectName.Core.Abstractions.Repositories;
using CompanyName.ProjectName.Core.Abstractions.Services;
using CompanyName.ProjectName.Core.Utilities;
using CompanyName.ProjectName.Infrastructure.Services;
using CompanyName.ProjectName.Repository.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyName.ProjectName.Mapping
{
    public class DependencyConfig
    {
        public static void Register(IServiceCollection services, IConfiguration configuration, string projectAssemblyName)
        {
            DatabaseConfig.AddDatabases(services, configuration);
            AddDependenciesAutomatically(services);
            ConfigureAutomapper(services, projectAssemblyName);
            LoggerConfig.AddDependencies(services);
        }

        // Add any Assembly Names that need to be scanned for AutoMapper Mapping Profiles here
        private static void ConfigureAutomapper(IServiceCollection services, string projectAssemblyName)
        {
            services.AddAutoMapper(
                (serviceProvider, cfg) => cfg.AddExpressionMapping(),
                Assembly.Load(projectAssemblyName),
                Assembly.Load("CompanyName.ProjectName.Infrastructure"),
                Assembly.Load("CompanyName.ProjectName.Repository"));
        }

        // The choice of seeding types used to get the exact assemblies is arbitrary as long as they
        // reside in the correct assemblies
        private static void AddDependenciesAutomatically(IServiceCollection services)
        {
            DependencyUtility.RegisterInterfaces("Service", services, Assembly.GetAssembly(typeof(IMessagesService)), Assembly.GetAssembly(typeof(MessagesService)));
            DependencyUtility.RegisterInterfaces("Repository", services, Assembly.GetAssembly(typeof(IMessagesRepository)), Assembly.GetAssembly(typeof(MessagesRepository)));
        }
    }
}
