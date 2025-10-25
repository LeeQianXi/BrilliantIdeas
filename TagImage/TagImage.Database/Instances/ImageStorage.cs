using DataBaseAbstract.Storage;
using TagImage.Database.Models;

namespace TagImage.Database.Instances;

internal class ImageStorage() : StorageBasic<ImageEntry>(nameof(TagImage))
{

}