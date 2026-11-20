using System;
using System.Collections.Generic;
using System.Linq;

namespace Vek.Docs.Models
{
    public class DocsComponents
    {
        private readonly List<VekComponent> _vekComponents = new();

        public DocsComponents AddItem(string name, Type component, params Type[] childComponents)
        {
            return AddItem(name, component, null, childComponents);
        }

        public DocsComponents AddItem(string name, Type component, string link, params Type[] childComponents)
        {
            var componentItem = new VekComponent
            {
                Name = name,
                Link = link ?? name.ToLowerInvariant().Replace(" ", ""),
                Type = component,
                ChildTypes = childComponents,
                IsNavGroup = false
            };
            _vekComponents.Add(componentItem);

            return this;
        }

        public DocsComponents AddNavGroup(string name, bool expanded, DocsComponents groupItems, bool sortByName = true)
        {
            var componentItem = new VekComponent
            {
                Name = name,
                NavGroupExpanded = expanded,
                GroupComponents = sortByName ? groupItems.GetComponentsSortedByName() : groupItems.Components,
                IsNavGroup = true
            };
            _vekComponents.Add(componentItem);

            return this;
        }

        internal List<VekComponent> Components => _vekComponents;

        internal List<VekComponent> GetComponentsSortedByName()
        {
            return _vekComponents.OrderBy(e => e.Name).ToList();
        }
    }
}
