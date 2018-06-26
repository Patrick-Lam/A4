using A4;
using A4.DATA.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace A4UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int debug = 0;

            string projectPath = @"C:\Code\A4\A4UnitTest\";

            string rootPath = projectPath + @"\Lab\Code\Template\debug\";
            string itemPath = projectPath + @"\Lab\Code\Template\HTMLInputItems\";

            DirectoryInfo root = new DirectoryInfo(rootPath);
            DirectoryInfo item = new DirectoryInfo(itemPath);

            List<Template> templates = new List<Template>();

            foreach (DirectoryInfo dir in root.GetDirectories())
            {
                if (dir.FullName.TrimEnd(new char[] { '\\' }) == item.FullName.TrimEnd(new char[] { '\\' })) continue;

                Template t = new Template(dir, item);

                string debug3 = t.ToString();

                Page.DATA2 d2 = new Page.DATA2();

                int table = 0;

                // ===

                table = 0;

                d2.Add(table, new System.Collections.Generic.List<RowItems>() { });


                d2[table].Add(new RowItems() { });

                d2[table][d2[table].Count - 1].Items = new System.Collections.Generic.List<Item>();

                d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r1t1" });
                d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r1t2" });



                d2[table].Add(new RowItems() { });

                d2[table][d2[table].Count - 1].Items = new System.Collections.Generic.List<Item>();

                d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r2t1" });
                d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r2t2" });
                d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r2t3" });



                //d2[table].Add(new RowItems() { });

                //d2[table][d2[table].Count - 1].Items = new System.Collections.Generic.List<Item>();

                //d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r3t1" });



                //d2[table].Add(new RowItems() { });

                //d2[table][d2[table].Count - 1].Items = new System.Collections.Generic.List<Item>();

                //d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r4t1" });



                d2[table].Add(new RowItems() { });

                d2[table][d2[table].Count - 1].InnerRowItems = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<RowItems>>();

                d2[table][d2[table].Count - 1].InnerRowItems.Add(0, new List<RowItems>());

                d2[table][d2[table].Count - 1].InnerRowItems[0].Add(new RowItems());

                d2[table][d2[table].Count - 1].InnerRowItems[0][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t1 r1t1" });
                d2[table][d2[table].Count - 1].InnerRowItems[0][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t1 r1t2" });



                //d2[table][d2[table].Count - 1].InnerRowItems.Add(1, new List<RowItems>());

                //d2[table][d2[table].Count - 1].InnerRowItems[1].Add(new RowItems());

                //d2[table][d2[table].Count - 1].InnerRowItems[1][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r1t1" });
                //d2[table][d2[table].Count - 1].InnerRowItems[1][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r1t2" });

                //d2[table][d2[table].Count - 1].InnerRowItems[1].Add(new RowItems());

                //d2[table][d2[table].Count - 1].InnerRowItems[1][1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r2t1" });
                //d2[table][d2[table].Count - 1].InnerRowItems[1][1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r2t2" });

                // ===

                //d2[table][d2[table].Count - 1].InnerRowItems.Add(2, new List<RowItems>());

                //d2[table][d2[table].Count - 1].InnerRowItems[2].Add(new RowItems());

                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r3t1" });
                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r3t2" });

                // === level ++

                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].InnerRowItems = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<RowItems>>();

                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].InnerRowItems.Add(0, new List<RowItems>());

                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].InnerRowItems[0].Add(new RowItems());

                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].InnerRowItems[0][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r3t1 r0" });
                //d2[table][d2[table].Count - 1].InnerRowItems[2][0].InnerRowItems[0][0].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "sub t2 r3t1 r0" });

                // ===



                //d2[table].Add(new RowItems() { });

                //d2[table][d2[table].Count - 1].Items = new System.Collections.Generic.List<Item>();

                //d2[table][d2[table].Count - 1].Items.Add(new Item() { ColumnName = "a", Name = "a", Value = "r5t1" });



                Page.Public_DataRender_WhilePage_TrGroup = (_templateTable, _tableIndex, _zIndex, _innerTableIndex, _pager, _data) =>
                {
                    var data = ((Page.DATA2)_data)[_data.TemplateTableIndex][_pager.CurrentPage - 1];

                    int trGroupIndex = _templateTable.PrepareTrGroup(data);

                    return trGroupIndex;
                };

                // ===

                Page p = new Page(t, d2);

                string debug4 = p.ToString();

                int __debug4__ = 0;

                // ===

                break;

                //templates.Add(t);
            }

            int debug1 = 0;
        }
    }
}
