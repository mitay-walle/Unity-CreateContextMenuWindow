using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MenuItemWindow : ItemMenuWindowBase
    {
        private HashSet<string> _excluded = new HashSet<string>()
        {
            "CONTEXT", ""
        };

        private string[] _toolbarValues;
        private List<NestedString> _data = new List<NestedString>();
        private int _toolbar;

        List<string> tempStrings = new List<string>();

        [MenuItem("Window/Menu Item Window")]
        public static void OpenWindow()
        {
            var window = GetWindow<MenuItemWindow>();
            if (!window) CreateInstance<MenuItemWindow>();
        }

        protected override void OnEnable()
        {
            LoadSettings();
            string stringData = EditorGUIUtility.SerializeMainMenuToString();
            _data.Clear();

            if (LOGS) Debug.Log(stringData);

            var lines = stringData.Split('\n').ToList();
            var firstLines = lines.Where(str => !str.StartsWith(TAB) && !_excluded.Contains(str.Trim())).ToList();

            var firstLineIndexes = new List<int>();

            foreach (string firstLine in firstLines)
            {
                firstLineIndexes.Add(lines.IndexOf(firstLine));
            }

            for (int i = 0; i < firstLineIndexes.Count; i++)
            {
                tempStrings.Clear();

                int j = firstLineIndexes[i] + 1;

                while (j < lines.Count && !string.IsNullOrEmpty(lines[j]) && lines[j].Contains(TAB))
                {
                    tempStrings.Add(lines[j]);
                    j++;
                }

                stringData = string.Join("\n", tempStrings);
                if (LOGS) Debug.Log(stringData);

                NestedString data = new NestedString(firstLines[i])
                {
                    IsExpanded = true
                };

                CollectBuiltInItems(data, stringData);
                _data.Add(data);
            }

            _toolbarValues = firstLines.ToArray();


            titleContent = new GUIContent("Menu Item");
        }

        protected override void OnGUI()
        {
            DrawSearch();

            int newToolbar = GUILayout.Toolbar(_toolbar, _toolbarValues);

            if (_toolbar != newToolbar)
            {
                _toolbar = newToolbar;
            }

            scroll = GUILayout.BeginScrollView(scroll);
            int index = 0;
            Draw(_data[_toolbar], ref index);
            GUILayout.EndScrollView();
            DrawSettings();
        }
    }
}
