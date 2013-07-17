﻿/************************************************************************************

Filename    :   OVRPlayerControllerEditor.cs
Content     :   Player controller interface. 
				This script adds editor functionality to the OVRPlayerController
Created     :   January 17, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(OVRPlayerController))]

//-------------------------------------------------------------------------------------
// ***** OVRPlayerControllerEditor
//
// OVRPlayerControllerEditor adds extra functionality in the inspector for the currently
// selected OVRPlayerController.
//
public class OVRPlayerControllerEditor : Editor
{
	// target component
	private OVRPlayerController m_Component;

	// foldouts
	private static bool m_MotorFoldout;
	private static bool m_PhysicsFoldout;

	// OnEnable
	void OnEnable()
	{
		m_Component = (OVRPlayerController)target;
	}

	// OnDestroy
	void OnDestroy()
	{
	}

	// OnInspectorGUI
	public override void OnInspectorGUI()
	{
		GUI.color = Color.white;

		{
			m_MotorFoldout = EditorGUILayout.Foldout(m_MotorFoldout, "Motor");

			if (m_MotorFoldout)
			{
				m_Component.Acceleration 	= EditorGUILayout.Slider("Acceleration", 	m_Component.Acceleration, 	0, 1);
				m_Component.Damping 		= EditorGUILayout.Slider("Damping", 		m_Component.Damping, 		0, 1);
				m_Component.JumpForce 		= EditorGUILayout.Slider("Jump Force", 		m_Component.JumpForce, 		0, 10);
				m_Component.RotationAmount 	= EditorGUILayout.Slider("Rotation Amount", m_Component.RotationAmount, 0, 5);
				
				OVREditorGUIUtility.Separator();
			}

			m_PhysicsFoldout = EditorGUILayout.Foldout(m_PhysicsFoldout, "Physics");
			
			if (m_PhysicsFoldout)
			{
				m_Component.GravityModifier = EditorGUILayout.Slider("Gravity Modifier", m_Component.GravityModifier, 0, 1);

				OVREditorGUIUtility.Separator();
			}
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}		
}

