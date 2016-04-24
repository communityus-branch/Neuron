using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using UnityEngine;

namespace Static_Interface.API.ConsoleFramework
{
    public class ConsoleGUI : SingletonComponent<ConsoleGUI>
    {
        public KeyCode OpenKey = KeyCode.P;
        private int _historyScrollValue;
        private int _commandIndex;

        private string _command = "";
        private bool _returnPressed;

        public GUISkin Skin = null;
        public int LinesVisible = 17;

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value)
                    return;
                _isOpen = value;
                if (_isOpen)
                {
                    InputUtil.Instance.LockInput(this);
                }
                else
                {
                    InputUtil.Instance.UnlockInput(this);
                }
            }
        }

    
        private string _partialCommand = "";

        private bool _moveCursorToEnd;

        private bool _wasCursorVisible;
        private CursorLockMode _previousLockmode;
        private int _hierarchyWidth = 150;

        protected override void Start()
        {
            base.Start();

            float height = Screen.height / 2;
            height -= Skin.box.padding.top + Skin.box.padding.bottom;
            height -= Skin.box.margin.top + Skin.box.margin.bottom;
            height -= Skin.textField.CalcHeight(new GUIContent(""),10);
            LinesVisible = (int)(height / Skin.label.CalcHeight(new GUIContent(""),10)) - 2;

            // set max line width
            float width = Screen.width - 10;
            width -= _hierarchyWidth;
            width -= Skin.verticalScrollbar.CalcSize(new GUIContent("")).x;
            Console.Instance.MaxLineWidth = (int)(width / Skin.label.CalcSize(new GUIContent("A")).x);
            
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (InputUtil.Instance.IsInputLocked(this)) return;
            GUI.skin = Skin;

            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return)
            {
                _returnPressed = true;
            }
            else
            {
                _returnPressed = false;
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
                for (int i = lines.Count - Mathf.Min(LinesVisible, lines.Count) - _historyScrollValue; i < lines.Count - _historyScrollValue; i++)
                {
                    GUILayout.Label(lines[i]);
                }
                GUILayout.EndVertical();
                if (lines.Count > LinesVisible)
                    _historyScrollValue = (int)GUILayout.VerticalScrollbar(_historyScrollValue, LinesVisible, lines.Count, 0, GUILayout.ExpandHeight(true));

 
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUI.SetNextControlName("CommandTextField");
                _command = GUILayout.TextField(_command);
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
                if (GUILayout.Button("Submit", GUILayout.ExpandWidth(false)))
                {
                    _returnPressed = true;
                }
#endif
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                if (Event.current.type == EventType.repaint && _moveCursorToEnd)
                {
                    TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (te != null)
                    {
                        te.MoveTextEnd();
                        te.cursorIndex = te.selectIndex;
                        te.graphicalCursorPos = te.style.GetCursorPixelPosition(new Rect(0f, 0f, te.position.width, te.position.height), te.content, te.cursorIndex);
                    }
                    _moveCursorToEnd = false;
                }

                if (GUI.GetNameOfFocusedControl() == "CommandTextField" && _returnPressed)
                {
                    Console.Instance.Print("> " + _command);
                    Console.Instance.Eval(_command);
                    _command = "";
                    _commandIndex = 0;
                }

                /*
                if (GUI.GetNameOfFocusedControl() == "CommandTextField" && upPressed)
                {
                    if (_commandIndex == 0)
                        _partialCommand = _command;

                    _commandIndex++;
                    var commandsCount = Console.Instance.Commands.Count();
                    if (commandsCount > 0)
                    {
                        if (_commandIndex > commandsCount) _commandIndex--;

                        _command = Console.Instance.Commands.GetItemAt((commandsCount - 1) - (_commandIndex - 1));

                        _moveCursorToEnd = true;
                    }
                }


                if (GUI.GetNameOfFocusedControl() == "CommandTextField" && downPressed)
                {
                    _commandIndex--;
                    var commandsCount = Console.Instance.Commands.Count();
                    if (_commandIndex < 0) _commandIndex = 0;

                    if (commandsCount > 0)
                    {
                        if (_commandIndex > 0)
                            _command = Console.Instance.Commands.GetItemAt((commandsCount - 1) - (_commandIndex - 1));
                        else
                            _command = _partialCommand;

                        _moveCursorToEnd = true;
                    }
                }
                */
            }

            if (!IsOpen && Event.current.type == EventType.keyUp && Event.current.keyCode == OpenKey)
            {
                IsOpen = true;
                Event.current.Use();
                Event.current.type = EventType.used;
                _wasCursorVisible = Cursor.visible;
                _previousLockmode = Cursor.lockState;
            }

            if (IsOpen)
            {
                Cursor.visible = true;    
                Cursor.lockState = CursorLockMode.None;
            }

            if (IsOpen && escPressed)
            {
                IsOpen = false;
                Cursor.visible = _wasCursorVisible;
                Cursor.lockState = _previousLockmode;
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

