[Export(typeof(IFunction))]
[ExportMetadata("Symbol", '+')]
class Add : IFunction
{
    public int Operate(int left, int right) => new Adder().Add((left, right));
}

[Export(typeof(IFunction))]
[ExportMetadata("Symbol", '-')]
class Subtract : IFunction
{
    public int Operate(int left, int right) => left - right;
}
[Export(typeof(IFunction))]
[ExportMetadata("Symbol", '%')]
public class Mod : IFunction
{
    public int Operate(int left, int right) => left % right;
}

internal class Adder
{
    public int Add((int left, int right) values)
    {
        return values.left + values.right;
    }
}