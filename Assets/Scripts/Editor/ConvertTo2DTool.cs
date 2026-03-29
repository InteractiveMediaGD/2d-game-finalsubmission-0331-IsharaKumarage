using UnityEngine;
using UnityEditor;

public class ConvertTo2DTool : Editor
{
    [MenuItem("Tools/Convert To 2D")]
    public static void ConvertNow()
    {
        // Change Editor Behavior to 2D
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        
        // Set active Scene View to 2D Mode
        if (SceneView.lastActiveSceneView != null) 
        {
            SceneView.lastActiveSceneView.in2DMode = true;
        }

        // Set Camera(s) to Orthographic Projection
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
        }
        else
        {
            Camera[] cams = FindObjectsOfType<Camera>();
            foreach (Camera cam in cams)
            {
                cam.orthographic = true;
            }
        }
        
        Debug.Log("Successfully Converted Project and Scene to 2D Mode!");
    }
}
