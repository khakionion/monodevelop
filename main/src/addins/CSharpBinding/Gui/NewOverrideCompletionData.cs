// NewOverrideCompletionData.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Text;
using MonoDevelop.Projects.Gui.Completion;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Output;

using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;

namespace MonoDevelop.CSharpBinding
{
	public class NewOverrideCompletionData : CompletionData, IActionCompletionData
	{
		TextEditor editor;
		IMember member;
		static Ambience ambience = new MonoDevelop.CSharpBinding.CSharpAmbience ();
		string indent;
		int    initialOffset;
		int    declarationBegin;
		bool   insertPrivate;
		bool   insertSealed;
		IType  type;
		
		public NewOverrideCompletionData (TextEditor editor, int declarationBegin, IType type, IMember member)
			: base (null)
		{
			this.editor = editor;
			this.type   = type;
			this.member = member;
			this.initialOffset = editor.CursorPosition;
			this.declarationBegin = declarationBegin;
			string declarationText = editor.GetText (declarationBegin, initialOffset);
			insertPrivate = declarationText.Contains ("private");
			insertSealed  = declarationText.Contains ("sealed");
			
			this.indent = GetIndentString (editor, editor.CursorPosition);
			this.Icon = member.StockIcon;
			this.DisplayText = ambience.GetString (member, OutputFlags.IncludeParameters | OutputFlags.IncludeGenerics
			                                       | OutputFlags.EmitMarkup | OutputFlags.HideExtensionsParameter);
			this.CompletionText = member.Name;
		}
		
		public void InsertCompletionText (ICompletionWidget widget, ICodeCompletionContext context)
		{
			editor.DeleteText (declarationBegin, editor.CursorPosition - declarationBegin);
			string mod = GetModifiers (member);
			
			if (insertPrivate && String.IsNullOrEmpty (mod)) {
				editor.InsertText (editor.CursorPosition, "private ");
			} else {
				editor.InsertText (editor.CursorPosition, mod);
			}
			
			if (insertSealed)
				editor.InsertText (editor.CursorPosition, "sealed ");
				
			if (member.DeclaringType.ClassType != ClassType.Interface && (member.IsVirtual || member.IsAbstract))
				editor.InsertText (editor.CursorPosition, "override ");
				
			if (member is IMethod) {
				InsertMethod (member as IMethod);
				return;
			}
			if (member is IProperty) {
				InsertProperty (member as IProperty);
				return;
			}
			editor.InsertText (editor.CursorPosition, member.Name);
		}
		
		internal static string GetIndentString (TextEditor editor, int pos)
		{
			string ch = editor.GetText (pos - 1, pos);
			int nwpos = pos;
			while (ch.Length > 0 && ch != "\n") {
				if (ch[0] != ' ' && ch[0] != '\t')
					nwpos = pos;
				pos--;
				ch = editor.GetText (pos - 1, pos);
			}
			return editor.GetText (pos, nwpos - 1);
		}
		
		void GenerateMethodBody (StringBuilder sb, IMethod method)
		{
			sb.Append (this.indent);
			sb.Append (SingleIndent);
			if (method.Name == "ToString" && (method.Parameters == null || method.Parameters.Count == 0) && method.ReturnType != null && method.ReturnType.FullName == "System.String") {
				sb.Append ("return string.Format(");
				sb.Append ("\"[");
				sb.Append (type.Name);
				if (type.PropertyCount > 0) 
					sb.Append (": ");
				int i = 0;
				foreach (IProperty property in type.Properties) {
					if (property.IsStatic || !property.IsPublic)
						continue;
					if (i > 0)
						sb.Append (", ");
					sb.Append (property.Name);
					sb.Append ("={");
					sb.Append (i++);
					sb.Append ("}");
				}
				sb.Append ("]\"");
				foreach (IProperty property in type.Properties) {
					if (property.IsStatic || !property.IsPublic)
						continue;
					sb.Append (", ");
					sb.Append (property.Name);
				}
				sb.Append (");");
				sb.AppendLine ();
				return;
			}
			
			if (method.ReturnType != null && method.ReturnType.FullName != "System.Void") {
				sb.Append ("return base.");
				sb.Append (method.Name);
				sb.Append (" (");
				if (method.Parameters != null) {
					for (int i = 0; i < method.Parameters.Count; i++) {
						if (i > 0)
							sb.Append (", ");
							
						// add parameter modifier
						if (method.Parameters[i].IsOut) {
							sb.Append ("out ");
						} else if (method.Parameters[i].IsRef) {
							sb.Append ("ref ");
						}
						
						sb.Append (method.Parameters[i].Name);
					}
				}
				sb.Append (");");
			} else {
				sb.Append ("throw new System.NotImplementedException ();");
			} 
			sb.AppendLine ();
		}
		
		void InsertMethod (IMethod method)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (ambience.GetString (method, OutputFlags.ClassBrowserEntries | OutputFlags.IncludeParameterName));
			sb.AppendLine ();
			sb.Append (this.indent);
			sb.AppendLine ("{");
			GenerateMethodBody (sb, method);
			sb.Append (this.indent);
			sb.Append ("}"); 
			sb.AppendLine ();
			editor.InsertText (editor.CursorPosition, sb.ToString ());
		}
		
		string SingleIndent {
			get {
				if (TextEditorProperties.ConvertTabsToSpaces) 
					return new string (' ', TextEditorProperties.TabIndent);
				return "\t";
			}
		}
			
		void GeneratePropertyBody (StringBuilder sb, IProperty property)
		{
			if (property.HasGet) {
				sb.Append (this.indent);
				sb.Append (SingleIndent);
				sb.AppendLine ("get {");
				sb.Append (this.indent);
				sb.Append (SingleIndent);
				sb.Append (SingleIndent);
				if (!property.IsAbstract) {
					sb.Append (" return base.");
					sb.Append (property.Name);
					sb.Append (";");
				}
				sb.AppendLine ();
				sb.Append (this.indent);
				sb.Append (SingleIndent);
				sb.AppendLine ("}");
			}
			
			if (property.HasSet) {
				sb.Append (this.indent);
				sb.Append (SingleIndent);
				sb.AppendLine ("set {");
				sb.Append (this.indent);
				sb.Append (SingleIndent);
				sb.Append (SingleIndent);
				sb.AppendLine ("throw new System.NotImplementedException ();");
				sb.Append (this.indent);
				sb.Append (SingleIndent);
				sb.AppendLine ("}");
			}
		}
		void InsertProperty (IProperty property)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (ambience.GetString (property, OutputFlags.ClassBrowserEntries | OutputFlags.IncludeParameterName));
			sb.AppendLine (" {");
			GeneratePropertyBody (sb, property);
			sb.Append (this.indent);
			sb.Append ("}"); 
			sb.AppendLine ();
			editor.InsertText (editor.CursorPosition, sb.ToString ());
		}
		
		string GetModifiers (IMember member)
		{
			if (member.IsPublic) 
				return "public ";
			if (member.IsPrivate) 
				return "";
				
			if (member.IsProtectedAndInternal) 
				return "protected internal ";
			if (member.IsProtectedOrInternal) 
				return "internal protected ";
			
			if (member.IsProtected) 
				return "protected ";
			if (member.IsInternal) 
				return "internal ";
				
			return "";
		}
	}
}
