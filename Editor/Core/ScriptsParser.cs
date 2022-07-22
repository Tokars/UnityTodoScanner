using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TodoScanner.Editor.Core
{
	public class ScriptsParser
	{
		private string _filePath;
		private string _text;
		private string[] _tags;

		public ScriptsParser(string filePath, string[] tags = null)
		{
			_filePath = filePath;
			var file = new FileInfo(_filePath);
			if(file.Exists)
				_text = File.ReadAllText(filePath);
			_tags = tags;
		}

		public TodoEntry[] Parse()
		{
			var file = new FileInfo(_filePath);
			if(!file.Exists)
				return null;
			var temp = new List<TodoEntry>();
			foreach(var tag in _tags)
			{
				var matches = Regex.Matches(_text, string.Format(@"(?<=\W|^)\/\/(\s?(?i){0}(?-i))(:?)(.*)", tag));
				temp.AddRange(
					from Match match in matches
					let text = match.Groups[3].Value.Trim()
					let line = IndexToLine(match.Index)
					select new TodoEntry(text, "", tag, _filePath, line));
			}
			return temp.ToArray();
		}

		private int IndexToLine(int index)
		{
			return _text.Take(index).Count(c => c == '\n') + 1;
		}
	}
}
