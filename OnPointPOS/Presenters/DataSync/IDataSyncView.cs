using POSSUM.Base;

namespace POSSUM.Presenters.DataSync
{
    public interface IDataSyncView : IBaseView
    {
        void Message(string message);
    }
}