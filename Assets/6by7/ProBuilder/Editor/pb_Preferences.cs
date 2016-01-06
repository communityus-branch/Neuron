using UnityEngine;
using UnityEditor;

public class pb_Preferences
{
	private static bool prefsLoaded = false;
	
	// ProBuilder Preferences
	static string defaultEditMode;
	static pb_Editor.EditMode _editMode;

	static string defaultSelectionMode;
	static ProBuilder.SelectMode _selectMode;

	static Color _faceColor;
	static string defaultFaceColor;

	static bool defaultOpenInDockableWindow;

	static bool defaultHideFaceMask;

	static Vector2 settingsScroll = Vector2.zero;

	static ProBuilder.Shortcut[] defaultShortcuts;

	[PreferenceItem ("BuildControl")]
	public static void PreferencesGUI () {
		// Load the preferences
		if (!prefsLoaded) {
			LoadPrefs();
			prefsLoaded = true;
			OnWindowResize();
		}
		
		settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.MaxHeight(136));
		// Geometry Settings
		GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);


		_editMode = (pb_Editor.EditMode)EditorGUILayout.EnumPopup("Default Edit Mode", _editMode);

		_selectMode = (ProBuilder.SelectMode)EditorGUILayout.EnumPopup("Default Selection Mode", _selectMode);

		_faceColor = EditorGUILayout.ColorField("Selected Face Color", _faceColor);
		
		defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);

		GUILayout.Space(4);

		GUILayout.Label("Texture Editing Settings", EditorStyles.boldLabel);

		defaultHideFaceMask = EditorGUILayout.Toggle("Hide face mask", defaultHideFaceMask);

		EditorGUILayout.EndScrollView();

		GUILayout.Space(4);

		GUILayout.Label("Shortcut Settings", EditorStyles.boldLabel);

		if(GUI.Button(resetRect, "Use defaults"))
			ResetToDefaults();

		ShortcutSelectPanel();
		ShortcutEditPanel();

		// Save the preferences
		if (GUI.changed)
			SetPrefs();
	}

	public static void OnWindowResize()
	{
		int pad = 10, buttonWidth = 100, buttonHeight = 20;
		resetRect = new Rect(Screen.width-pad-buttonWidth, Screen.height-pad-buttonHeight, buttonWidth, buttonHeight);
	}

	public static void ResetToDefaults()
	{
		if(EditorUtility.DisplayDialog("Delete ProBuilder editor preferences?", "Are you sure you want to delete these?, this action cannot be undone.", "Yes", "No")) {
			EditorPrefs.DeleteKey("defaultEditMode");
			EditorPrefs.DeleteKey("defaultSelectionMode");
			EditorPrefs.DeleteKey("defaultFaceColor");
			EditorPrefs.DeleteKey("defaultOpenInDockableWindow");
			EditorPrefs.DeleteKey("defaultShortcuts");
		}

		LoadPrefs();
	}

	/* Shield your eyes.  It's about to get ugly... */

	public static int shortcutIndex = 0;
	static Rect selectBox = new Rect(130, 207, 179, 185);

	static Rect resetRect = new Rect(0,0,0,0);
	static Vector2 shortcutScroll = Vector2.zero;
	static int CELL_HEIGHT = 20;
	public static void ShortcutSelectPanel()
	{
		GUILayout.Space(4);
		GUI.contentColor = Color.white;
		GUI.Box(selectBox, "");

		GUIStyle labelStyle = GUIStyle.none;

		if(EditorGUIUtility.isProSkin)
			labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

		labelStyle.alignment = TextAnchor.MiddleLeft;
		labelStyle.contentOffset = new Vector2(4f, 0f);

		shortcutScroll = EditorGUILayout.BeginScrollView(shortcutScroll, false, true, GUILayout.MaxWidth(183), GUILayout.MaxHeight(186));

		for(int n = 1; n < defaultShortcuts.Length; n++)
		{
			if(n == shortcutIndex) {
				GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
					labelStyle.normal.background = EditorGUIUtility.whiteTexture;
					Color oc = labelStyle.normal.textColor;
					labelStyle.normal.textColor = Color.white;
					GUILayout.Box(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT));
					labelStyle.normal.background = null;
					labelStyle.normal.textColor = oc;
				GUI.backgroundColor = Color.white;
	
				// if(GUILayout.Button(defaultShortcuts[n].action, EditorStyles.whiteLabel))
				// 	shortcutIndex = n;
			}
			else
			{

				if(GUILayout.Button(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT)))
					shortcutIndex = n;
			}
		}

		EditorGUILayout.EndScrollView();

	}

	static Rect keyRect = new Rect(324, 210, 168, 18);
	static Rect keyInputRect = new Rect(356, 210, 133, 18);

	static Rect descriptionTitleRect = new Rect(324, 270, 168, 200);
	static Rect descriptionRect = new Rect(324, 290, 168, 200);

	static Rect modifiersRect = new Rect(324, 240, 168, 18);
	static Rect modifiersInputRect = new Rect(383, 240, 107, 18);

	public static void ShortcutEditPanel()
	{
		// descriptionTitleRect = EditorGUI.RectField(new Rect(240,150,200,50), descriptionTitleRect);

		string keyString = defaultShortcuts[shortcutIndex].key.ToString();
	
		GUI.Label(keyRect, "Key");
		keyString = EditorGUI.TextField(keyInputRect, keyString);
		defaultShortcuts[shortcutIndex].key = pbUtil.ParseEnum(keyString, KeyCode.None);

		GUI.Label(modifiersRect, "Modifiers");
		defaultShortcuts[shortcutIndex].eventModifiers = 
			(EventModifiers)EditorGUI.EnumMaskField(modifiersInputRect, defaultShortcuts[shortcutIndex].eventModifiers);

		GUI.Label(descriptionTitleRect, "Description", EditorStyles.boldLabel);

		GUI.Label(descriptionRect, defaultShortcuts[shortcutIndex].description, EditorStyles.wordWrappedLabel);
	}

	public static void LoadPrefs()
	{
		defaultEditMode = EditorPrefs.GetString("defaultEditMode");
		_editMode = pbUtil.ParseEnum(defaultEditMode, _editMode);

		defaultSelectionMode = EditorPrefs.GetString("defaultSelectionMode");
		_selectMode = pbUtil.ParseEnum(defaultSelectionMode, _selectMode);

		defaultFaceColor = EditorPrefs.GetString("defaultFaceColor");
		_faceColor = pbUtil.ColorWithString(defaultFaceColor);

		if(!EditorPrefs.HasKey("defaultOpenInDockableWindow"))
			EditorPrefs.SetBool("defaultOpenInDockableWindow", true);

		defaultHideFaceMask = (EditorPrefs.HasKey("defaultHideFaceMask")) ? EditorPrefs.GetBool("defaultHideFaceMask") : false;
			
		defaultOpenInDockableWindow = EditorPrefs.GetBool("defaultOpenInDockableWindow", defaultOpenInDockableWindow);

		// shortcut defaults are set in pbUtil
		defaultShortcuts = EditorPrefs.HasKey("defaultShortcuts") ? 
			pbUtil.ParseShortcuts(EditorPrefs.GetString("defaultShortcuts")) : 
			ProBuilder.DefaultShortcuts();
	}

	public static void SetPrefs()
	{
		EditorPrefs.SetString("defaultEditMode", _editMode.ToString().ToLower());
		EditorPrefs.SetString("defaultSelectionMode", _selectMode.ToString().ToLower());
		EditorPrefs.SetString("defaultFaceColor", _faceColor.ToString());
		EditorPrefs.SetBool("defaultOpenInDockableWindow", defaultOpenInDockableWindow);
		EditorPrefs.SetBool("defaultHideFaceMask", defaultHideFaceMask);
		EditorPrefs.SetString("defaultShortcuts", pbUtil.ShortcutsToString(defaultShortcuts));
	}
}