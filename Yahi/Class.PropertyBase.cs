using System.ComponentModel;

namespace DevBot9.Protocols.Homie {
    public class PropertyBase : INotifyPropertyChanged {
        protected string _propertyId;
        protected Device _parentDevice;


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void RaisePropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged(sender, e);
        }

        internal virtual void Initialize(Device parentDevice) { }
    }
}
