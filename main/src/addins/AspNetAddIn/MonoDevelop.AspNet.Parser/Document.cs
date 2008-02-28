//
// Document.cs: Parses an ASP.NET file, and provides a range of services for 
//     gathering information from it.
//
// Authors:
//   Michael Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (C) 2006 Michael Hutchinson
//
//
// This source code is licenced under The MIT License:
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections.Generic;

using MonoDevelop.AspNet.Parser.Dom;
using MonoDevelop.Projects;

namespace MonoDevelop.AspNet.Parser
{
	public class Document
	{
		RootNode rootNode;
		ProjectFile projectFile;
		DocumentReferenceManager refMan;
		PageInfoVisitor info;
		MemberListVisitor memberList;
		List<ParserException> errors = new List<ParserException> ();
		
		public Document (ProjectFile file)
		{
			this.projectFile = file;
			rootNode = new RootNode ();
			
			StreamReader sr = null;
			try {
				sr = new StreamReader (file.FilePath);
				rootNode.Parse (file.FilePath, sr);
			} catch (Exception ex) {
				MonoDevelop.Core.LoggingService.LogError ("Unhandled error parsing ASP.NET document", ex);
				errors.Add (new ParserException (null, "Unhandled error parsing ASP.NET document: " + ex.ToString ()));
			} finally {
				if (sr != null)
					sr.Close ();
			}
			
			foreach (MonoDevelop.AspNet.Parser.Internal.ParseException ex in rootNode.ParseErrors)
				errors.Add (new ParserException (ex.Location, ex.Message));
			
			if (MonoDevelop.Core.LoggingService.IsLevelEnabled (MonoDevelop.Core.Logging.LogLevel.Debug)) {
				DebugStringVisitor dbg = new DebugStringVisitor ();
				rootNode.AcceptVisit (dbg);
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				sb.AppendLine ("Parsed AspNet file:");
				sb.AppendLine (dbg.DebugString);
				if (errors.Count > 0) {
					sb.AppendLine ("Errors:");
					foreach (ParserException ex in errors)
						sb.AppendLine (ex.ToString ());
				}
				MonoDevelop.Core.LoggingService.LogDebug (sb.ToString ());
			}
		}
		
		public bool IsValid {
			get { return errors.Count == 0; }
		}
		
		public IList<ParserException> ParseErrors {
			get { return errors; }
		}
		
		public RootNode RootNode {
			get { return rootNode; }
		}
		
		public DocumentReferenceManager ReferenceManager {
			get {
				if (refMan == null)
					refMan = new DocumentReferenceManager (this);
				return refMan;
			}
		}
		
		public PageInfoVisitor Info {
			get {
				if (info == null) {
					info = new PageInfoVisitor ();
					rootNode.AcceptVisit (info);
				}
				return info;
			}
		}
		
		public AspNetAppProject Project {
			get { return (AspNetAppProject) projectFile.Project; }
		}
		
		public ProjectFile ProjectFile {
			get { return projectFile; }
		}
		
		public MemberListVisitor MemberList {
			get {
				if (memberList == null) {
					memberList = new MemberListVisitor (this);
					rootNode.AcceptVisit (memberList);
				}
				return memberList;
			}
		}
	}
}
