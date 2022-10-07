namespace DevBot9.Protocols.Homie.Utilities;

public partial class PropertySwitcher : INotifyPropertyChanged, IDisposable {
    public ClientPropertyBase? HomieProperty;
    private readonly Action _updateValueAction;
    private string _deviceId = "";
    private bool _isDisposed = false;
    private bool _isPropertyFound = false;
    private string _nodeId = "";
    private string _propertyId = "";
    private HomieState _state = HomieState.Init;

    public PropertySwitcher(Action updateValueAction) {
        _updateValueAction = updateValueAction;

        HomieWatcher.Instance.Messenger.Register<DeviceUpdatedMessage>(this, HandleDeviceUpdatedMessage);
    }

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public bool IsPropertyFound {
        get { return _isPropertyFound; }
        set {
            if (_isPropertyFound != value) {
                _isPropertyFound = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsPropertyFound)));
            }
        }
    }
    public HomieState State {
        get { return _state; }
        set {
            if (_state != value) {
                _state = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }
    }

    public void Dispose() {
        // A good article explaining how to implement Dispose. https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void UpdateHomiePropertyMetadata(string deviceId, string nodeId, string propertyId, out string errorMessage) {
        _deviceId = deviceId;
        _nodeId = nodeId;
        _propertyId = propertyId;

        if (HomieWatcher.Instance.TryGetClientProperty(_deviceId, _nodeId, _propertyId, out var property, out var state)) {
            SwitchHomieProperties(property);
            errorMessage = "";
            State = state;
            IsPropertyFound = true;
        }
        else {
            errorMessage = "Cannot find Homie entity with such parameters.";
            IsPropertyFound = false;
        }
    }

    protected virtual void Dispose(bool isCalledManually) {
        if (_isDisposed == false) {
            if (isCalledManually) {
                if (HomieProperty != null) {
                    HomieProperty.PropertyChanged -= HandleHomieValueChanged;
                }
            }

            // Free unmanaged resources here and set large fields to null.

            _isDisposed = true;
        }
    }

    protected virtual void HandleHomieValueChanged(object? sender, PropertyChangedEventArgs e) {
        _updateValueAction();
    }

    protected void SwitchHomieProperties(ClientPropertyBase newProperty) {
        if (HomieProperty != null) {
            HomieProperty.PropertyChanged -= HandleHomieValueChanged;
        }

        newProperty.PropertyChanged += HandleHomieValueChanged;
        HomieProperty = newProperty;

        _updateValueAction();
    }

    private void HandleDeviceUpdatedMessage(DeviceUpdatedMessage deviceUpdatedMessage) {
        // Dispatcher.Invoke(() => {
        if (deviceUpdatedMessage.DeviceId == _deviceId) {
            if (HomieWatcher.Instance.IsConnected) {
                if (deviceUpdatedMessage.NewState == HomieState.Ready) {
                    UpdateHomiePropertyMetadata(_deviceId, _nodeId, _propertyId, out var _);
                }
                else {
                    State = deviceUpdatedMessage.NewState;
                }
            }
            else {
                IsPropertyFound = false;
            }
        };
        // });
    }
}
