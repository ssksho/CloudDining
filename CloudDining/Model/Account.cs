using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace CloudDining.Model
{
    public class Account
    {
        public Account(string name, Uri icon, FieldManager field)
        {
            Field = field;
            Name = name;
            Icon = icon;
            StatusSelecter = new List<CheckinStateViewModel>()
            {
                new CheckinStateViewModel(this, null, Controls.CloudStateType.Sunny, new Uri("/Resources/Wathers/sunny.png", UriKind.Relative), App.Current.Dispatcher),
                new CheckinStateViewModel(this, null, Controls.CloudStateType.Rainy, new Uri("/Resources/Wathers/rainy.png", UriKind.Relative), App.Current.Dispatcher),
                new CheckinStateViewModel(this, null, Controls.CloudStateType.Thunder, new Uri("/Resources/Wathers/thunder.png", UriKind.Relative), App.Current.Dispatcher),
            };
        }
        
        public FieldManager Field { get; private set; }
        public string Name { get; private set; }
        public Uri Icon { get; private set; }
        public Controls.CloudStateType Weather { get; set; }
        public List<CheckinStateViewModel> StatusSelecter { get; private set; }
        public List<PlaneNode> ReadedPictures
        {
            get
            {
                return Field.TimelineNodes
                    .Where(node => node.IsOpened == false && node is PlaneNode)
                    .Cast<PlaneNode>().ToList();
            }
        }

        public void PostPlane(Uri imageUrl, DateTime? raiseTime)
        {
            var res = PlaneNode.CreatePlanePair(imageUrl, this, raiseTime);
            var home = res[0];
            var time = res[1];
            Field.PostPlane(home, time);
        }
    }
    public class CheckinStateViewModel : ViewModel.ViewModelBase
    {
        public CheckinStateViewModel(Account accountModel, string name, Controls.CloudStateType type, Uri icon, Dispatcher uiDispatcher)
            : base(uiDispatcher)
        {
            _accountModel = accountModel;
            _weather = type;
            Name = name;
            Icon = icon;
            SelecteStatusCommand = new ViewModel.RelayCommand(SelectedStatus_Executed);
        }
        Account _accountModel;
        Controls.CloudStateType _weather;
        public string Name { get; private set; }
        public Uri Icon { get; private set; }
        public ICommand SelecteStatusCommand { get; private set; }

        void SelectedStatus_Executed(object param)
        {
            _accountModel.Weather = _weather;
            _accountModel.Field.CheckinUser(_accountModel);
        }
    }
}
