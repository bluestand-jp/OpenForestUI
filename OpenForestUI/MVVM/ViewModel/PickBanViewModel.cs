using OpenForestUI.Common.Controllers;
using OpenForestUI.MVVM.Core;
using OpenForestUI.MVVM.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace OpenForestUI.MVVM.ViewModel
{
    public class PickBanViewModel : ObservableObject
    {
        public bool IsActive
        {
            get { return ConfigController.Component.PickBan.IsActive; }
            set { ConfigController.Component.PickBan.IsActive = value; OnPropertyChanged(); }
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; OnPropertyChanged(); }
        }

        private DelegateCommand _openCommand;

        public DelegateCommand OpenCommand
        {
            get { return _openCommand; }
        }

        private DelegateCommand _closeCommand;

        public DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
            set { _closeCommand = value; }
        }


        public PickBanViewModel()
        {

            TeamConfigViewModel.BlueTeam.Init(ConfigController.PickBan.frontend.blueTeam, "blue");
            TeamConfigViewModel.RedTeam.Init(ConfigController.PickBan.frontend.redTeam, "red");

            // Clicking the collapsed card navigates to the PickBan route; NavigateTo performs the
            // IsOpen + Home InfoButton bookkeeping this command used to do inline.
            _openCommand = new(o => App.Services.GetRequiredService<INavigationService>().NavigateTo(AppRoute.PickBan));
            _openCommand.MouseGesture = MouseAction.LeftClick;

            _closeCommand = new(o => { IsOpen = false; });
            _closeCommand.GestureKey = Key.Escape;
        }

    }
}