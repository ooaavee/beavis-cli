﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace BeavisCli.Services
{
    public struct ServiceDefinition
    {
        public ServiceLifetime Lifetime { get; set; }

        public Type ImplementationType { get; set; }

        public Type ServiceType { get; set; }
    }
}