using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace TestApp {
    public class Program {
        public static void Main() {
            // Before deploying, configure WROVER KIT WiFi connection in the Device Explorer (need to do that only once). 
            WaitForIP();

            var brokerIp = "172.16.0.2";
            var lightbulbProducer = new LightbulbProducer();
            lightbulbProducer.Initialize(brokerIp);

            while (true) {
                Thread.Sleep(1000);
            }
        }

        static void WaitForIP() {
            Debug.WriteLine("Waiting for IP...");

            while (true) {
                var ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                if ((ni.IPv4Address != null) && (ni.IPv4Address.Length > 0)) {
                    if (ni.IPv4Address[0] != '0') {
                        Debug.WriteLine($"We have an IP: {ni.IPv4Address}");
                        break;
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}
