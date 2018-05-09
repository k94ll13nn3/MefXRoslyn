using System.ComponentModel.Composition;

namespace MefXRoslynLibrary
{
    [InheritedExport(typeof(IFunction))]
    public interface IFunction
    {
        int Operate(int left, int right);
    }

    public interface IFunctionData
    {
        char Symbol { get; }
    }
}