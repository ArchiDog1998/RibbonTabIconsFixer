using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RibbonTabIconsFixer
{
    public class RibbonTabIconsFixerInfo : GH_AssemblyInfo
    {
        public override string Name => "CategoryIconFixer";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.CategoryIconFixerIcon_24;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Fix Category icon can't show bitmap bug.";

        public override Guid Id => new Guid("C276B873-B256-4BE9-8B1C-2CA4F7A77BE3");

        //Return a string identifying you or your company.
        public override string AuthorName => "秋水";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "1123993881@qq.com";
    }

    public class CategoryIconFixAssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Instances_CanvasCreated(GH_Canvas canvas)
        {
            Grasshopper.Instances.CanvasCreated -= Instances_CanvasCreated;

            GH_DocumentEditor editor = Grasshopper.Instances.DocumentEditor;
            if (editor == null)
            {
                Grasshopper.Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;
                return;
            }
            DoingSomethingFirst(editor);
        }

        private void ActiveCanvas_DocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
        {
            Grasshopper.Instances.ActiveCanvas.DocumentChanged -= ActiveCanvas_DocumentChanged;

            GH_DocumentEditor editor = Grasshopper.Instances.DocumentEditor;
            if (editor == null)
            {
                MessageBox.Show("CategoryIconFix can't find the menu!");
                return;
            }
            DoingSomethingFirst(editor);
        }

        private void DoingSomethingFirst(GH_DocumentEditor editor)
        {
            //Load Icons to replace.
            CategoryIconChange.ReLoadIcons();


            //Major MenuItem.
            ToolStripMenuItem major = new ToolStripMenuItem("Ribbon Tab Icons Fix", Properties.Resources.CategoryIconFixerIcon_24, (sender, e) =>
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                CategoryIconChange.IsFixCategoryIcons = ((ToolStripMenuItem)sender).Checked;
            })
            { Checked = CategoryIconChange.IsFixCategoryIcons};

            //Open Folder Item.
            ToolStripMenuItem openDirectory = new ToolStripMenuItem("Open Icon Folder");
            openDirectory.Click += (sender, e) =>
            {
                CategoryIconChange.OpenFolder();
            };
            major.DropDownItems.Add(openDirectory);

            //Reload icons item.
            ToolStripMenuItem relaodIcon = new ToolStripMenuItem("Reload Icons");
            relaodIcon.Click += (sender, e) =>
            {
                CategoryIconChange.ReLoadIcons();
            };
            major.DropDownItems.Add(relaodIcon);

            //Add to Display Menu.
            ToolStripMenuItem viewItem = (ToolStripMenuItem)editor.MainMenuStrip.Items[2];
            viewItem.DropDownItems.Insert(3, major);

        }
    }
}