// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.Ide.StandardHeaders {
    
    
    public partial class StandardHeaderPanel {
        
        private Gtk.VBox vbox2;
        
        private Gtk.ScrolledWindow scrolledwindow1;
        
        private Gtk.TextView headerTextview;
        
        private Gtk.HBox hbox1;
        
        private Gtk.CheckButton generateCommentsCheckbutton;
        
        private Gtk.CheckButton emitstandardHeaderCheckbutton;
        
        private Gtk.HBox hbox2;
        
        private Gtk.Label label1;
        
        private Gtk.ComboBox templateCombobox;
        
        private Gtk.Button removeButton;
        
        private Gtk.Button addButton;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget MonoDevelop.Ide.StandardHeaders.StandardHeaderPanel
            Stetic.BinContainer.Attach(this);
            this.Name = "MonoDevelop.Ide.StandardHeaders.StandardHeaderPanel";
            // Container child MonoDevelop.Ide.StandardHeaders.StandardHeaderPanel.Gtk.Container+ContainerChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.scrolledwindow1 = new Gtk.ScrolledWindow();
            this.scrolledwindow1.CanFocus = true;
            this.scrolledwindow1.Name = "scrolledwindow1";
            this.scrolledwindow1.VscrollbarPolicy = ((Gtk.PolicyType)(1));
            this.scrolledwindow1.HscrollbarPolicy = ((Gtk.PolicyType)(1));
            this.scrolledwindow1.ShadowType = ((Gtk.ShadowType)(1));
            this.scrolledwindow1.BorderWidth = ((uint)(6));
            // Container child scrolledwindow1.Gtk.Container+ContainerChild
            this.headerTextview = new Gtk.TextView();
            this.headerTextview.CanFocus = true;
            this.headerTextview.Name = "headerTextview";
            this.scrolledwindow1.Add(this.headerTextview);
            this.vbox2.Add(this.scrolledwindow1);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox2[this.scrolledwindow1]));
            w2.Position = 0;
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 6;
            // Container child hbox1.Gtk.Box+BoxChild
            this.generateCommentsCheckbutton = new Gtk.CheckButton();
            this.generateCommentsCheckbutton.CanFocus = true;
            this.generateCommentsCheckbutton.Name = "generateCommentsCheckbutton";
            this.generateCommentsCheckbutton.Label = Mono.Unix.Catalog.GetString("Generate Comments");
            this.generateCommentsCheckbutton.DrawIndicator = true;
            this.generateCommentsCheckbutton.UseUnderline = true;
            this.hbox1.Add(this.generateCommentsCheckbutton);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.hbox1[this.generateCommentsCheckbutton]));
            w3.Position = 0;
            // Container child hbox1.Gtk.Box+BoxChild
            this.emitstandardHeaderCheckbutton = new Gtk.CheckButton();
            this.emitstandardHeaderCheckbutton.CanFocus = true;
            this.emitstandardHeaderCheckbutton.Name = "emitstandardHeaderCheckbutton";
            this.emitstandardHeaderCheckbutton.Label = Mono.Unix.Catalog.GetString("Emit Standard Header");
            this.emitstandardHeaderCheckbutton.DrawIndicator = true;
            this.emitstandardHeaderCheckbutton.UseUnderline = true;
            this.hbox1.Add(this.emitstandardHeaderCheckbutton);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.hbox1[this.emitstandardHeaderCheckbutton]));
            w4.Position = 1;
            this.vbox2.Add(this.hbox1);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
            w5.Position = 1;
            w5.Expand = false;
            w5.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbox2 = new Gtk.HBox();
            this.hbox2.Name = "hbox2";
            this.hbox2.Spacing = 6;
            // Container child hbox2.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Select Template:");
            this.hbox2.Add(this.label1);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.hbox2[this.label1]));
            w6.Position = 0;
            w6.Expand = false;
            w6.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.templateCombobox = Gtk.ComboBox.NewText();
            this.templateCombobox.Name = "templateCombobox";
            this.hbox2.Add(this.templateCombobox);
            Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(this.hbox2[this.templateCombobox]));
            w7.Position = 1;
            // Container child hbox2.Gtk.Box+BoxChild
            this.removeButton = new Gtk.Button();
            this.removeButton.CanFocus = true;
            this.removeButton.Name = "removeButton";
            this.removeButton.UseStock = true;
            this.removeButton.UseUnderline = true;
            this.removeButton.Label = "gtk-remove";
            this.hbox2.Add(this.removeButton);
            Gtk.Box.BoxChild w8 = ((Gtk.Box.BoxChild)(this.hbox2[this.removeButton]));
            w8.PackType = ((Gtk.PackType)(1));
            w8.Position = 2;
            w8.Expand = false;
            w8.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.addButton = new Gtk.Button();
            this.addButton.CanFocus = true;
            this.addButton.Name = "addButton";
            this.addButton.UseStock = true;
            this.addButton.UseUnderline = true;
            this.addButton.Label = "gtk-add";
            this.hbox2.Add(this.addButton);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.hbox2[this.addButton]));
            w9.PackType = ((Gtk.PackType)(1));
            w9.Position = 3;
            w9.Expand = false;
            w9.Fill = false;
            this.vbox2.Add(this.hbox2);
            Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
            w10.PackType = ((Gtk.PackType)(1));
            w10.Position = 2;
            w10.Expand = false;
            w10.Fill = false;
            this.Add(this.vbox2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Show();
        }
    }
}
