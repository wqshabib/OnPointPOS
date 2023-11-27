namespace POSSUM.Presenters.PromptInfo
{
    public interface IPromptInfoAmountView
    {
        decimal GetCashAmount();
        void ShowError(string message, string title);
        void ClosePrompt();
        void SuccessMessage(string message);
    }
}