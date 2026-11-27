// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Vek.Utilities.Expressions;
using Collections = System.Collections;

namespace Vek
{
    /// <summary>
    /// Binds a <see cref="VekDataGrid{T}"/> column to an object property, with automatic sorting, filtering, and formatting of its values.
    /// </summary>
    /// <typeparam name="T">The type of object represented by each row in the data grid.</typeparam>
    /// <typeparam name="TProperty">The type of the property whose values are displayed in the column's cells.</typeparam>
    /// <seealso cref="Column{T}" />
    /// <seealso cref="VekDataGrid{T}" />
    /// <seealso cref="TemplateColumn{T}" />
    public partial class PropertyColumn<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T, TProperty> : Column<T>
    {
        private readonly Guid _id = Guid.NewGuid();

        private string? _propertyName;
        private Func<T, object?>? _cellContentFunc;
        private Func<T, TProperty>? _compiledPropertyFunc;
        private Expression<Func<T, TProperty>>? _lastAssignedProperty;
        private Expression<Func<T, TProperty>>? _compiledPropertyFuncFor;

        /// <summary>
        /// The property whose values are displayed in the column.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public Expression<Func<T, TProperty>> Property { get; set; } = Expression.Lambda<Func<T, TProperty>>(Expression.Default(typeof(TProperty)), Expression.Parameter(typeof(T)));

        /// <summary>
        /// The format applied to property values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        public string? Format { get; set; }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            // We have to do a bit of pre-processing on the lambda expression. Only do that if it's new or changed.
            if (_lastAssignedProperty != Property)
            {
                _lastAssignedProperty = Property;
                _compiledPropertyFunc = null;
                _compiledPropertyFuncFor = null;
                var safePropertyExpression = ExpressionNull.AddNullChecks(Property);
                var compiledPropertyExpression = safePropertyExpression.Compile();
                _cellContentFunc = item => compiledPropertyExpression(item);
            }

            var property = PropertyPath.Visit(Property);
            if (property.IsBodyMemberExpression)
            {
                _propertyName = property.GetPath();
            }
            else
            {
                // Most likely this is a dynamic expression that people use as workaround https://github.com/madiseh/vek/snippet/cYGxuTmhyqAQeCVM
                // We can't assign any meaningful name at all, therefore we should assign an unique ID like we do for TemplateColumn
                _propertyName = _id.ToString();
            }
            Title ??= property.GetLastMemberName();

            CompileGroupBy();
        }

        protected internal override LambdaExpression? PropertyExpression
            => Property;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public override string? PropertyName
            => _propertyName;

        protected internal override string? ContentFormat
            => Format;

        protected internal override object? CellContent(T item)
            => _cellContentFunc!(item);

        internal override RenderFragment? RenderHeaderFilterPopup()
        {
            if (FilterTemplate is null && (typeof(TProperty) == typeof(string) || typeof(TProperty).IsEnum || Nullable.GetUnderlyingType(typeof(TProperty))?.IsEnum == true))
            {
                return null;
            }

            if (FilterTemplate is not null)
            {
                return builder => builder.AddContent(0, FilterTemplate(FilterContext));
            }

            return builder =>
            {
                builder.OpenComponent<DataGridHeaderFilterPopup<T, TProperty>>(0);
                builder.AddAttribute(1, nameof(DataGridHeaderFilterPopup<T, TProperty>.Items), DataGrid!.Items?.ToArray() ?? Array.Empty<T>());
                builder.AddAttribute(2, nameof(DataGridHeaderFilterPopup<T, TProperty>.GetValue), (Func<T, TProperty>)GetTypedPropertyValue);
                builder.AddAttribute(3, nameof(DataGridHeaderFilterPopup<T, TProperty>.FilterDefinition), FilterContext.FilterDefinition!);
                builder.AddAttribute(4, nameof(DataGridHeaderFilterPopup<T, TProperty>.SelectedValuesChanged), EventCallback.Factory.Create<IReadOnlyCollection<TProperty?>?>(this, SetHeaderFilterValuesAsync));
                builder.AddAttribute(5, nameof(DataGridHeaderFilterPopup<T, TProperty>.FilterChanged), EventCallback.Factory.Create<IFilterDefinition<T>>(this, ApplyHeaderFilterAsync));
                builder.AddAttribute(6, nameof(DataGridHeaderFilterPopup<T, TProperty>.SearchLabel), "Search values");
                builder.CloseComponent();
            };
        }

        private TProperty GetTypedPropertyValue(T item)
        {
            if (_compiledPropertyFunc == null || _compiledPropertyFuncFor != Property)
            {
                _compiledPropertyFunc = Property.Compile();
                _compiledPropertyFuncFor = Property;
            }

            return _compiledPropertyFunc(item);
        }

        private Task SetHeaderFilterValuesAsync(IReadOnlyCollection<TProperty?>? values)
        {
            var filterDefinition = FilterContext.FilterDefinition!;
            var selected = values?.ToArray() ?? [];
            filterDefinition.Value = selected.Length == 0 ? null : selected;
            if (filterDefinition is FilterDefinition<T> definition)
            {
                definition.FilterFunction = selected.Length == 0 ? null : item => selected.Contains(GetTypedPropertyValue(item));
            }

            return Task.CompletedTask;
        }

        private Task ApplyHeaderFilterAsync(IFilterDefinition<T> definition)
        {
            if (definition.Value is not Collections.IEnumerable selected || definition.Value is string)
            {
                definition.Value = null;
                if (definition is FilterDefinition<T> emptyDefinition)
                {
                    emptyDefinition.FilterFunction = null;
                }
            }
            else if (definition is FilterDefinition<T> typedDefinition)
            {
                var values = selected.Cast<object?>().ToArray();
                typedDefinition.FilterFunction = item => values.Contains(PropertyFunc(item));
            }

            return FilterContext.Actions.ApplyFilterAsync(definition);
        }

        protected internal override object? PropertyFunc(T item)
        {
            if (_compiledPropertyFunc == null || _compiledPropertyFuncFor != Property)
            {
                _compiledPropertyFunc = Property.Compile();
                _compiledPropertyFuncFor = Property;
            }

            return _compiledPropertyFunc(item);
        }

        protected internal override Type PropertyType
            => typeof(TProperty);

        /// <summary>
        /// If the Property expression looks like 'x => x.y.z', to set the value of 'z' we need to find the value of 'y', we can dig recursively until we find it.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private object? RecursiveGetSubProperties(MemberExpression memberExpression, object? item)
        {
            if (memberExpression.Expression is MemberExpression { Member: PropertyInfo propertyInfo } subMemberExpress)
            {
                var subObject = RecursiveGetSubProperties(subMemberExpress, item);

                return propertyInfo.GetValue(subObject) ?? throw new NullReferenceException($"Unable to get property value, value of '{propertyInfo.Name}' is null in '{Property}'");
            }

            return item;
        }

        protected internal override void SetProperty(object? item, object? value)
        {
            var expression = Property.Body;

            // Only MemberExpression is supported, MemberExpression access members like 'x.y' is accessing the member 'y'
            if (expression is MemberExpression memberExpression)
            {
                item = RecursiveGetSubProperties(memberExpression, item);

                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    if (value == null) // We can't use the normal execution path for null values, because we can't use Convert.ChangeType with null for types such as int?, double?, etc
                        propertyInfo.SetValue(item, null);
                    else
                    {
                        var actualType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? PropertyType;
                        propertyInfo.SetValue(item, Convert.ChangeType(value, actualType), null);
                    }
                }
            }
        }
    }
}
