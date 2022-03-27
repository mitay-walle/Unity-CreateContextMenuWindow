using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ContextMenuWindow : ItemMenuWindowBase
    {
        [MenuItem("Window/Create Context Menu Window")]
        public static void OpenWindow()
        {
            var window = GetWindow<ContextMenuWindow>();
            if (!window) CreateInstance<ContextMenuWindow>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Create Context Menu");
        }
    }
}
