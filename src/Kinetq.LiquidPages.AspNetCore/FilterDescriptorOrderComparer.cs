using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class FilterDescriptorOrderComparer : IComparer<FilterDescriptor>
{
    public static FilterDescriptorOrderComparer Comparer { get; } = new FilterDescriptorOrderComparer();

    public int Compare(FilterDescriptor? x, FilterDescriptor? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        if (x.Order == y.Order)
        {
            return x.Scope.CompareTo(y.Scope);
        }
        else
        {
            return x.Order.CompareTo(y.Order);
        }
    }
}