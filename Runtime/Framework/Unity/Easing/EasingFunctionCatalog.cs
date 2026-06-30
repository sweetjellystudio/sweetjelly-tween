using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class EasingFunctionCatalog
    {
        private readonly Dictionary<string, IEasingFunction> customFunctions =
            new(StringComparer.Ordinal);

        public void Register(string name, IEasingFunction function)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Easing function name cannot be empty.", nameof(name));

            customFunctions[name] = function ?? throw new ArgumentNullException(nameof(function));
        }

        public bool TryGetCustom(string name, out IEasingFunction function)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                function = null;
                return false;
            }

            return customFunctions.TryGetValue(name, out function);
        }
    }
}