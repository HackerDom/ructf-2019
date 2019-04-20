using System;
using System.IO;
using NUnit.Framework;

[TestFixture]
public class ClassNameTest
{
    [Test]
    public void MethodName_StateUnderTest_ExpectedBehavior()
    {
        Console.WriteLine(Path.Join("asd", "/aads"));
        Console.WriteLine(Path.Join("asd", "../aads"));
    }
}