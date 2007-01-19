// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike KrÃ¼ger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.CodeDom.Compiler;
using System.Threading;

using MonoDevelop.Projects;
using MonoDevelop.Projects.Serialization;

using MonoDevelop.Core.Properties;
using MonoDevelop.Core;
using MonoDevelop.Core.AddIns;
using MonoDevelop.Projects.Extensions;

namespace MonoDevelop.Projects
{
	
	public enum BeforeCompileAction {
		Nothing,
		SaveAllFiles,
		PromptForSave,
	}
	
	public class ProjectService : AbstractService, IProjectService
	{
		DataContext dataContext = new DataContext ();
		ArrayList projectBindings = new ArrayList ();
		ProjectServiceExtension defaultExtension = new DefaultProjectServiceExtension ();
		ProjectServiceExtension extensionChain;
		
		FileFormatManager formatManager = new FileFormatManager ();
		IFileFormat defaultProjectFormat = new MdpFileFormat ();
		IFileFormat defaultCombineFormat = new MdsFileFormat ();
		
		public DataContext DataContext {
			get { return dataContext; }
		}
		
		public FileFormatManager FileFormats {
			get { return formatManager; }
		}
		
		internal ProjectServiceExtension ExtensionChain {
			get { return extensionChain; }
		}
		
		public CombineEntry ReadCombineEntry (string file, IProgressMonitor monitor)
		{
			return ExtensionChain.Load (monitor, file);
		}
		
		internal CombineEntry ReadFile (string file, IProgressMonitor monitor)
		{
			IFileFormat format = formatManager.GetFileFormat (file);

			if (format == null)
				throw new InvalidOperationException ("Unknown file format: " + file);
			
			CombineEntry obj = format.ReadFile (file, monitor) as CombineEntry;
			if (obj == null)
				throw new InvalidOperationException ("Invalid file format: " + file);
			
			if (obj.FileFormat == null)	
				obj.FileFormat = format;

			return obj;
		}
		
		internal void WriteFile (string file, CombineEntry entry, IProgressMonitor monitor)
		{
			IFileFormat format = entry.FileFormat;
			if (format == null) {
				if (entry is Project) format = defaultProjectFormat;
				else if (entry is Combine) format = defaultCombineFormat;
				else format = formatManager.GetFileFormatForObject (entry);
				
				if (format == null)
					throw new InvalidOperationException ("FileFormat not provided for combine entry '" + entry.Name + "'");
				entry.FileName = format.GetValidFormatName (file);
			}
			entry.FileName = file;
			format.WriteFile (entry.FileName, entry, monitor);
		}
		
		public bool CanCreateSingleFileProject (string file)
		{
			foreach (ProjectBindingCodon projectBinding in projectBindings) {
				if (projectBinding.ProjectBinding.CanCreateSingleFileProject (file))
					return true;
			}
			return false;
		}
		
		public Project CreateSingleFileProject (string file)
		{
			foreach (ProjectBindingCodon projectBinding in projectBindings) {
				if (projectBinding.ProjectBinding.CanCreateSingleFileProject (file)) {
					return projectBinding.ProjectBinding.CreateSingleFileProject (file);
				}
			}
			return null;
		}
		
		public Project CreateProject (string type, ProjectCreateInformation info, XmlElement projectOptions)
		{
			foreach (ProjectBindingCodon projectBinding in projectBindings) {
				if (projectBinding.ProjectBinding.Name == type) {
					Project project = projectBinding.ProjectBinding.CreateProject (info, projectOptions);
					return project;
				}
			}
			return null;
		}
		
		public bool IsCombineEntryFile (string filename)
		{
			if (filename.StartsWith ("file://"))
				filename = new Uri(filename).LocalPath;
				
			IFileFormat format = formatManager.GetFileFormat (filename);
			return format != null;
		}

		public override void InitializeService()
		{
			base.InitializeService();

			formatManager.RegisterFileFormat (defaultProjectFormat);
			formatManager.RegisterFileFormat (defaultCombineFormat);
			
			Runtime.AddInService.RegisterExtensionItemListener ("/SharpDevelop/Workbench/ProjectFileFormats", OnFormatExtensionChanged);
			Runtime.AddInService.RegisterExtensionItemListener ("/SharpDevelop/Workbench/SerializableClasses", OnSerializableExtensionChanged);
			Runtime.AddInService.RegisterExtensionItemListener ("/SharpDevelop/Workbench/Serialization/ExtendedProperties", OnPropertiesExtensionChanged);
			Runtime.AddInService.RegisterExtensionItemListener ("/SharpDevelop/Workbench/ProjectBindings", OnProjectsExtensionChanged);
			UpdateExtensions ();
			Runtime.AddInService.ExtensionChanged += OnExtensionChanged;
		}
		
		void OnFormatExtensionChanged (ExtensionAction action, object item)
		{
			if (action == ExtensionAction.Add) {
				FileFormatCodon codon = (FileFormatCodon) item;
				if (codon.FileFormat != null)
					formatManager.RegisterFileFormat (codon.FileFormat);
			}
		}
		
		void OnSerializableExtensionChanged (ExtensionAction action, object item)
		{
			if (action == ExtensionAction.Add) {
				Type t = ((DataTypeCodon)item).Type;
				if (t == null) {
					throw new UserException ("Type '" + ((DataTypeCodon)item).Class + "' not found. It could not be registered as a serializable type.");
				}
				DataContext.IncludeType (t);
			}
		}
		
		void OnPropertiesExtensionChanged (ExtensionAction action, object item)
		{
			if (action == ExtensionAction.Add) {
				ItemPropertyCodon cls = (ItemPropertyCodon) item;
				if (cls.ClassType != null && cls.PropertyType != null)
					DataContext.RegisterProperty (cls.ClassType, cls.PropertyName, cls.PropertyType);
			}
		}
		
		void OnProjectsExtensionChanged (ExtensionAction action, object item)
		{
			if (action == ExtensionAction.Add)
				projectBindings.Add (item);
		}
		
		void OnExtensionChanged (string path)
		{
			if (path == "/SharpDevelop/Workbench/ProjectServiceExtensions")
				UpdateExtensions ();
		}
		
		void UpdateExtensions ()
		{
			ProjectServiceExtension[] extensions = (ProjectServiceExtension[]) Runtime.AddInService.GetTreeItems ("/SharpDevelop/Workbench/ProjectServiceExtensions", typeof(ProjectServiceExtension));
			for (int n=0; n<extensions.Length - 1; n++)
				extensions [n].Next = extensions [n + 1];

			if (extensions.Length > 0) {
				extensions [extensions.Length - 1].Next = defaultExtension;
				extensionChain = extensions [0];
			} else
				extensionChain = defaultExtension;
		}
	}
	
	public class DefaultProjectServiceExtension: ProjectServiceExtension
	{
		public override void Save (IProgressMonitor monitor, CombineEntry entry)
		{
			entry.OnSave (monitor);
		}
		
		public override CombineEntry Load (IProgressMonitor monitor, string fileName)
		{
			return Services.ProjectService.ReadFile (fileName, monitor);
		}
		
		public override void Clean (CombineEntry entry)
		{
			entry.OnClean ();
		}
		
		public override ICompilerResult Build (IProgressMonitor monitor, CombineEntry entry)
		{
			return entry.OnBuild (monitor);
		}
		
		public override void Execute (IProgressMonitor monitor, CombineEntry entry, ExecutionContext context)
		{
			entry.OnExecute (monitor, context);
		}
		
		public override bool GetNeedsBuilding (CombineEntry entry)
		{
			return entry.OnGetNeedsBuilding ();
		}
		
		public override void SetNeedsBuilding (CombineEntry entry, bool value)
		{
			entry.OnSetNeedsBuilding (value);
		}
	}	
}
