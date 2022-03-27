using System;
using System.Collections.Generic;

namespace Editor
{
    [Serializable]
    public class NestedString
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
}
