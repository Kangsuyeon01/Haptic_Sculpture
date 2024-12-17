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
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VirtualHaptic : MonoBehaviour
{
    
   public HapticPlugin HPlugin = null;
  

   public GameObject joint0;
   public GameObject joint1;
   public GameObject joint2;
   public GameObject joint3;
   public GameObject joint4;
   public GameObject joint5;

   public bool ShowGizmo = true;
   public bool ShowLabels = true;

    
        

    
    private void Update()
    {

        if (HPlugin == null)
            return;
                 

        joint0.transform.localRotation = Quaternion.Euler(0.0f, HPlugin.JointAngles[0],0.0f);
        joint1.transform.localRotation = Quaternion.Euler(HPlugin.JointAngles[1]*-1f,0.0f,0.0f);
        joint2.transform.localRotation = Quaternion.Euler((HPlugin.JointAngles[2] - HPlugin.JointAngles[1]) *-1f, 0.0f, 0.0f);
        joint3.transform.localRotation = Quaternion.Euler(0.0f, HPlugin.GimbalAngles[0]*-1.0f, 0.0f);
        joint4.transform.localRotation = Quaternion.Euler(HPlugin.GimbalAngles[1]*-1.0f, 0.0f, 0.0f);
        joint5.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, HPlugin.GimbalAngles[2]);
        

    }

#if UNITY_EDITOR

    float CalcAngle(float startPos, float endPos)
    {
        float result = 0;

        if(startPos < 0 && endPos < 0)
        {
            result = Mathf.Abs(endPos - startPos);
        }else if(startPos < 0 && endPos > 0)
        {
            result = endPos + Mathf.Abs(startPos);
        }
        else
        {
            result = Mathf.Abs(endPos - startPos);
        }

        return result;
    }
    
    void DrawNavSpeedArea(GameObject joint, Vector3 jnormal ,Vector3 rotAxis, Vector3 directionVec, Vector2 zone0, Vector2 zone1, Vector2 zone2, bool isBlocked)
    {
        Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);

        
        Vector3 jointDZ;

        if (isBlocked==false)
        {

            jointDZ = Quaternion.AngleAxis(zone0.x, rotAxis) * directionVec;



            Handles.DrawSolidArc(joint.transform.position,
                    joint.transform.worldToLocalMatrix * jnormal,
                    jointDZ,
                    CalcAngle(zone0.x, zone0.y),
                    0.015f);

            Handles.color = new Color(0.0f, 1.0f, 0.0f, 0.2f);
            jointDZ = Quaternion.AngleAxis(zone1.x, rotAxis) * directionVec;


            Handles.DrawSolidArc(joint.transform.position,
                    joint.transform.worldToLocalMatrix * jnormal,
                    jointDZ,
                    CalcAngle(zone1.x, zone1.y),
                    0.015f);

            jointDZ = Quaternion.AngleAxis(zone2.x, rotAxis) * directionVec;


            Handles.DrawSolidArc(joint.transform.position,
                    joint.transform.worldToLocalMatrix * jnormal,
                    jointDZ,
                    CalcAngle(zone2.x, zone2.y),
                    0.015f);

        }
        else
        {
            jointDZ =  directionVec;



            Handles.DrawSolidArc(joint.transform.position,
                    joint.transform.worldToLocalMatrix * jnormal,
                    jointDZ,
                    360.0f,
                    0.015f);
        }


        
    }

    void OnDrawGizmos()
    {
                
        if (ShowGizmo)
        {
            DrawNavSpeedArea(joint0, joint0.transform.up, Vector3.up, Vector3.forward, HPlugin.SliderTXZ0, HPlugin.SliderTXZ1n, HPlugin.SliderTXZ1p, HPlugin.FreezeTranslation.HasFlag(HapticPlugin.Axis.X));
            DrawNavSpeedArea(joint1, joint1.transform.right, Vector3.right, Vector3.up, HPlugin.SliderTYZ0, HPlugin.SliderTYZ1n, HPlugin.SliderTYZ1p, HPlugin.FreezeTranslation.HasFlag(HapticPlugin.Axis.Y));
            DrawNavSpeedArea(joint2, joint2.transform.right, Vector3.right, Vector3.up, HPlugin.SliderTZZ0, HPlugin.SliderTZZ1n, HPlugin.SliderTZZ1p, HPlugin.FreezeTranslation.HasFlag(HapticPlugin.Axis.Z));
            DrawNavSpeedArea(joint3, joint3.transform.up, Vector3.up, Vector3.forward, HPlugin.SliderRXZ0, HPlugin.SliderRXZ1n, HPlugin.SliderRXZ1p, HPlugin.FreezeRotation.HasFlag(HapticPlugin.Axis.X));
            DrawNavSpeedArea(joint4, joint4.transform.right, Vector3.right, Vector3.up, HPlugin.SliderRYZ0, HPlugin.SliderRYZ1n, HPlugin.SliderRYZ1p, HPlugin.FreezeRotation.HasFlag(HapticPlugin.Axis.Y));
            DrawNavSpeedArea(joint5, joint5.transform.forward, Vector3.forward, Vector3.up, HPlugin.SliderRZZ0, HPlugin.SliderRZZ1n, HPlugin.SliderRZZ1p, HPlugin.FreezeRotation.HasFlag(HapticPlugin.Axis.Z));

            Gizmos.color = Color.white;
            Vector3 direction;

            direction = Quaternion.AngleAxis(HPlugin.JointAngles[0], Vector3.up) * Vector3.forward - joint0.transform.position;
            Gizmos.DrawLine(joint0.transform.position, joint0.transform.position + (direction.normalized * 0.06f));

            direction = Quaternion.AngleAxis(HPlugin.JointAngles[1] * -1.0f, Vector3.right) * Vector3.forward + joint1.transform.position;
            Gizmos.DrawLine(joint1.transform.position, joint1.transform.position + (direction.normalized * 0.06f));

            direction = Quaternion.AngleAxis((HPlugin.JointAngles[2]- HPlugin.JointAngles[1]) * -1.0f, Vector3.right) * Vector3.forward - joint2.transform.position;
            Gizmos.DrawLine(joint2.transform.position, joint2.transform.position + (direction.normalized * 0.02f));

            direction = Quaternion.AngleAxis(HPlugin.GimbalAngles[0] * -1.0f, Vector3.up) * Vector3.forward - joint3.transform.position;
            Gizmos.DrawLine(joint3.transform.position, joint3.transform.position + (direction.normalized * 0.02f));

            direction = Quaternion.AngleAxis(HPlugin.GimbalAngles[1] * -1.0f, Vector3.right) * Vector3.up - joint4.transform.position;
            Gizmos.DrawLine(joint4.transform.position, joint4.transform.position + (direction.normalized * 0.02f));

            direction = Quaternion.AngleAxis(HPlugin.GimbalAngles[2], Vector3.forward) * Vector3.up - joint5.transform.position;
            Gizmos.DrawLine(joint5.transform.position, joint5.transform.position + (direction.normalized * 0.02f));
        }

        if (ShowLabels)
        {

            Handles.Label(joint0.transform.position, "JX Angle: " + HPlugin.JointAngles[0]);
            Handles.Label(joint1.transform.position, "JY Angle: " + HPlugin.JointAngles[1]);
            Handles.Label(joint2.transform.position, "JZ Angle: " + (HPlugin.JointAngles[2]-HPlugin.JointAngles[1]));
            Handles.Label(joint3.transform.position, "GX Angle: " + HPlugin.GimbalAngles[0]);
            Handles.Label(joint4.transform.position, "GY Angle: " + HPlugin.GimbalAngles[1]);
            Handles.Label(joint5.transform.position, "GZ Angle: " + HPlugin.GimbalAngles[2]);
            
        }

    }
#endif

}



