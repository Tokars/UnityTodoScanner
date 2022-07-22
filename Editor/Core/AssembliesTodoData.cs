using System;
using System.Collections.Generic;
using UnityEngine;

namespace TodoScanner.Editor.Core
{
    [CreateAssetMenu(menuName = "ToDo Manager/Asm Todos")]
    [Serializable]
    public class AssembliesTodoData : ScriptableObject
    {
        [SerializeField] private List<TodoAssemblyEntry> assemblies = null;
        public List<TodoAssemblyEntry> Assemblies => assemblies;

        private Dictionary<string, TodoAssemblyEntry> _registry;

        public void Init()
        {
            _registry = new Dictionary<string, TodoAssemblyEntry>();
            for (int i = 0; i < assemblies.Count; i++)
            {
                var a = assemblies[i];
                _registry.Add(a.AssemblyName, a);
            }
        }

        public bool ContainsAssembly(string assmName)
        {
            return _registry.ContainsKey(assmName);
        }

        public void Add(TodoAssemblyEntry assemblyEntry)
        {
            assemblies.Add(assemblyEntry);
            _registry.Add(assemblyEntry.AssemblyName, assemblyEntry);
        }

        public void RemoveAssembly(string key)
        {

        }

        public void Clear()
        {
            assemblies.Clear();
        }

        public void RefreshEntries(string assmName, List<TodoEntry> todoEntries)
        {
            _registry[assmName].Entries = todoEntries;
        }

        public bool IsActive(string assmSrcKey)
        {
            return ContainsAssembly(assmSrcKey) && _registry[assmSrcKey].Active;
        }
    }
}
