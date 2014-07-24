/*
 ***************************************************************************
 * version:1.0
 * 
 * 2014-7-18
 * 放入Editor下
 * lghtmap自动赋予贴图，根据名字自动找寻lightmap
 * 物体命名的规则xxx-xxx_num:t-t_0,t-t_1,t-t_2
 * 材质球命名规则xxx_num;t_0,t_1,t_2
 * lightmap命名规则xxx_num;t_0,t_1,t_2
 * 
 ***************************************************************************
*/
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Tion
{
    public class TinoLightmapTool : EditorWindow
    {
        [MenuItem("Tino/" + "Lightmap Auto Set")]

        private static void AddWindows()
        {
            Rect rect = new Rect(0, 0, 300, 200);
            var window =
                (TinoLightmapTool)GetWindowWithRect(typeof(TinoLightmapTool), rect, true, "Lightmap Set");
            window.Show();
        }


        private void OnGUI()
        {

            if (GUILayout.Button("lightmap选择好后点这里", GUILayout.Width(200)))
            {
                SelLMToLMArray();
            }

            if (GUILayout.Button("lightmap已加入后点这里", GUILayout.Width(200)))
            {
                SelOBJToLM();
            }
        }

        //Refresh
        private void OnInspectorUpdate()
        {
            Repaint();
        }

        //Selection Lightmap to Unity Lightmaping Array
        private void SelLMToLMArray()
        {
            int count = Selection.objects.Length;
            LightmapData[] LMData = new LightmapData[count];

            Object[] LMTexture = Selection.objects;

            for (int i = 0; i < count; ++i)
            {
                LightmapData lightmapFar = new LightmapData();
                lightmapFar.lightmapFar = LMTexture[i] as Texture2D;
                LMData[int.Parse((LMTexture[i].name.Split('_'))[1])] = lightmapFar;
            }

            LightmapSettings.lightmaps = LMData;
        }

        //Selection static gameobject to attch lightmap
        private void SelOBJToLM()
        { 

            foreach (GameObject obj in Selection.gameObjects)
            {
                int mark = int.Parse(((obj.name.Split('-'))[1].Split('_'))[1]);
                obj.renderer.lightmapIndex = mark;
            }
        }
    }
}

