/************************************************************************************

Filename    :   OVRCameraControllerEditor.cs
Content     :   Player controller interface. 
				This script adds editor functionality to the OVRCameraController
Created     :   March 06, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(OVRCameraController))]

//-------------------------------------------------------------------------------------
// ***** OVRCameraControllerEditor
//
// OVRCameraControllerEditor adds extra functionality in the inspector for the currently
// selected OVRCameraController.
//
public class OVRCameraControllerEditor : Editor
{
	// target component
	private OVRCameraController m_Component;

	// OnEnable
	void OnEnable()
	{
		m_Component = (OVRCameraController)target;
	}

	// OnDestroy
	void OnDestroy()
	{
	}

	// OnInspectorGUI
	public override void OnInspectorGUI()
	{
		GUI.color = Color.white;
		
		Undo.SetSnapshotTarget(m_Component, "OVRCameraController");

		{
			/*
			m_Component.NeckPosition 		= EditorGUILayout.Vector3Field("Neck Position", m_Component.NeckPosition);
			m_Component.EyeCenterPosition 	= EditorGUILayout.Vector3Field("Eye Center Position", m_Component.EyeCenterPosition);
			OVREditorGUIUtility.Separator();
			m_Component.TrackerRotatesY 	= EditorGUILayout.Toggle("Tracker Rotates Y", m_Component.TrackerRotatesY);
			OVREditorGUIUtility.Separator();
			m_Component.BackgroundColor 	= EditorGUILayout.ColorField("Background Color", m_Component.BackgroundColor);
			OVREditorGUIUtility.Separator();
			*/ 
			DrawDefaultInspector ();
		}

		if (GUI.changed)
		{
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
			EditorUtility.SetDirty(target);
		}
		
		Undo.ClearSnapshotTarget();
	}		
}

