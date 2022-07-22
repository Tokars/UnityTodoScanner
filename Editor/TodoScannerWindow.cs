using System.Collections.Generic;
using System.Linq;
using TodoScanner.Editor.Core;
using TodoScanner.Editor.Helpers;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace TodoScanner.Editor
{
    public class TodoScannerWindow : EditorWindow
    {
        private AssembliesTodoData _todoCache;
        private string _searchString = "";
        private TodoData _data;
        private Vector2 _sidebarScroll;
        private Vector2 _mainAreaScroll;
        private string _currentFilterTag = string.Empty;
        private TodoEntry[] _asmTodos = new TodoEntry[] { };
        private const string TodoCache = "todo_cache";

        /// <summary>
        /// assembly name, sources[] scripts
        /// </summary>
        private Dictionary<string, string[]> _sources;

        private Dictionary<string, List<TodoEntry>> _entries;
        private float SidebarWidth => position.width / 3f;


        private string[] Tags
        {
            get
            {
                // todo: add custom tags.
                if (_data != null && _data.TagsList.Count > 0)
                    return _data.TagsList.ToArray();
                else
                    return new string[] { "TODO", "BUG" };
            }
        }

        private string SearchString
        {
            get => _searchString;
            set
            {
                if (value != _searchString)
                {
                    _searchString = value;
                    RefreshEntriesToShow();
                }
            }
        }

        [MenuItem("Window/Todo Scanner")]
        private static void Init()
        {
            var window = GetWindow<TodoScannerWindow>();
            window.minSize = new Vector2(400, 250);
            window.titleContent = new GUIContent("Todo Scanner");
            window.Show();
        }

        private void OnEnable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            ScanAllFiles();
            RefreshEntriesToShow();
        }

        private void RefreshFiles()
        {
            _sources = new Dictionary<string, string[]>();
            _entries = new Dictionary<string, List<TodoEntry>>();

            _todoCache = Resources.Load<AssembliesTodoData>(TodoCache);
            _todoCache.Init();

            foreach (var assembly in CompilationPipeline.GetAssemblies())
            {
                if (assembly.sourceFiles[0].StartsWith("Packages/")) continue;
                _entries[assembly.name] = new List<TodoEntry>();
                _sources[assembly.name] = assembly.sourceFiles;
                if (_todoCache.ContainsAssembly(assembly.name))
                    _todoCache.RefreshEntries(assembly.name, _entries[assembly.name]);
                else
                {
                    _todoCache.Add(new TodoAssemblyEntry
                    {
                        Active = true, AssemblyName = assembly.name, Entries = _entries[assembly.name]
                    });
                }
            }
        }

        private void ScanAllFiles()
        {
            RefreshFiles();
            foreach (KeyValuePair<string, string[]> assmSrc in _sources)
            {
                for (int i = 0; i < assmSrc.Value.Length; i++)
                    ScanFile(assmSrc.Value[i], assmSrc.Key);
            }

            EditorUtility.SetDirty(_todoCache);
        }

        private void ScanFile(string filePath, string assm)
        {
            var parser = new ScriptsParser(filePath, Tags);
            _entries[assm].AddRange(parser.Parse());
        }

        private void SetCurrentAssembly(string asmName)
        {
            EditorApplication.delayCall += () =>
            {
                _currentFilterTag = asmName;
                RefreshEntriesToShow();
                Repaint();
            };
        }

        private void RefreshEntriesToShow()
        {
            if (string.IsNullOrEmpty(_searchString) == false)
            {
                _currentFilterTag = string.Empty;
                var tmp = _asmTodos;
                _asmTodos = tmp.Where(e => e.Text.Contains(_searchString)).ToArray();
            }
            else if (_todoCache.ContainsAssembly(_currentFilterTag) == false)
                GetAllFiltered();
            else if (_todoCache.IsActive(_currentFilterTag))
                _asmTodos = _entries[_currentFilterTag].ToArray();
        }

        private void GetAllFiltered()
        {
            var all = new List<TodoEntry>();
            foreach (var asm in _entries)
            {
                if (_todoCache.IsActive(asm.Key) == false) continue;
                all.AddRange(asm.Value);
            }

            _currentFilterTag = string.Empty;
            _asmTodos = all.ToArray();
        }

        private string SearchField(string searchStr, params GUILayoutOption[] options)
        {
            searchStr = GUILayout.TextField(searchStr, "ToolbarSeachTextField", options);
            if (GUILayout.Button("", "ToolbarSeachCancelButton"))
            {
                searchStr = "";
                GUI.FocusControl(null);
            }

            return searchStr;
        }

        #region GUI

        private void OnGUI()
        {
            CreateStyles();
            Toolbar();
            using (new HorizontalBlock())
            {
                AssembliesSidebar();
                MainArea();
            }
        }

        private void Toolbar()
        {
            using (new HorizontalBlock(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Scan", EditorStyles.miniButton))
                    ScanAllFiles();
                if (GUILayout.Button("All Filtered", EditorStyles.miniButton))
                    GetAllFiltered();

                GUILayout.FlexibleSpace();
                SearchString = SearchField(SearchString, GUILayout.Width(250));
            }
        }

        private void MainArea()
        {
            using (new VerticalBlock(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                using (new ScrollviewBlock(ref _mainAreaScroll))
                    for (var i = 0; i < _asmTodos.Length; i++)
                        EntryLabel(i);
            }
        }

        private void AssembliesSidebar()
        {
            using (new VerticalBlock(GUI.skin.box, GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true)))
            {
                using (new ScrollviewBlock(ref _sidebarScroll))
                {
                    for (int i = 0; i < _todoCache.Assemblies.Count; i++)
                        AssemblySectionLabel(i);
                }
            }
        }

        private void AssemblySectionLabel(int index)
        {
            Event e = Event.current;
            var filter = _todoCache.Assemblies[index].AssemblyName;
            var assm = _todoCache.Assemblies[index];
            if (assm.Entries.Count == 0) return;
            using (new HorizontalBlock(EditorStyles.helpBox))
            {
                using (new ColoredBlock(filter == _currentFilterTag ? Color.cyan : Color.white))
                {
                    GUILayout.BeginHorizontal("box");
                    assm.Active = GUILayout.Toggle(assm.Active, "", GUILayout.Width(16));
                    GUILayout.Label($"{assm.AssemblyName} {"[" + assm.Entries.Count + "]"}");
                    GUILayout.EndHorizontal();
                }
            }

            var rect = GUILayoutUtility.GetLastRect();
            if (e.isMouse && e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                SetCurrentAssembly(filter);
        }

        private void EntryLabel(int index)
        {
            using (new VerticalBlock(EditorStyles.helpBox))
            {
                var entry = _asmTodos[index];
                GUILayout.Space(4f);
                using (new HorizontalBlock())
                {
                    GUILayout.Label($"{entry.Tag}", EditorStyles.helpBox, GUILayout.Width(GetWidth(entry.Tag)));
                    GUILayout.Label($"{entry.Text}");
                }

                GUILayout.Space(2f);
                using (new HorizontalBlock())
                {
                    GUILayout.Label(entry.PathToShow, _scriptStyle, GUILayout.Width(GetWidth(entry.PathToShow)));

                    if (GUILayout.Button(entry.Line.ToString(), GUILayout.Width(GetWidth(entry.Line.ToString() + 24))))
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(entry.File, entry.Line);
                }

                GUILayout.Space(5f);
            }
        }


        private GUIStyle _scriptStyle = null;
        private void CreateStyles()
        {
            if (_scriptStyle == null)
            {
                _scriptStyle = new GUIStyle(GUI.skin.label);
                _scriptStyle.fontStyle = FontStyle.Italic;
                _scriptStyle.fontSize = 11;
            }
            
        }

        private float GetWidth(string entry)
        {
            return EditorStyles.label.CalcSize(new GUIContent(entry)).x;
        }

        #endregion
    }
}