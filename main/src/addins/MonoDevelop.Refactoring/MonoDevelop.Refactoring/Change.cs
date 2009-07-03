// 
// Change.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.Projects.Text;
using MonoDevelop.Projects.CodeGeneration;
using MonoDevelop.Projects.Dom;


namespace MonoDevelop.Refactoring
{
	public class Change
	{
		public string FileName {
			get;
			set;
		}
		
		public List<IType> AffectedTypes {
			get;
			private set;
		}
		
		public string Description {
			get;
			set;
		}
		
		public int Offset {
			get;
			set;
		}
		
		public int RemovedChars {
			get;
			set;
		}
		
		public string InsertedText {
			get;
			set;
		}
		
		public Change ()
		{
			this.AffectedTypes = new List<IType> ();
		}
		
		public virtual void PerformChange (IProgressMonitor monitor, RefactorerContext rctx)
		{
			if (rctx == null)
				throw new InvalidOperationException ("Refactory context not available.");

			IEditableTextFile file = rctx.GetFile (FileName);
			if (file != null) {
				if (RemovedChars > 0)
					file.DeleteText (Offset, RemovedChars);
				if (!string.IsNullOrEmpty (InsertedText))
					file.InsertText (Offset, InsertedText);
				rctx.Save ();
			}
		}
	}
	
	public class RenameFileChange : Change
	{
		public string NewName {
			get;
			set;
		}
		
		public RenameFileChange (string oldName, string newName)
		{
			this.FileName = oldName;
			this.NewName = newName;
			this.Description = string.Format (GettextCatalog.GetString ("Rename file '{0}' to '{1}'"), Path.GetFileName (oldName), Path.GetFileName (newName));
			this.Offset = -1;
		}
		
		public override void PerformChange (IProgressMonitor monitor, RefactorerContext rctx)
		{
			FileService.RenameFile (FileName, NewName);
		}
	}
}
