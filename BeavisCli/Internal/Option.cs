﻿using System.Collections.Generic;
using BeavisCli.Microsoft.Extensions.CommandLineUtils;

namespace BeavisCli.Internal
{
    internal class Option : IOption
    {
        private readonly CommandOption _target;

        public Option(CommandOption target)
        {
            _target = target;
        }

        public List<string> Values => _target.Values;

        public bool HasValue()
        {
            return _target.HasValue();
        }

        public string Value()
        {
            return _target.Value();
        }
    }
}