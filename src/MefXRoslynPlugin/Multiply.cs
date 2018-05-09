using System.ComponentModel.Composition;
using MefXRoslynLibrary;

namespace MefXRoslynPlugin
{
    [Export(typeof(IFunction))]
    [ExportMetadata("Symbol", '*')]
    public class Mod : IFunction
    {
        public int Operate(int left, int right) => left * right;
    }
}