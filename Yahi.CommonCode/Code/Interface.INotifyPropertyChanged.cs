// nF does not implement System.ComponentModel namespace and thus does not support INotifyPropertyChanged natively.
// At first, local implementation that is below was used in all the code without #if's, but this created a problem:
// when consumed in fullNET projects, one cannot just attach GUI controls to PropertyChanged events,
// because GUI expects INotifyPropertyChanged from System.ComponentModel dll, even though interface
// signatures are identical.
//
// Thus, I'm cheating here a bit: I'm putting my poor-man's implementation under Microsoft's namespace,
// and wrapping it with an #if. This way big NET will use the native interface, and nF will use this one.
// nF doesn't have GUI handlers to bind to PropertyChanged events anyway.

#if NANOFRAMEWORK_1_0
namespace System.ComponentModel {
    public interface INotifyPropertyChanged {
        event PropertyChangedEventHandler PropertyChanged;
    }

    public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);

    public class PropertyChangedEventArgs : EventArgs {
        public string PropertyName { get; private set; }
        public PropertyChangedEventArgs(string propertyName) {
            PropertyName = propertyName;
        }
    }
}
#endif
