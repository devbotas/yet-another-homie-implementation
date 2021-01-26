using System;
using DevBot9.Protocols.Homie;

namespace TestApp {

    // May need those some time later.
    //private string _regexStringForInt64 = @"^-?\d{1,19}$"; // <-- It may actually overflow, need a better string.
    //private string _regexStringForSimpleFloat = @"[-+]?[0-9]*\.?[0-9]+";
    //private string _regexStringForExponentFloat = @"[-+]?[0-9]{1}\.?[0-9]+([eE][-+]?[0-9]+)?";


    //public abstract class Base {
    //    public abstract void Use();
    //    public abstract object GetProp();
    //}

    //public abstract class GenericBase<T> : Base {
    //    public T Prop { get; set; }

    //    public override object GetProp() {
    //        return Prop;
    //    }
    //}

    //public class StrBase : GenericBase<string> {
    //    public override void Use() {
    //        Console.WriteLine("Using string: {0}", Prop);
    //    }
    //}

    //public class IntBase : GenericBase<int> {
    //    public override void Use() {
    //        Console.WriteLine("Using int: {0}", Prop);
    //    }
    //}

    internal class Program {

        private static void Main(string[] args) {
            DeviceFactory.Initialize("homie-test");

            var recuperatorConsumer = new RecuperatorConsumer();
            recuperatorConsumer.Initialize();

            var recuperatorProducer = new RecuperatorProducer();
            recuperatorProducer.Initialize();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
