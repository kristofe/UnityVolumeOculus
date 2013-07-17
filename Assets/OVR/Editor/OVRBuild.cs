/************************************************************************************

Filename    :   OVRBuild.cs
Content     :   Rift editor interface. 
				This script adds the ability to build demo from within Unity and from
				command line
Created     :   February 14, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEngine;
using UnityEditor;

//-------------------------------------------------------------------------------------
// ***** OculusBuild
//
// OculusBuild adds menu functionality for a user to build the currently selected scene, 
// and to also build and run the standalone build. These menu items can be found under the
// Oculus/Build menu from the main Unity Editor menu bar.
//

class OculusBuild
{
	// Build the standalone Windows demo and place into main project folder
	[MenuItem ("Oculus/Build/StandaloneWindows")]	
	static void PerformBuildStandaloneWindows ()
	{
		if(Application.isEditor)
		{
			string[] scenes = { EditorApplication.currentScene };
			BuildPipeline.BuildPlayer(scenes, "OculusUnityDemoScene.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
		}
	}
	
	// Build the standalone Windows demo and place into main project folder, and then run
	[MenuItem ("Oculus/Build/StandaloneWindows - Run")]	
	static void PerformBuildStandaloneWindowsRun ()
	{
		if(Application.isEditor)
		{
			string[] scenes = { EditorApplication.currentScene };
			BuildPipeline.BuildPlayer(scenes, "OculusUnityDemoScene.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
		}
		else
		{
			string[] scenes = { "Assets/Tuscany/Scenes/VRDemo_Tuscany.unity" };
			BuildPipeline.BuildPlayer(scenes, "OculusUnityDemoScene.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
		}
    }
}

//-------------------------------------------------------------------------------------
// ***** OculusBuildDemo
//
// OculusBuild allows for command line building of the Oculus main demo (Tuscany).
//
class OculusBuildDemo
{
	static void PerformBuildStandaloneWindows ()
	{
		string[] scenes = { "Assets/Tuscany/Scenes/VRDemo_Tuscany.unity" };
		BuildPipeline.BuildPlayer(scenes, "OculusUnityDemoScene.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }
	
	static void PerformBuildStandaloneWindowsRun ()
	{
		string[] scenes = { "Assets/Tuscany/Scenes/VRDemo_Tuscany.unity" };
		BuildPipeline.BuildPlayer(scenes, "OculusUnityDemoScene.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
    }
}