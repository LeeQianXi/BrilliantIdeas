namespace DataBaseAbstract.Services;

public interface IStorageBasic<TData> where TData : IModelBasic, new()
{
    #region Transaction

    public Task BeginTransactionAsync(Action<SQLiteConnection> action, CancellationToken token = default);

    #endregion

    #region Create

    //插入一条新数据
    Task<int> InsertDataAsync(TData value, CancellationToken token = default);
    Task InsertDataAsync(CancellationToken token = default, params IEnumerable<TData> values);

    #endregion

    #region Read

    //通过主键获取
    Task<TData> GetDataAsync(int key, CancellationToken token = default);
    Task<TV> GetDataAsync<TV>(int key, Expression<Func<TData, TV>> select, CancellationToken token = default);

    //通过主键查询
    Task<TData?> FindDataAsync(int key, CancellationToken token = default);
    Task<TV?> FindDataAsync<TV>(int key, Expression<Func<TData, TV>> select, CancellationToken token = default);

    //根据条件查询
    IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(int limit = 0, CancellationToken token = default);

    IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(Expression<Func<TData, TV>> select,
        int limit = 0, CancellationToken token = default);

    IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(Expression<Func<TData, bool>> predicate, int limit = 0,
        CancellationToken token = default);

    IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(Expression<Func<TData, bool>> predicate,
        Expression<Func<TData, TV>> select,
        int limit = 0, CancellationToken token = default);

    #region UDAF

/*
    Task<double> Sum(string column = "*", Predicate<TData>? filter = null, Transform<object, double> select = null);
    Task<double> Max(string column = "*", Predicate<TData>? filter = null, Transform<object, double> select = null);
    Task<double> Min(string column = "*", Predicate<TData>? filter = null, Transform<object, double> select = null);
    Task<double> Avg(string column = "*", Predicate<TData>? filter = null, Transform<object, double> select = null);
    Task<TData> First(string column = "*", Predicate<TData>? filter = null);
    Task<TData> Last(string column = "*", Predicate<TData>? filter = null);
    Task<int> Count(string column = "*", Predicate<TData>? filter = null);
*/

    #endregion

    #endregion

    #region Update

    //更新一条数据
    Task UpdateDataAsync(TData value, CancellationToken token = default);
    Task UpdateDataAsync(CancellationToken token = default, params IEnumerable<TData> values);

    #endregion

    #region Delete

    Task<TData> DeleteDataAsync(int key, CancellationToken token = default);
    Task DeleteDataAsync(CancellationToken token = default, params IEnumerable<int> keys);
    Task DeleteDataAsync(Expression<Func<TData, bool>> predicate, CancellationToken token = default);
    Task ClearTableAsync(CancellationToken token = default);

    #endregion
}