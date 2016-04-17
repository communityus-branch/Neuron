/**
 * Author: Sander Homan
 * Copyright 2012
 **/

using UnityEngine;

namespace Plugins.ConsoleUI.FrontEnd.UnityGUI
{
    public class ConsoleGUI : MonoBehaviour
    {
        public KeyCode openKey = KeyCode.P;
        private int historyScrollValue;
        private int commandIndex = 0;

        private string command = "";
        private bool returnPressed = false;

        public GUISkin skin = null;
        public int linesVisible = 17;

        public bool IsOpen = false;
        private string partialCommand = "";

        private bool moveCursorToEnd;

        public bool showHierarchy = true;

        private string[] displayObjects = null;
        private string[] displayComponents = null;
        private Vector2 hierarchyScrollValue;
        private Vector2 componentScrollValue;

        private int commandLastPos;
        private int commandLastSelectPos;

        private string[] displayMethods = null;
        private Vector2 methodScrollValue;
        private bool wasCursorVisible;
        private CursorLockMode previousLockmode;
        private bool wasControllerEnabled;
        private int hierarchyWidth = 150;

        public static ConsoleGUI Instance;
        public bool InputLocked { get; set; }
        void Start()
        {
            Instance = this;
            //InvokeRepeating("PrintLine", 2, 0.1f);
            //Console.Instance.RegisterCommand("printchildren", this, "PrintChildren");
            //Console.Console.Instance.RegisterCommand("printcomponents", this, "PrintComponents");
            //Console.Instance.RegisterCommand("testParse", this, "TestParse");

            displayObjects = Console.Instance.GetGameobjectsAtPath("/");
            displayComponents = Console.Instance.GetComponentsOfGameobject("/");
            displayMethods = Console.Instance.GetMethodsOfComponent("/");

            float height = Screen.height / 2;
            height -= skin.box.padding.top + skin.box.padding.bottom;
            height -= skin.box.margin.top + skin.box.margin.bottom;
            height -= skin.textField.CalcHeight(new GUIContent(""),10);
            linesVisible = (int)(height / skin.label.CalcHeight(new GUIContent(""),10)) - 2;

            // set max line width
            float width = Screen.width - 10;
            width -= hierarchyWidth;
            width -= skin.verticalScrollbar.CalcSize(new GUIContent("")).x;
            Console.Instance.maxLineWidth = (int)(width / skin.label.CalcSize(new GUIContent("A")).x);
            
        }

        void OnGUI()
        {
            if (InputLocked) return;
            GUI.skin = skin;

            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return)
            {
                returnPressed = true;
            }
            else
            {
                returnPressed = false;
            }

            bool upPressed = false;
            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.UpArrow)
            {
                upPressed = true;
                Event.current.Use();
            }

            bool downPressed = false;
            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.DownArrow)
            {
                downPressed = true;
                Event.current.Use();
            }

            bool escPressed = false;
            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Escape)
            {
                escPressed = true;
                Event.current.Use();
            }

            if (IsOpen)
            {
                GUI.depth = -100;
                GUILayout.BeginArea(new Rect(5, 5, Screen.width - 10, Screen.height / 2), (GUIStyle)"box");
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                var lines = Console.Instance.Lines;
                // display last 10 lines
                for (int i = lines.Count() - Mathf.Min(linesVisible, lines.Count()) - historyScrollValue; i < lines.Count() - historyScrollValue; i++)
                {
                    GUILayout.Label(lines.GetItemAt(i));
                }
                GUILayout.EndVertical();
                if (lines.Count() > linesVisible)
                    historyScrollValue = (int)GUILayout.VerticalScrollbar(historyScrollValue, linesVisible, lines.Count(), 0, GUILayout.ExpandHeight(true));

 
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUI.SetNextControlName("CommandTextField");
                string oldCommand = command;
                command = GUILayout.TextField(command);
                if (command != oldCommand)
                {
                    displayObjects = Console.Instance.GetGameobjectsAtPath(command);
                    displayComponents = Console.Instance.GetComponentsOfGameobject(command);
                    displayMethods = Console.Instance.GetMethodsOfComponent(command);
                    TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (te != null)
                    {
                        commandLastPos = te.cursorIndex;
                        commandLastSelectPos = te.selectIndex;
                    }
                }
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
                if (GUILayout.Button("Submit", GUILayout.ExpandWidth(false)))
                {
                    returnPressed = true;
                }
#endif
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                if (Event.current.type == EventType.repaint && moveCursorToEnd)
                {
                    TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (te != null)
                    {
                        te.MoveTextEnd();
                        te.cursorIndex = te.selectIndex;
                        te.graphicalCursorPos = te.style.GetCursorPixelPosition(new Rect(0f, 0f, te.position.width, te.position.height), te.content, te.cursorIndex);
                        commandLastPos = te.cursorIndex;
                        commandLastSelectPos = te.selectIndex;
                    }
                    moveCursorToEnd = false;
                }

                if (GUI.GetNameOfFocusedControl() == "CommandTextField" && returnPressed)
                {
                    Console.Instance.Print("> " + command);
                    Console.Instance.Eval(command);
                    command = "";
                    commandIndex = 0;
                    displayObjects = Console.Instance.GetGameobjectsAtPath(command);
                    displayComponents = Console.Instance.GetComponentsOfGameobject(command);
                }

                if (GUI.GetNameOfFocusedControl() == "CommandTextField" && upPressed)
                {
                    if (commandIndex == 0)
                        partialCommand = command;

                    commandIndex++;
                    var commandsCount = Console.Instance.Commands.Count();
                    if (commandsCount > 0)
                    {
                        if (commandIndex > commandsCount) commandIndex--;

                        command = Console.Instance.Commands.GetItemAt((commandsCount - 1) - (commandIndex - 1));

                        moveCursorToEnd = true;
                    }
                }

                if (GUI.GetNameOfFocusedControl() == "CommandTextField" && downPressed)
                {
                    commandIndex--;
                    var commandsCount = Console.Instance.Commands.Count();
                    if (commandIndex < 0) commandIndex = 0;

                    if (commandsCount > 0)
                    {
                        if (commandIndex > 0)
                            command = Console.Instance.Commands.GetItemAt((commandsCount - 1) - (commandIndex - 1));
                        else
                            command = partialCommand;

                        moveCursorToEnd = true;
                    }
                }
            }

            if (!IsOpen && Event.current.type == EventType.keyUp && Event.current.keyCode == openKey)
            {
                IsOpen = true;
                Event.current.Use();
                Event.current.type = EventType.used;
                wasCursorVisible = Cursor.visible;
                previousLockmode = Cursor.lockState;
            }

            if (IsOpen)
            {
                Cursor.visible = true;    
                Cursor.lockState = CursorLockMode.None;
            }

            if (IsOpen && escPressed)
            {
                IsOpen = false;
                Cursor.visible = wasCursorVisible;
                Cursor.lockState = previousLockmode;
            }

            // refocus the textfield if focus is lost
            if (IsOpen && Event.current.type == EventType.layout && GUI.GetNameOfFocusedControl() != "CommandTextField")
            {
                GUI.FocusControl("CommandTextField");
                TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                if (te != null)
                {
                    te.SelectNone();
                    te.MoveTextEnd();
                }
            }
        }
    }
}

