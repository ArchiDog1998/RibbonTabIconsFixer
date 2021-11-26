using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CategoryIconFix
{
    internal static class CategoryIconChange
    {
        internal static bool IsFixCategoryIcons
        {
            get 
            {
                return Grasshopper.Instances.Settings.GetValue(nameof(IsFixCategoryIcons), false);
            }
            set 
            {
                Grasshopper.Instances.Settings.SetValue(nameof(IsFixCategoryIcons), value);
                ChangeIcons(value);
            } 
        }


        private static readonly FieldInfo _categoryIconsFeild = typeof(GH_ComponentServer).GetRuntimeFields().Where((info) => info.Name.Contains("_categoryIcons")).First();

        private static SortedList<string, Bitmap> _originCategoryIcons = null;
        /// <summary>
        /// Save origin categories icons.
        /// </summary>
        private static SortedList<string, Bitmap> OriginCategoryIcons
        {
            get
            {
                if (_originCategoryIcons == null)
                {
                    //Get the already dictionary
                    SortedList<string, Bitmap> origin = _categoryIconsFeild.GetValue(Grasshopper.Instances.ComponentServer) as SortedList<string, Bitmap>;
                    _originCategoryIcons = new SortedList<string, Bitmap>(origin);
                }
                return _originCategoryIcons;
            }
        }

        private static readonly string _directoryName = "CategoryIcons";
        internal static string FolderPath => Grasshopper.Folders.SettingsFolder + _directoryName + '\\';
        private static SortedList<string, Bitmap> _mergedCategoryIcons = null;

        /// <summary>
        /// hold the categoryicon that can be changed!
        /// </summary>
        private static SortedList<string, Bitmap> MergedCategoryIcons
        {
            get
            {
                if (_mergedCategoryIcons == null)
                {
                    _mergedCategoryIcons = new SortedList<string, Bitmap>();

                    //Get additional category icons.
                    foreach (IGH_ObjectProxy proxy in Grasshopper.Instances.ComponentServer.ObjectProxies)
                    {
                        //if already have icon then skip.
                        if (_mergedCategoryIcons.ContainsKey(proxy.Desc.Category)) continue;

                        //find from directory
                        if (Directory.Exists(FolderPath))
                        {
                            bool isSucceed = false;
                            foreach (string itemFilePath in Directory.GetFiles(FolderPath))
                            {
                                if (!itemFilePath.Contains(proxy.Desc.Category)) continue;
                                try
                                {
                                    Bitmap bitmap = new Bitmap(itemFilePath);
                                    _mergedCategoryIcons.Add(proxy.Desc.Category, bitmap);
                                    isSucceed = true;
                                    break;
                                }
                                catch { continue; }
                            }
                            //Add icon from folder first.
                            if (isSucceed) continue;
                        }
                        //Create the Folder if not exists.
                        else Directory.CreateDirectory(FolderPath);

                        //if proxy is not compiled object continue.
                        if (proxy.Kind != GH_ObjectType.CompiledObject) continue;
                        //Skip if origin have icon.
                        if (OriginCategoryIcons.ContainsKey(proxy.Desc.Category)) continue;

                        //Find icon from library.
                        GH_AssemblyInfo info = Grasshopper.Instances.ComponentServer.FindAssembly(proxy.LibraryGuid);
                        if (info == null) continue;
                        if (info.Icon == null) continue;

                        //add it to dictionary.
                        _mergedCategoryIcons.Add(proxy.Desc.Category, info.Icon);
                    }

                    //Add the origin category icons.
                    foreach (var pair in OriginCategoryIcons)
                    {
                        //if already have icons, skip
                        if (_mergedCategoryIcons.ContainsKey(pair.Key)) continue;
                        _mergedCategoryIcons.Add(pair.Key, pair.Value);
                    }
                }
                return _mergedCategoryIcons;
            }
        }

        private static void ChangeIcons(bool isReplace)
        {
            if (isReplace)
            {
                _categoryIconsFeild.SetValue(Grasshopper.Instances.ComponentServer, MergedCategoryIcons);
            }
            else
            {
                _categoryIconsFeild.SetValue(Grasshopper.Instances.ComponentServer, OriginCategoryIcons);
            }
        }

        internal static void Init()
        {
            ChangeIcons(IsFixCategoryIcons);
        }
    }
}
