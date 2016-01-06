import System.Reflection;

class ProGrids_GUI extends ProGrids_Base 
{
    @MenuItem("Window/6by7/ProGrids (v1.5.1, Free)")
    static function Initialize()
	{
        var window = GetWindow(ProGrids_GUI, false, "Grid");
        window.Show();
    }

    function OnEnable()
    {
    	InitProGrids();
    	OnSelectionChange();
    }

    function OnDisable()
    {
    	pb_ToggleSnapToGrid(false);
    }

    function InitProGrids()
    {
		if(GameObject.Find("_grid")) //if a scene manager is present, connect to it and Rebuild()
		{
			proGrids = GameObject.Find("_grid").GetComponent(ProGrids);
			SyncToLocalGrid();
		}
		else //if no scene manager was fount, create a new scene manager
		{
			var proGridsObject = Instantiate((AssetDatabase.LoadAssetAtPath("Assets/6by7/ProGrids/_grid.prefab", typeof(GameObject))), Vector3(-1,0,0), Quaternion.identity);
			proGridsObject.name = "_grid";
			proGrids = proGridsObject.GetComponent(ProGrids);
			
			//set hide flags
			//var baseHideFlags = HideFlags.NotEditable;
			//proGridsObject.hideFlags = baseHideFlags;
			
			//sync up
			SyncToLocalGrid();
		}

		pb_ToggleSnapToGrid(proGrids.snapToGrid);

		toggleSnapGraphic = snapOnGraphic;
		toggleVisGraphic = visOnGraphic;
		toggleAnglesGraphic = anglesOnGraphic;
		
		snapSizeGraphic = (AssetDatabase.LoadAssetAtPath("Assets/6by7/Shared/GUI/icon_GridSize.tga", typeof(Object)));
	}
	
	function OnGUI()
	{
		//pull in correct skin for Unity version...must be a better way, but this is it for now!	
		#if UNITY_3_5
			mySkin = (Resources.LoadAssetAtPath("Assets/6by7/Shared/GUI/CustomSkin_Unity3.guiskin", typeof(Object)));
		#endif
		//
		
		var window = this;
		window.minSize = Vector2(46,205);
		window.maxSize = Vector2(46,205);
		
		if(!proGrids)
		{
			InitProGrids();
		}
		
		if(proGrids)
		{
			var stg : boolean = proGrids.snapToGrid;
			var sgl : boolean = proGrids.showGrid;
			
			GUI.skin = mySkin;
			EditorGUI.BeginChangeCheck();
				
			EditorGUILayout.BeginVertical();	
			GUILayout.Space(4);
			
			//snap on/off toggle
			if(GUILayout.Button(GUIContent(toggleSnapGraphic, "Toggle Snapping On/Off")))
			{
				proGrids.snapToGrid = !proGrids.snapToGrid;
				lastActiveTransform = null;

				if(proGrids.snapToGrid)
					toggleSnapGraphic = snapOnGraphic;
				else
					toggleSnapGraphic = snapOffGraphic;
				
				SceneView.RepaintAll();
			}

			//show grids on/off toggle
			if(GUILayout.Button(GUIContent(toggleVisGraphic, "Toggle Grid Visibility On/Off")))
			{
				proGrids.showGrid = !proGrids.showGrid;
				if(proGrids.showGrid)
					toggleVisGraphic = visOnGraphic;
				else
					toggleVisGraphic = visOffGraphic;
				
				SceneView.RepaintAll();
			}
			
//snap selected button
            //if(GUILayout.Button(GUIContent(snapSelectedGraphic, "Snap All Selected items to Nearest Grid Point")))
            //{
            //    EditorUtility.DisplayDialog("Snap Selected", "You can instantly snap all selected objects to the grid. Sorry, not available in the Free Version.", "Okay");
            //}            
            
            //snap size
            if(GUILayout.Button(GUIContent(snapSizeGraphic as UnityEngine.Texture, "Change the Grid Size and Units")))
            {
                EditorUtility.DisplayDialog("Grid Size and Units", "Grid size and units are adjustable at any time! Meters, Centimeters, Feet, and Inches are available. Sorry, not available in the Free Version.", "Okay");
            }

            //show angles on/off toggle
            if(GUILayout.Button(GUIContent(anglesOffGraphic as UnityEngine.Texture, "Toggle Angles Visibility On/Off")))
            {
                EditorUtility.DisplayDialog("Angle Guidelines", "You can view handy Angle Guidelines for precise construction. Sorry, not available in the Free Version.", "Okay");
            }
			
			EditorGUILayout.EndVertical();

			//check for changes and apply as needed
			if(EditorGUI.EndChangeCheck ()) 
			{	
				if(Selection.transforms.Length > 0)
					FindNearestSnapPos(Selection.transforms[0].position);
				else
					FindNearestSnapPos(Vector3(0,0,0));

				proGrids.gridCenterPos = nearestSnapPos;
				
				pb_ToggleSnapToGrid(proGrids.snapToGrid);

				SceneView.RepaintAll();
			}
			
			//check if snap has been turned on/off
			if(stg != proGrids.snapToGrid)
			{
				SetupSelectionForSnap();

				pb_ToggleSnapToGrid(proGrids.snapToGrid);
			}
		}
	}

	function OnSelectionChange()
	{
		pb_ToggleSnapToGrid(proGrids.snapToGrid);
	}
}