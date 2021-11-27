using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RibbonTabIconsFixer
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
        private static readonly string FolderPath = Grasshopper.Folders.SettingsFolder + _directoryName + '\\';

        private static string[] _files = null;
        private static string[] Files
        {
            get
            {
                if(_files == null)
                {
                    if (Directory.Exists(FolderPath))
                    {
                        _files = Directory.GetFiles(FolderPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(FolderPath);
                        _files = new string[0];
                    }
                }
                return _files;
            }
        }


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
                        bool isSucceed = false;
                        foreach (string itemFilePath in Files)
                        {
                            if (!itemFilePath.Contains(proxy.Desc.Category)) continue;
                            try
                            {
                                Bitmap bitmap = new Bitmap(itemFilePath);
                                _mergedCategoryIcons.Add(proxy.Desc.Category, ChangeBitmap(bitmap));
                            }
                            catch { continue; }
                            isSucceed = true;
                            break;
                        }
                        //Add icon from folder first.
                        if (isSucceed) continue;

                        //if proxy is not compiled object continue.
                        if (proxy.Kind != GH_ObjectType.CompiledObject) continue;
                        //Skip if origin have icon.
                        if (OriginCategoryIcons.ContainsKey(proxy.Desc.Category)) continue;

                        //Find icon from library.
                        GH_AssemblyInfo info = Grasshopper.Instances.ComponentServer.FindAssembly(proxy.LibraryGuid);
                        if (info == null) continue;
                        if (info.Icon == null) continue;

                        //add it to dictionary.
                        _mergedCategoryIcons.Add(proxy.Desc.Category, ChangeBitmap(info.Icon));
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

        private static Bitmap ChangeBitmap(Bitmap icon)
        {
            Bitmap bitmap = new Bitmap(16, 16, PixelFormat.Format32bppPArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.DrawImage(icon, 0, 0, 16, 16);
            graphics.Dispose();
            return bitmap;
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
            GH_ComponentServer.UpdateRibbonUI();
        }

        internal static void ReLoadIcons()
        {
            _mergedCategoryIcons = null;
            ChangeIcons(IsFixCategoryIcons);
        }

        internal static void OpenFolder()
        {
            System.Diagnostics.Process.Start(FolderPath);
        }
    }
}
