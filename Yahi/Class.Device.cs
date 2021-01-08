using System.Threading;

namespace DevBot.Homie {
    public class Device {
        private IBroker _broker;

        public string Id { get; set; }

        public string HomieVersion { get; } = "4.0.0";
        public string Name { get; set; }
        public string State { get; set; }
        public Device() { }


        public void Initialize(IBroker clientModel) {
            _broker = clientModel;

            SetState(States.Init);

            _broker.Publish($"homie/{Id}/$homie", HomieVersion);
            _broker.Publish($"homie/{Id}/$name", Name);
            //_broker.Publish($"homie/{Id}/$nodes", GetNodesString());
            //_broker.Publish($"homie/{Id}/$extensions", GetExtensionsString());

            // imitating some initialization work.
            Thread.Sleep(5000);

            SetState(States.Ready);
        }

        public void SetState(string stateToSet) {
            State = stateToSet;
            _broker.Publish($"homie/{Id}/$state", State);
        }
    }
}
