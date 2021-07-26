using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.Util
{
	/// <summary>
	/// Utility class used for calculating the size of text in a <see cref="TextMesh"/>
	/// </summary>
	public class TextMeshSize
	{
		private Dictionary<char, float> charWidthMap;

		private TextMesh textMesh;
		private Renderer renderer;

		/// <summary>
		/// Constructs a new <see cref="TextMeshSize"/> for the given <see cref="TextMesh"/>
		/// </summary>
		public TextMeshSize(TextMesh tm)
		{
			textMesh = tm;
			renderer = tm.GetComponent<Renderer>();
			charWidthMap = new Dictionary<char, float>();
			GetSpace();
		}

		private void GetSpace()
		{
			string oldText = textMesh.text;

			textMesh.text = "a";
			float aw = renderer.bounds.size.x;
			charWidthMap.Add('a', aw);

			textMesh.text = "a a";
			float cw = renderer.bounds.size.x - 2 * aw;
			charWidthMap.Add(' ', cw);

			textMesh.text = oldText;
		}

		/// <summary>
		/// Returns whether the associated <see cref="TextMesh"/> still exists.
		/// </summary>
		public bool IsValid()
        {
			return textMesh != null;
        }

		/// <summary>
		/// Gets the width of the given string when displayed inside the <see cref="TextMesh"/>.
		/// </summary>
		public float GetTextWidth(string s)
		{
			char[] charList = s.ToCharArray();
			float w = 0;
			string oldText = null;

			for (int i = 0; i < charList.Length; i++)
			{
				char c = charList[i];

                if (!charWidthMap.TryGetValue(c, out float cw))
                {
                    if (oldText == null) oldText = textMesh.text;
                    textMesh.text = "" + c;
                    cw = renderer.bounds.size.x;
                    charWidthMap.Add(c, cw);
                }
                w += cw;
			}

			if (oldText != null) textMesh.text = oldText;
			return w;
		}

		/// <summary>
		/// Automatically inserts newlines into the given string in order to make it word wrap properly in the <see cref="TextMesh"/> of a given maximum width.
		/// </summary>
		public string InsertNewlines(string s, float maxWidth)
        {
			StringBuilder sb = new StringBuilder();

			char[] charList = s.ToCharArray();
			int newlineCount = 0;
			int lastSpaceIndex = -1;
			float lastSpaceWidth = 0;
			float width = 0;
			string oldText = null;

			for (int i = 0; i < charList.Length; i++)
			{
				char c = charList[i];

				if (!charWidthMap.TryGetValue(c, out float cw))
				{
					if (oldText == null) oldText = textMesh.text;
					textMesh.text = "" + c;
					cw = renderer.bounds.size.x;
					charWidthMap.Add(c, cw);
				}
				width += cw;

				if (c == '\n')
				{
					width = 0;
					lastSpaceIndex = -1;
					lastSpaceWidth = 0;
				}
				else if (c == ' ')
				{
					if (width > maxWidth)
                    {
						c = '\n';
						width = 0;
						lastSpaceIndex = -1;
						lastSpaceWidth = 0;
					}
					else
                    {
						lastSpaceIndex = i + newlineCount;
						lastSpaceWidth = width;
					}
                }
				else if (width > maxWidth)
                {
					if (lastSpaceIndex != -1)
                    {
						sb[lastSpaceIndex] = '\n';
						width -= lastSpaceWidth;
                    }
					else
                    {
						sb.Append('\n');
						newlineCount++;
						width = 0;
					}
					lastSpaceIndex = -1;
					lastSpaceWidth = 0;
				}

				sb.Append(c);
			}

			if (oldText != null) textMesh.text = oldText;

			return sb.ToString();
        }

		/// <summary>
		/// The width of the TextMesh
		/// </summary>
		public float Width { get { return GetTextWidth(textMesh.text); } }
		/// <summary>
		/// The height of the TextMesh
		/// </summary>
		public float Height { get { return renderer.bounds.size.y; } }
	}
}
