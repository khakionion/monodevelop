// UnknownEntryNodeBuilder.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
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
//


using System;
using MonoDevelop.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Commands;

namespace MonoDevelop.Ide.Gui.Pads.ProjectPad
{
	public class UnknownEntryNodeBuilder: TypeNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(UnknownCombineEntry); }
		}
		
		public override string ContextMenuAddinPath {
			get { return "/MonoDevelop/Ide/ContextMenu/ProjectPad/UnknownEntry"; }
		}
		
		public override Type CommandHandlerType {
			get { return typeof(UnknownEntryCommandHandler); }
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			UnknownCombineEntry entry = (UnknownCombineEntry) dataObject;
			
			if (entry.LoadError.Length > 0) {
				icon = Context.GetIcon (Gtk.Stock.DialogError);
				label = GettextCatalog.GetString ("{0} <span foreground='red' size='small'>(Load failed)</span>", entry.Name);
			} else {
				icon = Context.GetIcon (MonoDevelop.Core.Gui.Stock.Project);
				Gdk.Pixbuf gicon = Context.GetComposedIcon (icon, "fade");
				if (gicon == null) {
					gicon = Services.Icons.MakeTransparent (icon, 0.5);
					Context.CacheComposedIcon (icon, "fade", gicon);
				}
				icon = gicon;
				label = entry.Name;
			}
		}
		
		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			UnknownCombineEntry entry = (UnknownCombineEntry) dataObject;
			return entry.LoadError.Length > 0;
		}
		
		public override void BuildChildNodes (ITreeBuilder treeBuilder, object dataObject)
		{
			UnknownCombineEntry entry = (UnknownCombineEntry) dataObject;
			if (entry.LoadError.Length > 0)
				treeBuilder.AddChild (new TreeViewItem (entry.LoadError));
		}

		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return ((UnknownCombineEntry)dataObject).Name;
		}
	}
	
	public class UnknownEntryCommandHandler: NodeCommandHandler
	{
		[CommandHandler (ProjectCommands.Reload)]
		public void OnReload ()
		{
			UnknownCombineEntry entry = (UnknownCombineEntry) CurrentNode.DataItem;
			using (IProgressMonitor m = IdeApp.Workbench.ProgressMonitors.GetLoadProgressMonitor (true)) {
				entry.ParentCombine.ReloadEntry (m, entry);
			}
		}
		
		[CommandUpdateHandler (ProjectCommands.Reload)]
		public void OnUpdateReload (CommandInfo info)
		{
			UnknownCombineEntry entry = (UnknownCombineEntry) CurrentNode.DataItem;
			info.Enabled = entry.ParentCombine != null;
		}
		
		[CommandHandler (EditCommands.Delete)]
		public void RemoveItem ()
		{
			UnknownCombineEntry entry = (UnknownCombineEntry) CurrentNode.DataItem;
			Combine cmb = entry.ParentCombine;
			if (cmb == null)
				return;
			
			bool yes = Services.MessageService.AskQuestion (GettextCatalog.GetString (
				"Do you really want to remove project '{0}' from solution '{1}'", entry.FileName, cmb.Name));

			if (yes) {
				cmb.RemoveEntry (entry);
				entry.Dispose ();
				IdeApp.ProjectOperations.SaveCombineEntry (cmb);
			}
		}
	}
}
