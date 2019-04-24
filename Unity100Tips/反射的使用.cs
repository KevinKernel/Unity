using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Test1
{
    class TestDebug : MonoBehaviour
    {
        public GameObject go = null;

        private void Start()
        {
            Debuging d = new Debuging();
            d.Debug("<Color=#00ff00>来来来....</Color> 不错 不错 。。。");

            Instantiate(go);

            StartCoroutine(InitConsole());
        }

        IEnumerator InitConsole()
        {

            Type console = null;
            if (Application.isEditor)
            {
                Debug.Log("Editor");
                string path = Application.dataPath.Replace("Assets", "Library/ScriptAssemblies/Assembly-CSharp.dll");
                console = Assembly.LoadFrom(path).GetType("SRDebugger.UI.Tabs.ConsoleTabController");
            }
            else
            {
                Debug.Log("Runtime");
                console = Assembly.Load("Assembly-CSharp.dll").GetType("SRDebugger.UI.Tabs.ConsoleTabController");
            }

            var o = FindObjectOfType(console);
            while (true)
            {
                o = FindObjectOfType(console);
                if (o != null)
                {
                    break;
                }
                yield return new WaitForSeconds(1);
            }
                
            
            
            Toggle t = console.GetField("FilterToggle").GetValue(o) as Toggle;
            t.isOn = true;
            InputField i = console.GetField("FilterField").GetValue(o) as InputField;
            i.text = "[system]";



        }
    }
}