namespace DataBaseAbstract.Services;

public interface IModelBasic
{
    [AutoIncrement, PrimaryKey] int PrimaryKey { get; set; }
}