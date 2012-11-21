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
                new CheckinStateViewModel(this, "晴天", Controls.CloudStateType.Sunny, new Uri("/Resources/Wathers/sunny.jpg", UriKind.Relative), App.Current.Dispatcher),
                new CheckinStateViewModel(this, "曇天", Controls.CloudStateType.Cloudy, new Uri("/Resources/Wathers/cloudy.jpg", UriKind.Relative), App.Current.Dispatcher),
                new CheckinStateViewModel(this, "雨天", Controls.CloudStateType.Rainy, new Uri("/Resources/Wathers/rainy.jpg", UriKind.Relative), App.Current.Dispatcher),
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

        public PlaneNode CreatePlaneNode(Uri imageUrl, DateTime? raiseTime)
        { return new PlaneNode(imageUrl, this, raiseTime); }
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
            _accountModel.Field.CheckinUser(_accountModel, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }
    }
}
