import Homans.Console;

// Use this for initialization
function Start () {
	Console.Instance.RegisterCommand("customCommandTest", this, "Command");
}

@Help("Usage: customCommandTest param\nA simple custom command test")
function Command (param1: String) {
	Console.Instance.Print("Called with value " + param1);
}

