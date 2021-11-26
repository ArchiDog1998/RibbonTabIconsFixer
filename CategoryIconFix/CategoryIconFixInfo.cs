using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace CategoryIconFix
{
    public class CategoryIconFixInfo : GH_AssemblyInfo
    {
        public override string Name => "CategoryIconFix";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Fix Category icon can't show bitmap bug.";

        public override Guid Id => new Guid("C276B873-B256-4BE9-8B1C-2CA4F7A77BE3");

        //Return a string identifying you or your company.
        public override string AuthorName => "秋水";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "1123993881@qq.com";
    }
}