using A4.DATA.Model;
using A4.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A4
{
    public class Page
    {
        public delegate int DataRender_WhilePage_TrGroupFunc(Template.Table _templateTable, int _tableIndex, int _zIndex, int _innerTableIndex, Pager _pager, IDATA _data);

        public delegate void DataRender_WhilePageFunc(Template.Table _templateTable, int _tableIndex, int _trGroupIndex, Pager _pager, IDATA _data, DataRender_WhilePage_LoopFunc _dataReader_WhilePage_LoopFunc);

        public delegate void DataRender_WhilePage_LoopFunc(Template.TR _templateTr, Pager _pager, IDATA _data, int _ii, int _index);

        public static string TemplateTag = "<!-- {0} -->";

        public static string ReplaceHolderTemplateTag = "<!-- Template_{0}_ReplaceHolder_{1} -->";

        public static string ReplaceHolderTemplateTag2 = "/* Template_{0}_ReplaceHolder_{1} */";

        public static DataRender_WhilePage_TrGroupFunc Public_DataRender_WhilePage_TrGroup = null;

        public Page(Template _template, DATA1 _data)
        {
            this.template = _template;
            this.data = _data;

            this.content = _template.ToString();

            Page.Render(this, _data, Page.Public_DataRender_WhilePage_TrGroup);
        }

        public Page(Template _template, DATA2 _data)
        {
            this.template = _template;
            this.data = _data;

            this.content = _template.ToString();

            Page.Render(this, _data, Page.Public_DataRender_WhilePage_TrGroup);
        }

        private Template template { get; set; }

        public List<Template.Table> tables { get; set; }

        private IDATA data { get; set; }

        private string content { get; set; }

        public List<Template.Table> Tables { get { return this.tables; } }

        public string Content { get { return this.content; } }

        public string ToString()
        {
            return this.content;
        }

        public void ContentRef(ref string _str)
        {
            this.content = _str;
        }

        public static void Render(Page _page, DATA1 _data, DataRender_WhilePage_TrGroupFunc _dataRender_WhilePage_TrGroupFunc)
        {
            _page.tables = new List<Template.Table>();

            Page.TemplateInit(_page, _page.template, _page.tables);

            OuterDataRender outerDataRender = new OuterDataRender();

            outerDataRender.DataRender(_page, m => { return new Pager(m.RecordsCount, m.TableSize); }, new DataRender_WhilePageFunc(_page.DataRender_WhilePage), _dataRender_WhilePage_TrGroupFunc);
        }

        public static void Render(Page _page, DATA2 _data, DataRender_WhilePage_TrGroupFunc _dataRender_WhilePage_TrGroupFunc)
        {
            _page.tables = new List<Template.Table>();

            Page.TemplateInit(_page, _page.template, _page.tables);

            OuterDataRender outerDataRender = new OuterDataRender();

            outerDataRender.DataRender(_page, m =>
            {

                var data2 = (DATA2)m;

                return new Pager(data2[m.TemplateTableIndex]);

            }, new DataRender_WhilePageFunc(_page.DataRender_WhilePage), _dataRender_WhilePage_TrGroupFunc);
        }

        public void DataRender_WhilePage(Template.Table _templateTable, int _tableIndex, int _trGroupIndex, Pager _pager, IDATA _data, DataRender_WhilePage_LoopFunc _dataRender_WhilePage_LoopFunc)
        {
            Template.TR templateTr = null;

            int trIndex = -1;

            var emptyTR = _templateTable.GTRs[_trGroupIndex].TRs.FirstOrDefault(tr => !tr.IsRendered);

            if (emptyTR != null)
            {
                trIndex = _templateTable.GTRs[_trGroupIndex].TRs.IndexOf(emptyTR);
            }
            else
            {
                _templateTable.GTRs[_trGroupIndex].TRs.Add(new Template.TR(_templateTable.GTRs[_trGroupIndex].TRs[0]));

                trIndex = _templateTable.GTRs[_trGroupIndex].TRs.Count - 1;
            }

            _templateTable.TLogs.Add(new Template.Table.TLog(_trGroupIndex, trIndex));
            //
            templateTr = _templateTable.GTRs[_trGroupIndex].TRs[trIndex];

            #region ADD ref IDATA

            templateTr.DATA = _data;

            #endregion

            for (int ii = _pager.PagerInfo.DefaultPageInfo.beginIndex, iiIndex = 0; ii <= _pager.PagerInfo.DefaultPageInfo.endIndex; ii++, iiIndex++)
            {
                // td s -> loop
                //
                _dataRender_WhilePage_LoopFunc(templateTr, _pager, _data, ii, iiIndex);
            }

            templateTr.Combine();
        }

        public void DataRender_WhilePage_Loop(Template.TR _templateTr, Pager _pager, IDATA _data, int _ii, int _index)
        {
            if (typeof(Page.DATA1) == _data.GetType())
            {
                this.DataRender_WhilePage_Loop_ByDATATYPE((Page.DATA1)_data, _pager, _templateTr, _ii, _index);
                return;
            }

            if (typeof(Page.DATA2) == _data.GetType())
            {
                this.DataRender_WhilePage_Loop_ByDATATYPE((Page.DATA2)_data, _pager, _templateTr, _ii, _index);
                return;
            }
        }

        public virtual void DataRender_WhilePage_Loop_ByDATATYPE(Page.DATA1 _data, Pager _pager, Template.TR _templateTr, int _ii, int _index)
        {
            var oneDataTd = (System.Data.DataRow)_data.DataSet.Tables[_data.TemplateTableIndex].Rows[_ii - 1];

            _templateTr = _templateTr + oneDataTd;
        }

        public virtual void DataRender_WhilePage_Loop_ByDATATYPE(Page.DATA2 _data, Pager _pager, Template.TR _templateTr, int _ii, int _index)
        {
            //int debug = 0;

            var oneDataTd = _data[_data.TemplateTableIndex][_pager.CurrentPage - 1].Items[_index];

            _templateTr = _templateTr + oneDataTd;
        }

        public static void TemplateInit(Page _page, Template _template, List<Template.Table> _tables)
        {
            string content = _template.ToString();

            string templateTag = Page.TemplateTag;

            string tableReplaceHolderTemplateTag = string.Format(Page.ReplaceHolderTemplateTag, "TABLE", "{0}");

            List<Template.BlockInfo> tableTemplateArray = Template.Blocks(ref content, templateTag, "Template_TABLE", "/Template_TABLE", _replaceHolderTemplateTag: tableReplaceHolderTemplateTag);

            _page.ContentRef(ref content);

            // ===

            int tableIndex = -1;

            foreach (var tableTemplateHolder in tableTemplateArray)
            {
                tableIndex++;

                // ===

                _tables.Add(new Template.Table(tableTemplateHolder, _template));

                Template.Table table = _tables[tableIndex];

                Page.TemplateInit_TemplateTableTrGroup(ref table, ref tableIndex, 0, 0, 0, tableTemplateHolder);
            }
        }

        public static void TemplateInit_TemplateTableTrGroup(ref Template.Table table, ref int tableIndex, int trGroupIndex, int trIndex, int tdIndex, Template.BlockInfo tableTemplateHolder)
        {
            table.GTRs = new Template.Block<Template.TR>.Group<Template.TR>();

            List<Template.BlockInfo> trGroupTemplateArray = Template.Blocks(tableTemplateHolder.Value, Page.TemplateTag, "Template_TRGroup", "/Template_TRGroup");

            #region None Template_TRGroup

            if (trGroupTemplateArray.Count == 0)
            {
                trGroupIndex = 0;

                Page.FillTRContainerTemplate(ref table, ref tableIndex, ref trGroupIndex, ref trIndex, ref tdIndex, tableTemplateHolder, ref Page.TemplateTag);

                #region GTR Pair <= Tr

                var pTRz = table.GTRs[trGroupIndex];

                var TRzFirst = pTRz.TRs[0];
                var TRzLast = pTRz.TRs[pTRz.TRs.Count - 1];

                pTRz.Pairs = pTRz.Pairs.Append(pTRz.TemplateHolder.ToPairs(TRzFirst.Container ?? TRzFirst.TemplateHolder, TRzLast.Container ?? TRzLast.TemplateHolder));

                #endregion

                // return => outside (for) continue
                //
                return;
            }

            #endregion

            trGroupIndex = -1;

            foreach (var trGroupTemplateHolder in trGroupTemplateArray)
            {
                trGroupIndex++;

                string templateSource = trGroupTemplateHolder.Value;

                List<Template.BlockInfo> innerTableTemplateArray = Template.Blocks(ref templateSource, Page.TemplateTag, "Template_TABLE", "/Template_TABLE");

                if (innerTableTemplateArray.Count == 0)
                {
                    #region None Table Template

                    table.GTRs[trGroupIndex] = new Template.Block<Template.TR>.TGroup<Template.TR>(trGroupTemplateHolder);

                    Page.FillTRContainerTemplate(ref table, ref tableIndex, ref trGroupIndex, ref trIndex, ref tdIndex, trGroupTemplateHolder, ref Page.TemplateTag);

                    #region GTR Pair <= Tr

                    var pTRz = table.GTRs[trGroupIndex];

                    var TRzFirst = pTRz.TRs[0];
                    var TRzLast = pTRz.TRs[pTRz.TRs.Count - 1];

                    pTRz.Pairs = pTRz.Pairs.Append(pTRz.TemplateHolder.ToPairs(TRzFirst.Container ?? TRzFirst.TemplateHolder, TRzLast.Container ?? TRzLast.TemplateHolder));

                    #endregion

                    #endregion
                }
                else
                {
                    //// for 1 Template_TABLE
                    ////
                    //foreach (var innerTableTemplateHolder in innerTableTemplateArray)
                    //{
                    //    table.GTRs[trGroupIndex] = new Template.Block<Template.TR>.TGroup<Template.TR>(trGroupTemplateHolder, innerTableTemplateHolder);

                    //    Template.Table innerTable = table.GTRs[trGroupIndex].InnerTable;

                    //    Page.TemplateInit_TemplateTableTrGroup(ref innerTable, ref tableIndex, 0, 0, 0, innerTableTemplateHolder);
                    //}

                    #region n || 1 Table s Template

                    table.GTRs[trGroupIndex] = new Template.Block<Template.TR>.TGroup<Template.TR>(trGroupTemplateHolder, innerTableTemplateArray);

                    for (int i = 0, iMax = table.GTRs[trGroupIndex].InnerTables.Count; i < iMax; i++)
                    {
                        Template.Table innerTable = table.GTRs[trGroupIndex].InnerTables[i];

                        Page.TemplateInit_TemplateTableTrGroup(ref innerTable, ref tableIndex, 0, 0, 0, innerTable.TemplateHolder);
                    }

                    #region GTR Pair <= Table s

                    var pTRz = table.GTRs[trGroupIndex];

                    var tableTemplateFirst = pTRz.InnerTables[0];
                    var tableTemplateLast = pTRz.InnerTables[pTRz.InnerTables.Count - 1];

                    pTRz.Pairs = pTRz.Pairs.Append(pTRz.TemplateHolder.ToPairs(tableTemplateFirst.TemplateHolder, tableTemplateLast.TemplateHolder));

                    #endregion

                    #endregion
                }
            }

            return;
        }

        public static void FillTRContainerTemplate(ref Template.Table table, ref int tableIndex, ref int trGroupIndex, ref int trIndex, ref int tdIndex, Template.BlockInfo sourceTemplateHolder, ref string templateTag)
        {
            #region new TR Container || not ( Default: new TR TemplateHolder )

            List<Template.BlockInfo> trContainerTemplateArray = Template.Blocks(sourceTemplateHolder.Value, templateTag, "Template_Container", "/Template_Container", _searchBeforeTag: "Template_TR");

            List<Template.BlockInfo> trTemplateArray = null;

            Template.BlockInfo trContainer = null;

            if (trContainerTemplateArray.Count >= 1)
            {
                trContainer = trContainerTemplateArray[0];

                trTemplateArray = Template.Blocks(trContainer.Value, templateTag, "Template_TR", "Template_TR");
            }
            else
            {
                trContainer = null;

                trTemplateArray = Template.Blocks(sourceTemplateHolder.Value, templateTag, "Template_TR", "Template_TR");
            }

            if (table.GTRs[trGroupIndex] == null)
            {
                table.GTRs[trGroupIndex] = new Template.Block<Template.TR>.TGroup<Template.TR>();
            }

            #region new TR s ( Container & TemplateHolder )

            trIndex = -1;

            foreach (var trTemplateHolder in trTemplateArray)
            {
                trIndex++;

                //if (trIndex == 0)
                //{
                //    int __debug__ = 0;
                //}

                if (trIndex == 0)
                {
                    trTemplateHolder.FillTemplateRef(table);
                    trContainer.FillTemplateRef(table);
                    //
                    // # DATA Item & Template.TagInfo #
                    //
                    // debug pause
                    //
                    //trTemplateHolder.FillTagInfos();
                    //trContainer.FillTagInfos();
                }

                var TRsList = table.GTRs[trGroupIndex].TRs;

                TRsList.Add(new Template.TR(trTemplateHolder, trContainer));

                var TRz = TRsList[TRsList.Count - 1];

                #region new TD Container || not ( Default: new TD TemplateHolder )

                List<Template.BlockInfo> tdContainerTemplateArray = Template.Blocks(trTemplateHolder.Value, templateTag, "Template_Container", "/Template_Container", _searchBeforeTag: "Template_TD");

                List<Template.BlockInfo> tdTemplateArray = null;

                Template.BlockInfo tdContainer = null;

                if (tdContainerTemplateArray.Count >= 1)
                {
                    tdContainer = tdContainerTemplateArray[0];

                    tdTemplateArray = Template.Blocks(tdContainer.Value, templateTag, "Template_TD", "Template_TD");
                }
                else
                {
                    tdContainer = null;

                    tdTemplateArray = Template.Blocks(trTemplateHolder.Value, templateTag, "Template_TD", "Template_TD");
                }

                #region new TD s ( Container & TemplateHolder )

                tdIndex = -1;

                foreach (var tdTemplateHolder in tdTemplateArray)
                {
                    tdIndex++;

                    if (tdIndex == 0)
                    {
                        tdTemplateHolder.FillTemplateRef(table);
                        tdContainer.FillTemplateRef(table);
                        //
                        // # DATA Item & Template.TagInfo #
                        //
                        tdTemplateHolder.FillTagInfos();
                        //
                        // debug pause
                        //
                        //tdContainer.FillTagInfos();
                    }

                    var TDsList = TRsList[trIndex].TDs;

                    TDsList.Add(new Template.TD(null, tdTemplateHolder, tdContainer));

                    var TDz = TDsList[TDsList.Count - 1];

                    #region TD Pair

                    TDz.Pairs = TDz.Container.ToPairs(tdTemplateHolder);

                    #endregion
                }

                #endregion

                #endregion

                #region TR Pair s

                var TDzFirst = TRz.TDs[0];
                var TDzLast = TRz.TDs[TRz.TDs.Count - 1];

                var pairs = TRz.TemplateHolder.ToPairs(TDzFirst.Container ?? TDzFirst.TemplateHolder, TDzLast.Container ?? TDzLast.TemplateHolder);

                TRz.Pairs = TRz.Pairs.Append(pairs);

                if (TRz.Container != null)
                {
                    TRz.Pairs = TRz.Pairs.Append(TRz.Container.ToPairs(TRz.TemplateHolder));
                }

                #endregion
            }

            #endregion

            #endregion
        }

        public class OuterDataRender
        {
            public void DataRender(Page _page, Func<IDATA, IPager> _pagerFunc, DataRender_WhilePageFunc _dataRender_WhilePageFunc, DataRender_WhilePage_TrGroupFunc _dataRender_WhilePage_TrGroupFunc)
            {
                string content = _page.template.ToString();

                for (int i = 0, iMax = _page.tables.Count; i < iMax; i++)
                {
                    _page.data.TemplateTableIndex = i;

                    Template.Table templateTable = _page.tables[i];

                    //if (i == 1)
                    //{
                    //    int debug = 0;
                    //}

                    //InnerDataRender innerDataRender = new InnerDataRender();

                    //innerDataRender.DataRender(_pagerFunc, templateTable, _page.data, i, 0, _dataReader_WhilePage_TrGroupFunc, _dataReader_WhilePageFunc, new DataRender_WhilePage_LoopFunc(_page.DataRender_WhilePage_Loop));

                    // ===

                    Page.InnerDataRender.DataRender2(_pagerFunc, templateTable, _page.data, i, 0, 0, _dataRender_WhilePage_TrGroupFunc, _dataRender_WhilePageFunc, new DataRender_WhilePage_LoopFunc(_page.DataRender_WhilePage_Loop));

                    templateTable.Combine();
                }

                _page.TableCombine();
            }
        }

        public class InnerDataRender
        {
            public static void DataRender2(Func<IDATA, IPager> _pagerFunc, Template.Table _templateTable, IDATA _data, int _tableIndex, int _zIndex, int _innerTableIndex, DataRender_WhilePage_TrGroupFunc _dataRender_WhilePage_TrGroupFunc, DataRender_WhilePageFunc _dataRender_WhilePageFunc, DataRender_WhilePage_LoopFunc _dataRender_WhilePage_LoopFunc)
            {
                Pager pager = (Pager)_pagerFunc(_data);

                while (pager.Next())
                {
                    //if (pager.CurrentPage == 3)
                    //{
                    //    int debug = 0;
                    //}

                    //if (pager.CurrentPage == 5)
                    //{
                    //    int debug = 0;
                    //}

                    //if (pager.CurrentPage == 6)
                    //{
                    //    int debug = 0;
                    //}

                    //if (pager.CurrentPage == 1)
                    //{
                    //    int debug = 0;
                    //}

                    //if (pager.CurrentPage == 2)
                    //{
                    //    int debug = 0;
                    //}

                    int trGroupIndex = 0;

                    //try
                    //{
                    trGroupIndex = _dataRender_WhilePage_TrGroupFunc(_templateTable, _tableIndex, _zIndex, _innerTableIndex, pager, _data);

                    //    // int trGroupIndex = _templateTable.PrepareTrGroup(((Page.DATA2)_data)[_tableIndex][_innerTableIndex]);

                    //}
                    //catch (Exception e)
                    //{
                    //    int __debug__ = 0;
                    //}

                    if (_templateTable.GTRs[trGroupIndex].TRs != null)
                    {
                        //try
                        //{
                        //    // data1 : 1p <=> n datarow
                        //    // data2 : 1p <=> 1 rowitems => n items
                        //    //
                        _dataRender_WhilePageFunc(_templateTable, _tableIndex, trGroupIndex, pager, _data, _dataRender_WhilePage_LoopFunc);
                        //    //
                        //    ////((DATA)_data).DataLayers.Add(new DATA.Layer() { ZIndex = _zIndex, Page = pager.CurrentPage, Begin = pager.PagerInfo.DefaultPageInfo.beginIndex, End = pager.PagerInfo.DefaultPageInfo.endIndex, InnerTableIndex = _innerTableIndex, TrGroupIndex = trGroupIndex, IsEndPoint = true });
                        //}
                        //catch (Exception e2)
                        //{
                        //    int __debug2__ = 0;
                        //}
                    }
                    else if (_templateTable.GTRs[trGroupIndex].InnerTables != null)
                    {
                        /* for DATA2 */

                        for (int i = 0, iMax = _templateTable.GTRs[trGroupIndex].InnerTables.Count; i < iMax; i++)
                        {
                            Template.Table innerTable = _templateTable.GTRs[trGroupIndex].InnerTables[i];

                            var innerDATA = ((DATA2)(_data))[_tableIndex][pager.CurrentPage - 1].InnerRowItems[i];

                            if (innerDATA == null)
                            {
                                throw new ArgumentException(string.Format("InnerTables DATA NULL [trGroupIndex:{0}] [tableIndex:{1}] [currentPage:{2}]", trGroupIndex, _tableIndex, pager.CurrentPage));
                            }

                            DATA2 innerDATA2 = new DATA2((DATA2)_data);

                            innerDATA2.Add(0, innerDATA);

                            //InnerDataRender innerDataRender = new InnerDataRender();

                            ////((DATA)_data).DataLayers.Add(new DATA.Layer() { ZIndex = _zIndex, Page = pager.CurrentPage, Begin = pager.PagerInfo.DefaultPageInfo.beginIndex, End = pager.PagerInfo.DefaultPageInfo.endIndex, InnerTableIndex = i, TrGroupIndex = trGroupIndex, IsEndPoint = false });
                            //
                            Page.InnerDataRender.DataRender2(_pagerFunc, innerTable, innerDATA2, _tableIndex, (_zIndex + 1), (i), _dataRender_WhilePage_TrGroupFunc, _dataRender_WhilePageFunc, _dataRender_WhilePage_LoopFunc);
                            //
                            // # 2018.06.07 # disabled
                            //
                            //_templateTable.TLogs.Add(new Template.Table.TLog(trGroupIndex, -1));
                            //
                        }
                        //
                        // # 2018.06.07 # enable
                        //
                        _templateTable.TLogs.Add(new Template.Table.TLog(trGroupIndex, -1));
                        //
                    }

                    //if (pager.CurrentPage == 3)
                    //{
                    //    int debug = 0;
                    //}
                }
            }
        }

        public class DATA1 : DATA
        {
            public DATA1() : base() { }

            public DATA1(DATA1 _data1)
                : base(_data1)
            {

            }

            public System.Data.DataSet DataSet { get; set; }
        }

        public class DATA2 : DATA
        {
            public DATA2()
                : base()
            {
                this.rDic = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<RowItems>>();
            }

            public DATA2(DATA2 _data2)
                : base(_data2)
            {
                this.rDic = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<RowItems>>();
            }

            private Dictionary<int, List<A4.DATA.Model.RowItems>> rDic { get; set; }

            public List<A4.DATA.Model.RowItems> this[int index]
            {
                get { return this.rDic.ContainsKey(index) ? this.rDic[index] : null; }

                set
                {

                    if (this.rDic.ContainsKey(index))
                    {
                        this.rDic[index] = value;
                    }
                    else
                    {
                        this.rDic.Add(index, value);
                    }
                }
            }

            public void Add(int _index, List<A4.DATA.Model.RowItems> _value)
            {
                this[_index] = _value;
            }

            public int Count { get { return this.rDic.Keys.Count; } }
        }

        public class DATA : IDATA
        {
            public DATA()
            {
                ////this.DataLayers = new List<DATA.ILayer>();
            }

            public DATA(DATA _data)
            {
                ////this.DataLayers = _data.DataLayers;
            }

            ////public List<DATA.ILayer> DataLayers { get; set; }
            public int RecordsCount { get; set; }
            public int TableSize { get; set; }
            public int TemplateTableIndex { get; set; }

            public class Layer : ILayer
            {
                public Layer()
                {

                }

                public Layer(int _zIndex, int _innerTableIndex, bool _isEndPoint = true)
                {
                    this.ZIndex = _zIndex;
                    this.InnerTableIndex = _innerTableIndex;
                    this.IsEndPoint = _isEndPoint;
                }

                public Layer(Layer _layer)
                {
                    this.ZIndex = _layer.ZIndex;
                    this.InnerTableIndex = _layer.InnerTableIndex;
                    this.IsEndPoint = _layer.IsEndPoint;
                }

                public int ZIndex { get; set; }
                public int InnerTableIndex { get; set; }
                public bool IsEndPoint { get; set; }
            }

            public interface ILayer
            {
                int ZIndex { get; set; }
                int InnerTableIndex { get; set; }
                bool IsEndPoint { get; set; }
            }
        }

        public interface IDATA
        {
            int RecordsCount { get; set; }
            int TableSize { get; set; }
            int TemplateTableIndex { get; set; }
        }

        public class Pager : A4.Tools.Pager
        {
            private List<A4.DATA.Model.RowItems> pagerSourceContainer = null;

            public Pager(List<A4.DATA.Model.RowItems> _data)
            {
                this.pagerSourceContainer = _data;

                int maxRecords = 0;
                int pageSize = 0;

                _data.ForEach(m =>
                {

                    if (m.InnerRowItems == null)
                    {
                        maxRecords += m.Items.Count;
                    }
                    else
                    {
                        m.InnerRowItems.AllCount(ref maxRecords);
                    }
                });

                if (maxRecords > 0)
                {
                    if (_data[0].InnerRowItems == null)
                    {
                        pageSize = _data[0].Items.Count;
                    }
                    else
                    {
                        _data[0].InnerRowItems.AllCount(ref pageSize);
                    }
                }

                this.MaxRecords = maxRecords;
                this.PageSize = pageSize;
                this.CurrentPage = 1;

                this.InitPagerInfo();
            }

            public Pager(int _maxRecords, int _pageSize, int _currentPage = 1)
                : base(_maxRecords, _pageSize, _currentPage)
            {

            }

            public override bool Next(int _nextPageSize = -1)
            {
                if (this.nextPage <= 0) this.nextPage = 1;

                if (this.nextPage <= this.MaxPage)
                {
                    // ===

                    if (this.pagerSourceContainer != null)
                    {
                        _nextPageSize = this.pagerSourceContainer[this.nextPage - 1].Items.Count;

                        if (_nextPageSize == 0)
                        {
                            // 2 layer count
                            //
                            if (this.pagerSourceContainer[this.nextPage - 1].InnerRowItems != null)
                            {
                                foreach (var innerRowItems in this.pagerSourceContainer[this.nextPage - 1].InnerRowItems.Values)
                                {
                                    innerRowItems.ForEach(row =>
                                    {
                                        _nextPageSize += row.Items.Count;
                                    });
                                }
                            }
                        }
                    }

                    if (_nextPageSize != -1)
                    {
                        this.Reset(_nextPageSize);
                    }

                    // ===

                    this.Page(this.nextPage);

                    this.nextPage++;

                    return true;
                }
                else
                {
                    this.nextPage = -1;

                    return false;
                }
            }
        }

        public class RenderModel
        {
            public string TagName { get; set; }
            public object Value { get; set; }
        }
    }

}
