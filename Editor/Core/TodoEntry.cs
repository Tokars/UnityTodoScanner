using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TodoScanner.Editor.Core
{
	[System.Serializable]
    public class TodoAssemblyEntry
    {
        [SerializeField] private string assemblyName = default;
        [SerializeField] private List<TodoEntry> entries = null;
        [SerializeField] private bool active;

        public bool Active
        {
            get => active;
            set => active = value;
        }

        public string AssemblyName
        {
            get => assemblyName;
            set => assemblyName = value;
        }

        public List<TodoEntry> Entries
        {
            get => entries;
            set => entries = value;
        }

        public TodoAssemblyEntry()
        {
        }
    }


	[System.Serializable]
	public class TodoEntry
	{
		public string Text;
		public string Note;
		public string Tag;
		public string File;
		public int Line;

		public string PathToShow;

		public TodoEntry(string text, string note, string tag, string file, int line)
        {
            if (text.StartsWith(":"))
                text = text.Replace(":", "");

			Text = text;
			Note = note;
			Tag = tag;
			File = file;
			Line = line;

            PathToShow = Path.GetFileName(File.Replace("\\", "/"));
        }

		public override bool Equals(object obj)
		{
			var x = this;
			var y = (TodoEntry)obj;
			if(ReferenceEquals(x, y))
				return true;
			if(ReferenceEquals(x, null))
				return false;
			if(ReferenceEquals(y, null))
				return false;
			if(x.GetType() != y.GetType())
				return false;
			return string.Equals(x.Text, y.Text) && string.Equals(x.Note, y.Note) && string.Equals(x.Tag, y.Tag) && string.Equals(x.File, y.File) && x.Line == y.Line;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var obj = this;
				var hashCode = (obj.Text != null ? obj.Text.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Note != null ? obj.Note.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Tag != null ? obj.Tag.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.File != null ? obj.File.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.Line;
				return hashCode;
			}
		}
	}
}
