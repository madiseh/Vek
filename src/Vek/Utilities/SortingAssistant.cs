// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
using System.Reflection;

namespace Vek.Utilities;

public static class SortingAssistant
{
    public static void UpdateOrder<T>(this IEnumerable<T> items, VekItemDropInfo<T> dropInfo, Expression<Func<T, int>> valueUpdater, int zoneOffset = 0)
    {
        if (valueUpdater.Body is not MemberExpression memberSelectorExpression) { throw new InvalidOperationException(); }

        var property = memberSelectorExpression.Member as PropertyInfo;

        if (property == null) { throw new InvalidOperationException(); }

        var newIndex = dropInfo.IndexInZone + zoneOffset;

        var item = dropInfo.Item;

        var index = 0;
        foreach (var currentItem in items.OrderBy(x => (int?)property.GetValue(x)))
        {
            if (currentItem is not null && currentItem.Equals(item))
            {
                property.SetValue(item, newIndex);
            }
            else
            {
                if (index == newIndex)
                {
                    index++;
                }

                property.SetValue(currentItem, index);

                index++;
            }
        }
    }
}
