﻿using BeavisCli.Commands;
using BeavisCli.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BeavisCli
{
    public sealed class BeavisCliOptions
    {
        public BeavisCliOptions()
        {
            BuiltInCommands = GetBuiltInCommands();
        }

        public string Path { get; set; } = "/beaviscli";

        public bool DisplayExceptions { get; set; }

        /// <summary>
        /// IUnauthorizedHandler
        /// </summary>
        public ServiceDefinition UnauthorizedHandlerService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(UnauthorizedHandler),
            ServiceType = typeof(IUnauthorizedHandler)
        };

        /// <summary>
        /// ITerminalInitializer
        /// </summary>
        public ServiceDefinition TerminalInitializerService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(TerminalInitializer),
            ServiceType = typeof(ITerminalInitializer)
        };

        /// <summary>
        /// IFileStorage
        /// </summary>
        public ServiceDefinition FileStorageService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(Services.FileStorage),
            ServiceType = typeof(IFileStorage)
        };

        /// <summary>
        /// IAuthorizationHandler
        /// </summary>
        public ServiceDefinition AuthorizationHandlerService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(AuthorizationHandler),
            ServiceType = typeof(IAuthorizationHandler)
        };

        /// <summary>
        /// ICommandProvider
        /// </summary>
        public ServiceDefinition CommandProviderService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(CommandProvider),
            ServiceType = typeof(ICommandProvider)
        };

        /// <summary>
        /// IRequestExecutor
        /// </summary>
        public ServiceDefinition RequestExecutorService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(RequestExecutor),
            ServiceType = typeof(IRequestExecutor)
        };

        /// <summary>
        /// IJobPool
        /// </summary>
        public ServiceDefinition JobPoolService { get; } = new ServiceDefinition
        {
            Lifetime = ServiceLifetime.Singleton,
            ImplementationType = typeof(JobPool),
            ServiceType = typeof(IJobPool)
        };

        public IReadOnlyDictionary<string, CommandDefinition> BuiltInCommands { get; }

        private static IReadOnlyDictionary<string, CommandDefinition> GetBuiltInCommands()
        {
            var values = new Dictionary<string, CommandDefinition>();

            void Add(Type type)
            {
                values[CommandInfo.ForType(type).Name] = new CommandDefinition { ImplementationType = type };
            }

            Add(typeof(Help));
            Add(typeof(Clear));
            Add(typeof(Reset));
            Add(typeof(Shortcuts));
            Add(typeof(License));
            Add(typeof(Upload));
            Add(typeof(Commands.FileStorage));

            return values;
        }
    }
}