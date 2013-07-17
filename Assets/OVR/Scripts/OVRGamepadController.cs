/************************************************************************************

Filename    :   OVRGamepadController.cs
Content     :   Interface to XBox360 controller
Created     :   January 8, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/

using UnityEngine;
#if UNITY_STANDALONE_WIN	
using XInputDotNetPure;
#else
// Different input device interface for other platforms goes here
#endif

//-------------------------------------------------------------------------------------
// ***** OVRGamepadController
//
// OVRGamepadController is an interface class to a gamepad controller.
//
// On Windows machines, the gamepad must be XInput-compliant.
//
public class OVRGamepadController : MonoBehaviour
{
// Only Windows supports XInput-compliant controllers
#if UNITY_STANDALONE_WIN	

    private static bool 		playerIndexSet = false;
    private static PlayerIndex 	playerIndex;
	
	public  static GamePadState state;
	
	private GamePadState 		testState;
	
	// * * * * * * * * * * * * *
	
 	// Start
	void Start()
    {
    }
	
	// Update
    void Update()
    {
        // Find a PlayerIndex, for a single player game
        if (!playerIndexSet)
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex pidx = (PlayerIndex)i;
                testState = GamePad.GetState(pidx);
               
				if (testState.IsConnected)
                {
                    Debug.Log(string.Format("GamePad {0} found", pidx));
                    playerIndex = pidx;
                    playerIndexSet = true;
                }
            }
        }

        state = GamePad.GetState(playerIndex);  
    }
	
	// CheckButton
	private static bool CheckButton(ButtonState bState)
	{
		if(!playerIndexSet) return false;
		if(bState == ButtonState.Pressed) return true;
		return false;
	}
	
	// * * * * * * * * * * * * *
	// Analog
	public static float GetAxisLeftX()
	{
		if(!playerIndexSet) return 0.0f;
		return state.ThumbSticks.Left.X;
	}
	public static float GetAxisLeftY()
	{
		if(!playerIndexSet) return 0.0f;
		return state.ThumbSticks.Left.Y;
	}
	public static float GetAxisRightX()
	{
		if(!playerIndexSet) return 0.0f;
		return state.ThumbSticks.Right.X;
	}
	public static float GetAxisRightY()
	{
		if(!playerIndexSet) return 0.0f;
		return state.ThumbSticks.Right.Y;
	}
	public static float GetTriggerLeft()
	{
		if(!playerIndexSet) return 0.0f;
		return state.Triggers.Left;
	}
	public static float GetTriggerRight()
	{
		if(!playerIndexSet) return 0.0f;
		return state.Triggers.Right;
	}
	// * * * * * * * * * * * * *
	// DPad
	public static bool GetDPadUp()
	{	
		return CheckButton(state.DPad.Up);
	}
	public static bool GetDPadDown()
	{	
		return CheckButton(state.DPad.Down);
	}
	public static bool GetDPadLeft()
	{	
		return CheckButton(state.DPad.Left);
	}
	public static bool GetDPadRight()
	{	
		return CheckButton(state.DPad.Right);
	}
	// * * * * * * * * * * * * *
	// Buttons
	public static bool GetButtonStart()
	{
		return CheckButton(state.Buttons.Start);
	}
	public static bool GetButtonBack()
	{
		return CheckButton(state.Buttons.Back);
	}
	public static bool GetButtonA()
	{
		return CheckButton(state.Buttons.A);
	}
	public static bool GetButtonB()
	{
		return CheckButton(state.Buttons.B);
	}
	public static bool GetButtonX()
	{
		return CheckButton(state.Buttons.X);
	}
	public static bool GetButtonY()
	{
		return CheckButton(state.Buttons.Y);
	}
	public static bool GetButtonLShoulder()
	{
		return CheckButton(state.Buttons.LeftShoulder);
	}
	public static bool GetButtonRShoulder()
	{
		return CheckButton(state.Buttons.RightShoulder);
	}
	public static bool GetButtonLStick()
	{
		return CheckButton(state.Buttons.LeftStick);
	}
	public static bool GetButtonRStick()
	{
		return CheckButton(state.Buttons.RightStick);
	}
#else
	public static float GetAxisLeftX()
	{
		return 0;
	}
	public static float GetAxisLeftY()
	{
		return 0;
	}
	public static float GetAxisRightX()
	{
		return 0;
	}
	public static float GetAxisRightY()
	{
		return 0;
	}
	public static float GetTriggerLeft()
	{
		return 0;
	}
	public static float GetTriggerRight()
	{
		return 0;
	}
	// DPad
	public static bool GetDPadUp()
	{	
		return false;
	}
	public static bool GetDPadDown()
	{	
		return false;	
	}
	public static bool GetDPadLeft()
	{	
		return false;
	}
	public static bool GetDPadRight()
	{
		return false;
	}
	// Buttons
	public static bool GetButtonStart()
	{
		return false;
	}
	public static bool GetButtonBack()
	{
		return false;
	}
	public static bool GetButtonA()
	{
		return false;
	}
	public static bool GetButtonB()
	{
		return false;
	}
	public static bool GetButtonX()
	{
		return false;
	}
	public static bool GetButtonY()
	{
		return false;
	}
	public static bool GetButtonLShoulder()
	{
		return false;
	}
	public static bool GetButtonRShoulder()
	{
		return false;
	}
	public static bool GetButtonLStick()
	{
		return false;
	}
	public static bool GetButtonRStick()
	{
		return false;
	}
#endif
}
