using System.Linq.Expressions;

namespace DataBaseAbstract.Services;

public interface IStorageBasic<TData> where TData : IModelBasic, new()
{
    #region Transaction

    public Task BeginTransactionAsync(Action<SQLiteConnection> action);

    #endregion

    #region Create

    //插入一条新数据
    Task<int> InsertDataAsync(TData value);
    Task InsertDataAsync(params IEnumerable<TData> values);

    #endregion

    #region Read

    //通过主键获取
    Task<TData> GetDataAsync(int key);
    Task<TV> GetDataAsync<TV>(int key, Transform<TData, TV> select);

    //通过主键查询
    Task<TData?> FindDataAsync(int key);
    Task<TV?> FindDataAsync<TV>(int key, Transform<TData, TV> select);

    //根据条件查询
    IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(int limit = 0);

    IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(Transform<TData, TV> select,
        int limit = 0);

    IAsyncEnumerable<IEnumerable<TData>> SelectDatasAsync(Expression<Func<TData, bool>> predicate, int limit = 0);

    IAsyncEnumerable<IEnumerable<TV>> SelectDatasAsync<TV>(Expression<Func<TData, bool>> predicate,
        Transform<TData, TV> select,
        int limit = 0);

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
    public delegate TO Transform<in TI, out TO>(TI value);

    #endregion

    #endregion

    #region Update

    //更新一条数据
    Task UpdateDataAsync(TData value);
    Task UpdateDataAsync(params IEnumerable<TData> values);

    #endregion

    #region Delete

    Task<TData> DeleteDataAsync(int key);
    Task DeleteDataAsync(params IEnumerable<int> keys);
    Task DeleteDataAsync(Expression<Func<TData, bool>> predicate);
    Task ClearTableAsync();

    #endregion
}