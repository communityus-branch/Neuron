using UnityEngine;
using Homans.Console;

public struct TestObject
{
    public int x;
    public int y;
}

public class CustomParseTest : MonoBehaviour
{
    void Start()
    {
        Console.Instance.RegisterCommand("customParseTest", this, "Command");
        Console.Instance.RegisterParser(typeof(TestObject), ParseTestObject);
    }

    [Help("Usage: customParseTest param\nA simple custom parse test")]
    void Command(TestObject param1)
    {
        Console.Instance.Print("Called with value " + param1.x + ", " + param1.y);
    }

    public bool ParseTestObject(string line, out object obj)
    {
        string[] s = line.Split(',');
        if (s.Length != 2)
        {
            obj = null;
            return false;
        }

        int x;
        if (!int.TryParse(s[0], out x))
        {
            obj = null;
            return false;
        }
        int y;
        if (!int.TryParse(s[1], out y))
        {
            obj = null;
            return false;
        }

        TestObject result = new TestObject();
        result.x = x;
        result.y = y;
        obj = result;
        return true;
    }
}
