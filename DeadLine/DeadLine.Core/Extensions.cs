using DIAbstract;
using FluentValidation;

namespace DeadLine.Core;

public static class Extensions
{
    public static IObservable<TResult> LiveStat<T, TResult>(
        this IObservable<IChangeSet<T>> changes,
        Func<IReadOnlyCollection<T>, TResult> query) where T : notnull
    {
        return changes.QueryWhenChanged(query).DistinctUntilChanged();
    }

    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddMultiSingleton<Application, DeadLineApp>()
                .AddMultiSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseDeadLineCore()
        {
            return collection
                .AddValidatorsFromAssemblyContaining<NewDeadLineItemValidator>(includeInternalTypes: true)
                .AddSingleton<IDeadLineWindow, DeadLineWindow>(p =>
                    (DeadLineWindow)p.GetRequiredService<IStartupWindow>())
                .AddMultiSingleton<IDeadLineViewModel, DeadLineViewModel>()
                .AddTransient<INewDeadLineItemView, NewDeadLineItemWindow>()
                .AddTransient<INewDeadLineItemViewModel, NewDeadLineItemViewModel>()
                .AddTransient<IEditItemInfoWindow, EditItemInfoWindow>();
        }
    }
}