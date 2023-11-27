using POSSUM.Base;

namespace POSSUM.Presenters.Login
{
    public interface ILoginView : IBaseView
    {
        string GetUsername();

        void SetFocusUsername();

        string GetPassword();

        void SetPassword(string password);

        void SetFocusPassword();

        void ShowOpening();

        void ShowLogin();

        bool GetConfirm();

        decimal GetOpeningAmount();

        void ShowIsClosed(bool isClosed);
        bool IsNewUser();
    }
}