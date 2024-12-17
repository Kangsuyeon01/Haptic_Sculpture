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
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class HapticEditor
{
    const string MENU_ROOT = "GameObject/Haptics Direct/";
    const string MENU_ROOT2 = "Component/Haptics Direct/";

    [MenuItem(MENU_ROOT + "Haptic Actor/Default Device", false, 100)]
    static void AddHapticActorComponent_Default()
    {

        GameObject GObj = null;
        GObj = GameObject.Find("HapticActor_DefaultDevice");
        if (GObj == null)
        {
          GObj= new GameObject("HapticActor_DefaultDevice");
          GObj.AddComponent<HapticPlugin>();
          HapticPlugin hp = GObj.GetComponent<HapticPlugin>();
          hp.DeviceIdentifier = "Default Device";
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "A GameObject with the device name already exists.", "Ok", "");
        }
        
    }

    [MenuItem(MENU_ROOT + "Haptic Actor/Left Device", false, 101)]
    static void AddHapticActorComponent_Left()
    {
        GameObject GObj = null;
        GObj = GameObject.Find("HapticActor_LeftDevice");
        if (GObj == null)
        {
            GObj = new GameObject("HapticActor_LeftDevice");
            GObj.AddComponent<HapticPlugin>();
            HapticPlugin hp = GObj.GetComponent<HapticPlugin>();
            hp.DeviceIdentifier = "Left Device";
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "A GameObject with the device name already exists.", "Ok", "");
        }


    }

    [MenuItem(MENU_ROOT + "Haptic Actor/Right Device", false, 102)]
    static void AddHapticActorComponent_Right()
    {
        GameObject GObj = null;
        GObj = GameObject.Find("HapticActor_RightDevice");
        if (GObj == null)
        {
            GObj = new GameObject("HapticActor_RightDevice");
            GObj.AddComponent<HapticPlugin>();
            HapticPlugin hp = GObj.GetComponent<HapticPlugin>();
            hp.DeviceIdentifier = "Right Device";
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "A GameObject with the device name already exists.", "Ok", "");
        }


    }

    

    [MenuItem(MENU_ROOT2 + "Haptic Collider", false, 105)]
    static void AddHapticColliderComponent()
    {
        bool bTest_Rigidbody = false;
        bool bTest_HMaterial = false;
        

        foreach (GameObject GObj in Selection.gameObjects)
        {
            if(GObj.GetComponent<HapticMaterial>() != null)
            {
                bTest_HMaterial = EditorUtility.DisplayDialog("Warning", "GameObject has a Haptic Material component. Do you want replace it?", "Yes", "No");
                if (bTest_HMaterial)
                {
                    Object.DestroyImmediate(GObj.GetComponent<HapticMaterial>());
                    bTest_HMaterial = false;
                }
            }

            if (GObj.GetComponent<HapticCollider>() == null && GObj.GetComponent<HapticMaterial>() == null)
            {
                Collider test_collider = GObj.GetComponent<Collider>();
                if (test_collider == null)
                {
                    EditorUtility.DisplayDialog("Warning", "GameObject has no collider component. A collider component is required.", "Ok", "");
                }

                Rigidbody test_rigidbody = GObj.GetComponent<Rigidbody>();
                if (test_rigidbody == null)
                {
                    bTest_Rigidbody = EditorUtility.DisplayDialog("Warning", "GameObject has no rigidbody component. Do you want add a rigidbody component?", "Yes", "No");
                    if (bTest_Rigidbody)
                    {
                        GObj.AddComponent<Rigidbody>();
                        GObj.GetComponent<Rigidbody>().useGravity = false;
                        bTest_Rigidbody = false;
                    }
                }
                else
                {
                    GObj.GetComponent<Rigidbody>().useGravity = false;
                }

                GObj.AddComponent<HapticCollider>();
            }
        }
    }


    [MenuItem(MENU_ROOT2 + "Haptic Material", false, 111)]
    static void AddHapticMaterialComponent()
    {
        foreach (GameObject GObj in Selection.gameObjects)
        {
            if(GObj.GetComponent<HapticMaterial>() == null)
            {
               GObj.AddComponent<HapticMaterial>();
            }
            
        }
    }

    [MenuItem(MENU_ROOT2 + "Haptic Material - Static Setup", false, 112)]
    static void AddHapticMaterialStatic()
    {
        foreach (GameObject GObj in Selection.gameObjects)
        {
            if (GObj.GetComponent<Collider>() == null)
            {
                GObj.AddComponent<MeshCollider>();
            }
            if (GObj.GetComponent<Rigidbody>() == null)
            {
                GObj.AddComponent<Rigidbody>();
            }

            GObj.GetComponent<Rigidbody>().isKinematic = true;

            if (GObj.GetComponent<HapticMaterial>() == null)
            {
                GObj.AddComponent<HapticMaterial>();
            }
        }
    }

    [MenuItem(MENU_ROOT2 + "Haptic Material - Dynamic Setup", false, 113)]
    static void AddHapticMaterialDynamic()
    {
        foreach (GameObject GObj in Selection.gameObjects)
        {
            if (GObj.GetComponent<Collider>() == null)
            {
                GObj.AddComponent<MeshCollider>();
                GObj.GetComponent<MeshCollider>().convex = true;
            }
            if (GObj.GetComponent<Rigidbody>() == null)
            {
                GObj.AddComponent<Rigidbody>();
            }

            GObj.GetComponent<Rigidbody>().isKinematic = false;

            if (GObj.GetComponent<HapticMaterial>() == null)
            {
                GObj.AddComponent<HapticMaterial>();
            }
        }
    }



}
#endif