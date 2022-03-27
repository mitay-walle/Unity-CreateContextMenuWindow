using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ContextMenuWindow : EditorWindow
    {
        private const bool LOGS = true;
        private const string TYPE_NAME = nameof(ContextMenuWindow);
        private const string TAB = "    ";

        [Serializable]
        private class NestedString
        {
            public string Value;
            public string Name;
            public string ValueLower;
            public bool IsExpanded;
            public int Depth;
            [NonSerialized] public NestedString Parent;
            public List<NestedString> Children = new List<NestedString>();
            public NestedString(string value)
            {
                Value = value;
                Name = value;
            }
        }

        [SerializeField] private string _filter;
        private string _filterLower;
        [SerializeField] private Vector2Int ButtonSize = new Vector2Int(200, 20);
        [SerializeField] private int Indent = 15;
        [SerializeField] private bool ShowLines;
        [SerializeField] private bool ShowLabels;
        [SerializeField] private bool UseExpandable;

        [SerializeField] private NestedString Data = new NestedString("Assets/Create") {IsExpanded = true};
        private Vector2 scroll;

        [MenuItem("Window/Create Context Menu Window")]
        public static void OpenWindow()
        {
            var window = GetWindow<ContextMenuWindow>();
            if (!window) CreateInstance<ContextMenuWindow>();
        }

        void OnEnable()
        {
            Debug.Log("OnEnable");
            LoadSettings();

            titleContent = new GUIContent("Create Context Menu");
            CollectBuiltInItems();
        }

        private void LoadSettings()
        {
            string key = $"{TYPE_NAME}_Indent";
            if (EditorPrefs.HasKey(key)) Indent = EditorPrefs.GetInt(key, Indent);

            key = $"{TYPE_NAME}_ButtonSize.x";
            if (EditorPrefs.HasKey(key)) ButtonSize.x = EditorPrefs.GetInt(key, ButtonSize.x);
            key = $"{TYPE_NAME}_ButtonSize.y";
            if (EditorPrefs.HasKey(key)) ButtonSize.y = EditorPrefs.GetInt(key, ButtonSize.y);

            key = $"{TYPE_NAME}_ShowLines";
            if (EditorPrefs.HasKey(key)) ShowLines = EditorPrefs.GetBool(key, ShowLines);
            key = $"{TYPE_NAME}_ShowLabels";
            if (EditorPrefs.HasKey(key)) ShowLabels = EditorPrefs.GetBool(key, ShowLabels);
            key = $"{TYPE_NAME}_UseExpandable";
            if (EditorPrefs.HasKey(key)) UseExpandable = EditorPrefs.GetBool(key, UseExpandable);
        }

        private void DrawSettings()
        {
            DrawLine();

            EditorGUI.BeginChangeCheck();
            Indent = EditorGUILayout.IntField("Indent", Indent);
            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetInt($"{TYPE_NAME}_Indent", Indent);
            EditorGUI.BeginChangeCheck();
            ButtonSize = EditorGUILayout.Vector2IntField("Button Height", ButtonSize);

            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log($"set {ButtonSize.x}");
                EditorPrefs.SetInt($"{TYPE_NAME}_ButtonSize.x", ButtonSize.x);
                EditorPrefs.SetInt($"{TYPE_NAME}_ButtonSize.y", ButtonSize.y);
            }

            EditorGUI.BeginChangeCheck();
            ShowLines = EditorGUILayout.Toggle("Lines", ShowLines);
            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool($"{TYPE_NAME}_ShowLines", ShowLines);

            EditorGUI.BeginChangeCheck();
            ShowLabels = EditorGUILayout.Toggle("Labels", ShowLabels);
            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool($"{TYPE_NAME}_ShowLabels", ShowLabels);

            EditorGUI.BeginChangeCheck();
            UseExpandable = EditorGUILayout.Toggle("Expandable", UseExpandable);
            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetBool($"{TYPE_NAME}_UseExpandable", UseExpandable);
        }

        void CollectBuiltInItems()
        {
            string stringData = EditorGUIUtility.SerializeMainMenuToString();


            //Debug.Log(stringData);

            var index = stringData.IndexOf("Create") + $"Create\n".Length;
            var last = $"\n{TAB}Delete\n";
            var index2 = stringData.IndexOf(last);
            var length2 = index2 - index;

            //Debug.Log($"index {index} | index2 {index2} | total length = {stringData.Length} | length {length2} | last {last}");

            stringData = stringData.Substring(index, length2);

            if (LOGS) Debug.Log(stringData);

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

                if (LOGS) Debug.Log($"[ {current.Name} ]");


                if (newDepth == previous.Depth)
                {
                    current.Value = lastParent.Value + '/' + current.Value;
                    current.ValueLower = current.Value.ToLowerInvariant();

                    current.Parent = lastParent;
                    lastParent.Children.Add(current);

                    if (LOGS) Debug.Log($"add child 1 '{lastParent.Name}' + '{current.Name}'");
                }

                if (newDepth != previous.Depth)
                {
                    if (newDepth < previous.Depth)
                    {
                        NestedString temp = lastParent;

                        while (current.Depth - temp.Depth != 1)
                        {
                            if (LOGS) Debug.Log($"temp '{temp.Name}' | temp {temp.Depth} | current {current.Depth}");

                            if (temp.Parent == null) break;

                            temp = temp.Parent;
                        }

                        lastParent = temp;

                        if (LOGS) Debug.Log($"new last parent 2 '{temp.Name}'");

                        current.Value = lastParent.Value + '/' + current.Value;
                        current.ValueLower = current.Value.ToLowerInvariant();
                        current.Parent = lastParent;
                        lastParent.Children.Add(current);
                        if (LOGS) Debug.Log($"add child 2 '{lastParent.Name}' + '{current.Name}'");
                    }
                    else
                    {
                        current.Value = previous.Value + '/' + current.Value;
                        current.ValueLower = current.Value.ToLowerInvariant();
                        current.Parent = previous;
                        previous.Children.Add(current);
                        lastParent = previous;

                        if (LOGS) Debug.Log($"add child 1 '{previous.Name}' + '{current.Name}'");
                    }
                }

                //Debug.Log($"after new {newDepth} | last {lastDepth}");

                previous = current;
            }
        }

        void OnGUI()
        {
            DrawSearch();
            scroll = GUILayout.BeginScrollView(scroll);
            int index = 0;
            Draw(Data, ref index);
            GUILayout.EndScrollView();
            DrawSettings();
        }

        private void DrawSearch()
        {
            EditorGUI.BeginChangeCheck();

            string newFilter = EditorGUILayout.TextField("Search", _filter);

            if (EditorGUI.EndChangeCheck())
            {
                _filter = newFilter;
                _filterLower = _filter.ToLowerInvariant();
            }
        }

        private void Draw(NestedString target, ref int index)
        {
            if (target.Children.Count == 0 && !string.IsNullOrEmpty(_filterLower) && !target.ValueLower.Contains(_filterLower)) return;

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));
            GUILayout.Space((target.Depth - 1) * Indent);

            if (target.Children.Count > 0)
            {
                if (UseExpandable)
                {
                    if (GUILayout.Button($"{(target.IsExpanded ? '-' : '+')} {target.Name}", "label", GUILayout.Height(ButtonSize.y), GUILayout.Width(ButtonSize.x)))
                    {
                        target.IsExpanded = !target.IsExpanded;
                    }
                }
                else
                {
                    GUILayout.Label(target.Name);
                }

                if (ShowLines)
                {
                    DrawLine();
                }
            }
            else
            {
                if (ShowLabels)
                {
                    if (GUILayout.Button(target.Name, GUILayout.Height(ButtonSize.y), GUILayout.Width(ButtonSize.x)))
                    {
                        Selection.activeObject = GetSelectedPathOrFallback();
                        EditorApplication.ExecuteMenuItem(target.Value);
                    }

                    GUILayout.Label(target.Value);
                }
                else
                {
                    if (GUILayout.Button(target.Name, GUILayout.Height(ButtonSize.y)))
                    {
                        Selection.activeObject = GetSelectedPathOrFallback();
                        EditorApplication.ExecuteMenuItem(target.Value);
                    }
                }
            }

            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            index++;

            if (UseExpandable && !target.IsExpanded && string.IsNullOrEmpty(_filter)) return;

            foreach (NestedString child in target.Children)
            {
                Draw(child, ref index);
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
