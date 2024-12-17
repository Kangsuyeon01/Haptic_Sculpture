// This code contains 3D SYSTEMS Confidential Information and is disclosed to you
// under a form of 3D SYSTEMS software license agreement provided separately to you.
//
// Notice
// 3D SYSTEMS and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from 3D SYSTEMS is strictly prohibited.
//
// ALL 3D SYSTEMS DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". 3D SYSTEMS MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, 3D SYSTEMS assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of 3D SYSTEMS. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// 3D SYSTEMS products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// 3D SYSTEMS.
//
// Copyright (c) 2021 3D SYSTEMS. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class HapticSwipeEvents
{

    public UnityEvent OnSwipeLeft;

    public UnityEvent OnSwipeRight;

    
}

public class HapticSwipe : MonoBehaviour
{
    public HapticPlugin HPlugin = null;

    private bool swipeLeft = false;
    private bool swipeRight = false;
    public float VelocityActivation = 600.0f;

    public float VelControl;
    public GameObject Stylus1;
    public GameObject Stylus2;

    public HapticSwipeEvents GestureEvents;

    void Start()
    {

    }


    void Update()
    {
        if (HPlugin != null)
        {
            VelControl = HPlugin.CurrentVelocity.x;
            if (swipeLeft==false && HPlugin.CurrentVelocity.x > VelocityActivation)
            {
                GestureEvents.OnSwipeLeft.Invoke();
                swipeLeft = true;
            }

            if (swipeRight == false && HPlugin.CurrentVelocity.x < VelocityActivation*-1.0)
            {
                GestureEvents.OnSwipeRight.Invoke();
                swipeRight = true;
            }

            if (HPlugin.CurrentVelocity.x < 10 && HPlugin.CurrentVelocity.x > -10)
            {
                swipeLeft = false;
                swipeRight = false;
            }
        }

    }

    public void ChangeToNewStylus()
    {
        HPlugin.VisualizationMesh = Stylus2;
    }

    public void ChangeToOldStylus()
    {
        HPlugin.VisualizationMesh = Stylus1;
    }

    public void MoveRight()
    {
        HPlugin.transform.Translate(-0.7f, 0.0f, 0.0f);
    }

    public void MoveLeft()
    {
        HPlugin.transform.Translate(0.7f, 0.0f, 0.0f);
    }
}

