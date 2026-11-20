using Vek.Charts;
namespace Vek.Docs.Models
{
#nullable enable
    public static class ApiLink
    {
        /// <summary>
        /// Gets the URL of the API documentation for a type.
        /// </summary>
        /// <param name="type">The type to find.</param>
        public static string GetApiLinkFor(Type type)
        {
            return $"api/{type.Name.Replace("`1", "").Replace("`2", "").ToLowerInvariant()}";
        }

        public static string GetComponentLinkFor(Type type)
        {
            return $"components/{GetComponentName(type)}";
        }

        /// <summary>
        /// Converts a lowercase component name from an URL into the C# Type name.
        /// Examples: 
        ///   table --> <see cref="VekTable{T}"/>
        ///   button  <see cref="VekButton"/>
        ///   appbar  <see cref="VekAppBar"/>
        /// </summary>
        public static Type? GetTypeFromComponentLink(string component)
        {
            if (string.IsNullOrEmpty(component))
            {
                return null;
            }
            if (component.Contains('#'))
            {
                component = component[..component.IndexOf('#')];
            }
            if (InverseSpecialCase.TryGetValue(component, out var type))
            {
                return type;
            }

            var assembly = typeof(VekComponentBase).Assembly;
            foreach (var componentType in assembly.GetTypes())
            {
                var typeNameWithoutGenericInfo = new string(componentType.Name.ToLowerInvariant().TakeWhile(c => c != '`').ToArray());
                if (typeNameWithoutGenericInfo.Equals($"vek{component}", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (componentType.Name.Contains('`'))
                    {
                        return componentType.MakeGenericType(typeof(T));
                    }

                    if (string.Equals(componentType.Name, $"vek{component}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return componentType;
                    }
                }
            }

            return null;
        }

        private static string GetComponentName(Type type)
        {
            if (!SpecialCaseComponents.TryGetValue(type, out var component))
            {
                component = new string(type
                        .ToString()
                        .Replace("Vek.Vek", "")
                        .TakeWhile(c => c != '`')
                        .ToArray())
                    .ToLowerInvariant();
            }

            return component;
        }

        private static readonly Dictionary<Type, string> SpecialCaseComponents =
            new()
            {
                [typeof(VekFab)] = "buttonfab",
                [typeof(VekIcon)] = "icons",
                [typeof(VekProgressCircular)] = "progress",
                [typeof(VekText)] = "typography",
                [typeof(VekSnackbarProvider)] = "snackbar",
                [typeof(Bar<T>)] = "barchart",
                [typeof(StackedBar<T>)] = "stackedbarchart",
                [typeof(Donut<T>)] = "donutchart",
                [typeof(Line<T>)] = "linechart",
                [typeof(TimeSeries<T>)] = "timeserieschart",
                [typeof(ScatterPlot<T>)] = "scatterplotchart",
                [typeof(Pie<T>)] = "piechart",
                [typeof(VekChip<T>)] = "chips",
                [typeof(ChartOptions)] = "options"
            };

        // this is the inversion of above lookup
        private static readonly Dictionary<string, Type> InverseSpecialCase =
            SpecialCaseComponents.ToDictionary(pair => pair.Value, pair => pair.Key);
    }
}
