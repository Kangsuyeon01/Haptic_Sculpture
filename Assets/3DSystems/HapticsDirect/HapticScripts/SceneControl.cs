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
using TMPro;

public class SceneControl : MonoBehaviour
{
    // Start is called before the first frame update
    public HapticPlugin HPlugin = null;
    private int ActiveStage = 0;
    private float dbAS;
    public GameObject DeviceInfo;
    public GameObject Device1;
    public GameObject Device2;
    public GameObject[] StageBorders;
    



    private void UpdateKeys()
    {
        if (HPlugin != null)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (!HPlugin.isNavTranslation())
                {
                    HPlugin.EnableNavTranslation();
                    HPlugin.DisableNavRotation();

                }
                else
                {
                    HPlugin.DisableNavTranslation();
                }

            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (!HPlugin.isNavRotation())
                {
                    HPlugin.EnableNavRotation();
                    HPlugin.DisableNavTranslation();
                }
                else
                {
                    HPlugin.DisableNavRotation();
                }

            }

            if (Input.GetKeyDown(KeyCode.N))
            {

                HPlugin.EnableVibration();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {

                HPlugin.DisableVibration();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {

                CenterStage();

            }

            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }

        }

    }

    public int GetActiveStage()
    {
        float StagePos;
        int result = 0;

        for (int i=0; i<StageBorders.Length; i++)
        {
            StagePos = i * -0.7f;
            if(StagePos-0.1f < HPlugin.transform.position.x && StagePos + 0.1f > HPlugin.transform.position.x)
            {
                result = i;
                ActiveStage = i;
            }
            else
            {
                result = ActiveStage;
            }
        }

               
        return result;
    }

    public void CenterStage()
    {
        HPlugin.transform.SetPositionAndRotation(new Vector3(-0.7f * GetActiveStage(), 0.0f, 0.0f), HPlugin.transform.rotation);
    }

    public void UpdateStageBorder()
    {
        int aStage = GetActiveStage();
        
        
                
                Material mat1 = Resources.Load<Material>("Models/Materials/MaterialStage1");
                StageBorders[aStage].gameObject.GetComponent<MeshRenderer>().material = mat1;

                if (aStage > 0)
                {
                    Material mat2 = Resources.Load<Material>("Models/Materials/MaterialStage2");
                    StageBorders[aStage - 1].gameObject.GetComponent<MeshRenderer>().material = mat2;
                }
                if(aStage < StageBorders.Length-1)
                {
                    Material mat2 = Resources.Load<Material>("Models/Materials/MaterialStage2");
                    StageBorders[aStage + 1].gameObject.GetComponent<MeshRenderer>().material = mat2;
                }
          
            
                
            
       
    }

    private void Start()
    {

        TextMeshPro TMesh = DeviceInfo.GetComponent<TextMeshPro>();
         
        TMesh.text = HPlugin.DeviceIdentifier + "\n" + HPlugin.ModelType + "\n" + HPlugin.SerialNumber + "\n" + HPlugin.MaxForce.ToString("F") + " N";
        if (HPlugin.ModelType == "Touch")
        {
            Device1.SetActive(true);
            Device1.GetComponent<VirtualHaptic>().ShowGizmo = true;
            Device1.GetComponent<VirtualHaptic>().ShowLabels = true;
            Device2.SetActive(false);
            Device2.GetComponent<VirtualHaptic>().ShowGizmo = false;
            Device2.GetComponent<VirtualHaptic>().ShowLabels = false;
            

        }
        if (HPlugin.ModelType == "Touch X")
        {
            Device1.SetActive(false);
            Device1.GetComponent<VirtualHaptic>().ShowGizmo = false;
            Device1.GetComponent<VirtualHaptic>().ShowLabels = false;
            
            Device2.SetActive(true);
            Device2.GetComponent<VirtualHaptic>().ShowGizmo = true;
            Device2.GetComponent<VirtualHaptic>().ShowLabels = true;
        }


    }

    

    // Update is called once per frame
    private void Update()
    {
       
        UpdateKeys();
        UpdateStageBorder();
        
               








    }

    private void LateUpdate()
    {

        
    }
}
