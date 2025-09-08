// bitmap 은 필요없을 듯
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;

//namespace MGAlienLib
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class DynamicBitmapPacker
//    {
//        private int _initailPageSize = 2048;
//        private int _padding = 0;
//        private int _IdSeed = 0;

//        private List<(DynamicRectPacker packer, Bitmap bitmap)> pages = new();

//        //public class Key
//        //{
//        //    public int Page;
//        //    public int Id;

//        //    public override bool Equals(object obj)
//        //    {
//        //        return obj is Key key &&
//        //               Page == key.Page &&
//        //               Id == key.Id;
//        //    }

//        //    public static bool operator >(Key a, Key b)
//        //    {
//        //        if (a.Page > b.Page)
//        //        {
//        //            return true;
//        //        }
//        //        else if (a.Page == b.Page)
//        //        {
//        //            return a.Id > b.Id;
//        //        }
//        //        else
//        //        {
//        //            return false;
//        //        }
//        //    }

//        //    public static bool operator <(Key a, Key b)
//        //    {
//        //        if (a.Page < b.Page)
//        //        {
//        //            return true;
//        //        }
//        //        else if (a.Page == b.Page)
//        //        {
//        //            return a.Id < b.Id;
//        //        }
//        //        else
//        //        {
//        //            return false;
//        //        }
//        //    }

//        //    public static bool operator >=(Key a, Key b)
//        //    {
//        //        return a > b || a == b;
//        //    }

//        //    public static bool operator <=(Key a, Key b)
//        //    {
//        //        return a < b || a == b;
//        //    }

//        //    public static bool operator ==(Key a, Key b)
//        //    {
//        //        return a.Page == b.Page && a.Id == b.Id;
//        //    }

//        //    public static bool operator !=(Key a, Key b)
//        //    {
//        //        return a.Page != b.Page || a.Id != b.Id;
//        //    }

//        //    public override int GetHashCode()
//        //    {
//        //        return HashCode.Combine(Page, Id);
//        //    }
//        //}

//        public int Insert(Bitmap source)
//        {
//            _IdSeed++;
//            int Id = _IdSeed;

//            if (pages.Count == 0)
//            {
//                pages.Add(
//                        (new DynamicRectPacker(_initailPageSize, _initailPageSize, _padding),
//                        new Bitmap(_initailPageSize, _initailPageSize, 1))
//                    );
//            }

//            for (int i = 0; i < pages.Count; i++)
//            {
//                if (pages[i].packer.TryInsert(source.Width, source.Height, Id, out var rect))
//                {
//                    Bitmap.Blit(pages[i].bitmap, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), 
//                        source, new Rectangle(0, 0, source.Width, source.Height));
//                    return Id;
//                }
//            }

//            return -1;
//        }

//        public void debug_SavePng(string path)
//        {
//            for (int i = 0; i < pages.Count; i++)
//            {
//                var stream = new System.IO.FileStream($"{path}_{i}.png", System.IO.FileMode.Create);
//                {
//                    var tex = pages[i].bitmap.ToTexture2D();
//                    tex.SaveAsPng(stream, tex.Width, tex.Height);
//                }
//            }
//        }
//    }
//}
