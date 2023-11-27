using System.Windows;

namespace POSSUM.Presenters.PromptInfo
{
    public class PromptInfoPresenter
    {
        public enum PromptType
        {
            TextSingleLine,
            TextMultiLine
        }

        private readonly PromptInfoConfig _config;
        private readonly IPromptInfoView _view;

        public PromptInfoPresenter(IPromptInfoView view, PromptInfoConfig config)
        {
            _view = view;
            _config = config;
            switch (config.PromptType)
            {
                case PromptType.TextSingleLine:
                    {
                        view.SetMultiline(false);
                    }
                    break;
                case PromptType.TextMultiLine:
                    {
                        view.SetMultiline(true);
                    }
                    break;
            }
            view.SetTitle(config.Title);
            view.SetDescription(config.Description);
            view.SetValue(config.Value);
        }

        public void HandleOk()
        {            
            _config.Value = _view.GetValue();
            if (_config.Value.Length>50)
            {
                MessageBox.Show("Order Comments can not be more then 50 characters.", "Order Comments", MessageBoxButton.OK, MessageBoxImage.Warning);
                return ;
            }
            _view.CloseWithStatus(true);
        }

        public void HandleCancel()
        {
            _view.CloseWithStatus(false);
        }

        public class PromptInfoConfig
        {
            public string Description;
            public PromptType PromptType;
            public string Title;
            public string Value;
        }
    }
}