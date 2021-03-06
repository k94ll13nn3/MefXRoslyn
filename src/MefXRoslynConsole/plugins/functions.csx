﻿using System;

[Export(typeof(IFunction))]
[ExportMetadata("Symbol", '+')]
class Add : IFunction
{
    public int Operate(int left, int right)
    {
        var a = 0;
        Console.WriteLine("'Add' plugin");
        return new Adder().Add((left, right));
    }
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