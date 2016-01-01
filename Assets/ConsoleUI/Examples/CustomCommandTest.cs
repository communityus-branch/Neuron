using Assets.API.Netvar;
using UnityEngine;
using Homans.Console;

public class CustomCommandTest : MonoBehaviour
{
    void Start()
    {
        Console.Instance.RegisterCommand("customCommandTest", this, "Command");

    }

    [Help("Usage: customCommandTest param\nA simple custom command test")]
    void Command(string param1)
    {
        Console.Instance.Print("Called with value " + param1);
    }




}


