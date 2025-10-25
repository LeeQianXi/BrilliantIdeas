using DataBaseAbstract.Storage;
using TagImage.Database.Models;

namespace TagImage.Database.Instances;

internal class TagStorage() : StorageBasic<TagEntry>(nameof(TagImage))
{

}