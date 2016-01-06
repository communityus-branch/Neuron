using UnityEngine;
using UnityEditor;
using System.Collections;

public class ProBuilderMenuItems : EditorWindow {

	//[MenuItem("Window/ProBuilder/Open Shape Menu %#k")]
	public static void ShapeMenu()
	{
		EditorWindow.GetWindow(typeof(pb_Geometry_Interface), true, "Shape Menu", true);
	}

	//[MenuItem("Window/ProBuilder/ProBuilder Window")]
	public static pb_Editor OpenEditorWindow()
	{
		if(EditorPrefs.HasKey("defaultOpenInDockableWindow") && !EditorPrefs.GetBool("defaultOpenInDockableWindow"))
			return (pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), true, "ProBuilder", true);			// open as floating window
		else
			return (pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), false, "ProBuilder", true);			// open as dockable window
	}
}