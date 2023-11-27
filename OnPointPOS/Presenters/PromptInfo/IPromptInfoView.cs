using POSSUM.Base;

namespace POSSUM.Presenters.PromptInfo
{
    public interface IPromptInfoView : IBaseView
    {
        void SetMultiline(bool multiline);

        void SetTitle(string title);

        void SetDescription(string description);

        void SetValue(string value);

        string GetValue();

        void CloseWithStatus(bool success);
    }
}