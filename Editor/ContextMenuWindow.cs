using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ContextMenuWindow : EditorWindow
    {
        private const string TAB = "    ";

        private class NestedString
        {
            public string Value;
            public string Name;
            public int Depth;
            public NestedString Parent;
            public List<NestedString> Children = new List<NestedString>();
            public NestedString(string value)
            {
                Value = value;
                Name = value;
            }
        }

        [SerializeField] private Vector2Int ButtonSize = new Vector2Int(200, 20);
        [SerializeField] private int Indent = 15;

        private NestedString Data = new NestedString("Assets/Create");
        private Vector2 scroll;

        [MenuItem("Window/Create Context Menu Window")]
        public static void OpenWindow()
        {
            var window = GetWindow<ContextMenuWindow>();
            if (!window) CreateInstance<ContextMenuWindow>();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Create Context Menu");

            string stringData = EditorGUIUtility.SerializeMainMenuToString();


            //Debug.Log(stringData);

            var index = stringData.IndexOf("Create") + $"Create\n".Length;
            var last = $"\n{TAB}Delete\n";
            var index2 = stringData.IndexOf(last);
            var length2 = index2 - index;

            //Debug.Log($"index {index} | index2 {index2} | total length = {stringData.Length} | length {length2} | last {last}");

            stringData = stringData.Substring(index, length2);

            //Debug.Log(stringData);

            Data.Depth = 1;
            Data.Children.Clear();
            NestedString lastParent = Data;
            NestedString previous = Data;
            string[] lines = stringData.Split('\n');
            var length = lines.Length;

            for (int i = 0; i < length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim())) continue;

                int newDepth = 0;

                NestedString current = new NestedString(lines[i]);

                while (current.Value.Contains(TAB))
                {
                    current.Value = current.Value.Remove(0, 4);
                    newDepth++;
                }

                current.Name = current.Value;
                current.Depth = newDepth;

                // Debug.Log($"current '{current.Name}'");
                // Debug.Log($"new {newDepth} | last {lastDepth} | previous {previous.Depth}");


                if (newDepth == previous.Depth)
                {
                    current.Value = lastParent.Value + '/' + current.Value;
                    current.Parent = lastParent;
                    lastParent.Children.Add(current);

                    //Debug.Log($"add child 1 '{lastParent.Name}' + '{current.Name}'");
                }

                if (newDepth != previous.Depth)
                {
                    if (newDepth < previous.Depth)
                    {
                        NestedString temp = lastParent;

                        while (current.Depth - temp.Depth != 1)
                        {
                            temp = temp.Parent;

                            //Debug.Log($"temp '{temp.Name}' | temp {temp.Depth} | current {current.Depth}");
                        }

                        lastParent = temp;

                        //Debug.Log($"new last parent 2 '{temp.Name}'");
                    }
                    else
                    {
                        current.Value = previous.Value + '/' + current.Value;
                        current.Parent = previous;
                        previous.Children.Add(current);
                        lastParent = previous;

                        //Debug.Log($"add child 2 '{previous.Name}' + '{current.Name}'");
                    }
                }

                //Debug.Log($"after new {newDepth} | last {lastDepth}");

                previous = current;
            }
        }

        void OnGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll);
            int index = 0;
            Draw(Data,ref index);
            GUILayout.EndScrollView();
            DrawLine();
            Indent = EditorGUILayout.IntField("Indent", Indent);
            ButtonSize = EditorGUILayout.Vector2IntField("Button Height", ButtonSize);
        }

        private void Draw(NestedString target,ref int index)
        {
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
            GUILayout.Space(target.Depth * Indent);
   
            if (target.Children.Count > 0)
            {
                GUILayout.Label(target.Name);
                DrawLine();
            }
            else
            {
                if (GUILayout.Button(target.Name, GUILayout.Height(ButtonSize.y), GUILayout.Width(ButtonSize.x)))
                {
                    Selection.activeObject = GetSelectedPathOrFallback();
                    EditorApplication.ExecuteMenuItem(target.Value);
                }

                GUILayout.Label(target.Value);
            }

            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            index++;
            foreach (NestedString child in target.Children)
            {
                Draw(child,ref index);
            }
        }

        public static Object GetSelectedPathOrFallback()
        {
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object[] args = new object[0];
            var path = (string)_tryGetActiveFolderPath.Invoke(null, args);

            return AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        private void DrawLine()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}
