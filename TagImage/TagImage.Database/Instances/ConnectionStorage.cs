using DataBaseAbstract.Storage;
using TagImage.Database.Models;

namespace TagImage.Database.Instances;

internal class ConnectionStorage() : StorageBasic<ConnectionEntry>(nameof(TagImage))
{

}