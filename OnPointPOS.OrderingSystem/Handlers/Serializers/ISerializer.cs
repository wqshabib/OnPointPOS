using System.IO;

namespace ML.Common.Handlers.Serializers
{
    public interface ISerializer
    {
        Stream Serialize(object dto);
    }
}