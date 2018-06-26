using A4.DATA.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace A4
{
    public static class TemplatePageExtensions
    {
        public static bool IsEmpty(this Dictionary<string, List<Template.TagInfo>> _tagInfos)
        {
            if (_tagInfos == null) return true;

            if (_tagInfos.Count == 0) return true;

            return false;
        }

        public static bool IsCurrentExpression(this string _expression, object _dataItem)
        {
            // $$ColumnName == "string"

            return (bool)TemplatePageExtensions.ExpressionEval(_dataItem, _expression);
        }

        public static object ExpressionEval(object _obj, string _expressionString, string _preFixChars = null)
        {
            //_preFixChars = _preFixChars ?? Template.TagTypeComboPreChars;

            //_expressionString = _expressionString.Replace(_preFixChars, "@.");

            //var reg = new TypeRegistry();

            //reg.RegisterSymbol("@", _obj);

            //var expression = new CompiledExpression(_expressionString);

            //expression.TypeRegistry = reg;

            //return expression.Eval();

            // ExpressionEvaluator.dll

            return null;
        }

        public static string ItemExtendName(this IEnumerable<CustomAttributeData> _customAttributeDatas)
        {
            var attr = _customAttributeDatas.FirstOrDefault(m => m.AttributeType == typeof(A4.DATA.Model.ItemExtendNameAttribute));

            if (attr == null) return null;

            return attr.ConstructorArguments[0].Value.ToString();
        }

        public static List<Page.RenderModel> ToRenderModels<T>(this T _dataItem, bool _degreeInnerItemExtend = true, int _groupItemsIndex = -1) where T : IItemExtend
        {
            Type dataType = _dataItem.GetType();

            MemberInfo[] memberInfos = dataType.GetMembers();

            var simpleColumnInfos = memberInfos.Where(m => (m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field));

            BindingFlags bindingFlags = BindingFlags.Default;

            Dictionary<MemberTypes, BindingFlags> memberTypeNBindingFlag = new Dictionary<MemberTypes, BindingFlags>() {
            
                { MemberTypes.Property, BindingFlags.GetProperty },
                { MemberTypes.Field, BindingFlags.GetField }
            };

            // ===

            string itemExtendParamsFullName = "";

            IEnumerable<MemberInfo> defaultInfos = null;

            IEnumerable<MemberInfo> itemExtendParamsInfo = null;

            if (_dataItem.ItemExtendParams != null)
            {
                itemExtendParamsFullName = _dataItem.ItemExtendParams.GetType().ToString();

                itemExtendParamsInfo = simpleColumnInfos.Where(m => m.ToString().StartsWith(itemExtendParamsFullName.PadRight(1)));

                defaultInfos = simpleColumnInfos.Where(m => !m.ToString().StartsWith(itemExtendParamsFullName.PadRight(1)));
            }
            else
            {
                defaultInfos = simpleColumnInfos;
            }

            object tryObj = null;

            List<Page.RenderModel> renderModels = new List<Page.RenderModel>();

            string itemReplaceHolderName = "";

            List<Page.RenderModel> extendRenderModels = null;

            if (_degreeInnerItemExtend && _dataItem.GroupItems != null)
            {
                string groupItemsFullName = _dataItem.GroupItems.GetType().ToString();

                var groupItemsInfo = simpleColumnInfos.Where(m => m.ToString().StartsWith(groupItemsFullName.PadRight(1)));

                foreach (var item in groupItemsInfo)
                {
                    tryObj = dataType.InvokeMember(item.Name, memberTypeNBindingFlag[item.MemberType], null, _dataItem, new Object[] { });

                    if (tryObj != null)
                    {
                        var tryObj2 = (A4.DATA.Model.Rnd<T>)tryObj;

                        int columnIndex = -1;

                        foreach (var item2 in tryObj2.BindingColumns)
                        {
                            columnIndex++;

                            extendRenderModels = item2.ToRenderModels<T>(_degreeInnerItemExtend: false, _groupItemsIndex: columnIndex);

                            renderModels.Append(extendRenderModels);
                        }
                    }

                    break;
                }
            }

            foreach (var item in defaultInfos)
            {
                itemReplaceHolderName = item.CustomAttributes.ItemExtendName();

                itemReplaceHolderName = itemReplaceHolderName ?? item.Name;

                if (renderModels.FirstOrDefault(m => m.TagName == itemReplaceHolderName) != null) continue;

                tryObj = dataType.InvokeMember(item.Name, memberTypeNBindingFlag[item.MemberType], null, _dataItem, new Object[] { });

                if (_groupItemsIndex != -1)
                {
                    itemReplaceHolderName = string.Format("{0}{1}{2}", itemReplaceHolderName, "$", _groupItemsIndex);
                }

                renderModels.Add(new Page.RenderModel() { TagName = itemReplaceHolderName, Value = tryObj });
            }

            if (_degreeInnerItemExtend && itemExtendParamsInfo != null)
            {
                foreach (var item in itemExtendParamsInfo)
                {
                    tryObj = dataType.InvokeMember(item.Name, memberTypeNBindingFlag[item.MemberType], null, _dataItem, new Object[] { });

                    if (tryObj != null)
                    {
                        extendRenderModels = ((ItemExtendParams)tryObj).ConvertToRenderModels();

                        renderModels = renderModels.Append(extendRenderModels);
                    }

                    break;
                }
            }

            return renderModels;
        }

        public static void TemplateRender(this List<Page.RenderModel> _renderModels, Template.BlockInfo _blockInfo)
        {
            string content = _blockInfo.Value;

            _renderModels.TemplateRender(ref content);

            _blockInfo.Value = content;
        }

        public static void TemplateRender(this List<Page.RenderModel> _renderModels, Template.TD _td)
        {
            string content = _td.Content;

            _renderModels.TemplateRender(ref content);

            _td.Content = content;
        }

        public static void TemplateRender(this List<Page.RenderModel> _renderModels, ref string _content)
        {
            string str = "";

            foreach (var item in _renderModels)
            {
                str = item.Value == null ? "" : item.Value.ToString();

                _content = _content.Replace(string.Format("{0}{1}", Template.TagTypeComboPreChars, item.TagName), str);
            }
        }

        public static Dictionary<string, List<Template.TagInfo>> Filter(this Dictionary<string, List<Template.TagInfo>> _tagInfosDic, object _dataItem, A4.DATA.Model.IItemExtend _dataExtendItem)
        {
            List<Template.TagInfo> tmp = null;

            foreach (string item in _tagInfosDic.Keys)
            {
                tmp = _tagInfosDic[item];

                tmp.Filter(_dataItem, _dataExtendItem);
            }

            return _tagInfosDic;
        }

        public static List<Template.TagInfo> Filter(this List<Template.TagInfo> _tagInfos, object _dataItem, A4.DATA.Model.IItemExtend _dataExtendItem)
        {
            List<Template.TagInfo> targetTagInfos = new List<Template.TagInfo>();

            foreach (var item in _tagInfos)
            {
                if (item.IsTagType(Template.TagType.Fixed))
                {
                    if (!targetTagInfos.Contains(item))
                    {
                        targetTagInfos.Add(item);
                    }

                    continue;
                }

                if (item.IsTagType(Template.TagType.ColumnValue))
                {
                    if (item.ExpressionItems[Template.TagType.ColumnValue].IsCurrentExpression(_dataItem))
                    {
                        if (!targetTagInfos.Contains(item))
                        {
                            targetTagInfos.Add(item);
                        }

                        continue;
                    }
                }

                if (item.IsTagType(Template.TagType.Combo))
                {
                    if (item.ExpressionItems[Template.TagType.Combo] == _dataExtendItem.GroupItems.Expression)
                    {
                        if (!targetTagInfos.Contains(item))
                        {
                            item.GroupItems = _dataExtendItem.GroupItems;

                            targetTagInfos.Add(item);
                        }

                        continue;
                    }
                }
            }

            return targetTagInfos;
        }

        public static bool IsTagType(this Template.TagInfo _tagInfo, Template.TagType _tagType)
        {
            return _tagInfo.ExpressionItems.ContainsKey(_tagType);
        }

        public static void FillTemplateRef(this Template.BlockInfo _blockInfo, Template.Table _table)
        {
            if (_blockInfo == null) return;

            _blockInfo.Template = _table.TemplateHolder.Template;
        }

        public static DataRow FirstOrDefault(this DataRowCollection _dataRows, Func<DataRow, bool> _predicateFunc)
        {
            for (int i = 0, iMax = _dataRows.Count; i < iMax; i++)
            {
                if (_predicateFunc(_dataRows[i])) return _dataRows[i];
            }

            return null;
        }

        public static List<DataRow> Select(this DataRowCollection _dataRows, Func<DataRow, bool> _predicateFunc)
        {
            List<DataRow> tmp = new List<DataRow>();

            for (int i = 0, iMax = _dataRows.Count; i < iMax; i++)
            {
                if (_predicateFunc(_dataRows[i]))
                {

                    tmp.Add(_dataRows[i]);
                }
            }

            return tmp.Count == 0 ? null : tmp;
        }

        public static void FillTagInfos(this Template.BlockInfo _blockInfo)
        {
            if (_blockInfo == null) return;

            Template.BlockInfo tmp = null;

            Template.BlockInfo tag = null;

            List<Template.BlockInfo> tags = null;

            string tmp1 = "";
            string tmp2 = "";

            Template.BlockInfo[] FirstNLast = new Template.BlockInfo[] { null, null };

            // ===

            #region search atag Expression s

            tmp = Template.BlockFirst(_blockInfo.Value, "<!-- {0} -->", "Template_$Tag", "/Template_$Tag");

            if (tmp == null)
            {
                // only 1 $Tag Group ( none: <!-- Template_$Tag --> ... <!-- /Template_$Tag --> )

                tag = _blockInfo;
            }
            else
            {
                // n || 1 $Tag Groups

                tag = tmp;
            }

            // FirstOrDefault => n || 1 $Tag Groups => atagExpression s
            //
            tags = Template.Blocks(tag.Value, "{0}", "Template_$Tag", "-->",

                _foreachATagPredicate: (int aTagInex, ref string content) =>
                {

                    if (aTagInex == 0) return true;

                    if (content.Substring(aTagInex - 1, 1) == "/") return false;

                    return true;
                });

            #endregion

            // ===

            #region End Point : none $Tag

            if (tags.Count == 0)
            {

                if (tmp == null)
                {
                    // only 1 $Tag Group ( none: <!-- Template_$Tag --> ... <!-- /Template_$Tag --> )

                    return;
                }
                else
                {
                    /*
                 
                     <!-- Template_$Tag -->
                     ...
                     <!-- /Template_$Tag -->

                    */

                    FirstNLast[0] = tag;
                    FirstNLast[1] = tag;

                    #region # [abc] -> [a] [c]

                    tmp1 = _blockInfo.Value.Substring(0, FirstNLast[0].OuterAIndex + 1 - 1);
                    tmp2 = _blockInfo.Value.Substring(FirstNLast[1].OuterBIndex + 1);

                    #endregion

                    _blockInfo.Value = tmp1 + tag.Value + tmp2;
                }

                return;
            }

            #endregion

            // ===

            #region new atagsExpression <= atag Expression s

            List<string> atagsExpression = new List<string>();

            string _tmp_ = "";

            foreach (var item in tags)
            {
                _tmp_ = item.Value.Trim();

                if (_tmp_.StartsWith(":"))
                {
                    _tmp_ = _tmp_.TrimStart(new char[] { ':' }).Trim();

                    atagsExpression.Add(_tmp_);
                }
                else
                {
                    atagsExpression.Add(null);
                }
            }

            #endregion

            #region new taginfos <= taginfo <= foreach atag Expression

            List<Template.TagInfo> taginfos = new List<Template.TagInfo>();

            string _tag_ = "";

            int atagsExpressionIndex = -1;

            Template.BlockInfo innerTag = null;

            foreach (var item in atagsExpression)
            {
                atagsExpressionIndex++;

                if (item == null)
                {
                    _tag_ = string.Format("Template_$Tag{0}{1}", "", "");
                }
                else
                {
                    _tag_ = string.Format("Template_$Tag{0}{1}", ":".PadRight(2), item);
                }

                innerTag = Template.BlockFirst(tag.Value, "<!-- {0} -->", _tag_, "/Template_$Tag");

                if (innerTag == null) continue;

                if (atagsExpressionIndex == 0)
                {
                    FirstNLast[0] = innerTag;
                }
                else
                {
                    FirstNLast[1] = innerTag;
                }

                // Top Template Ref
                //
                innerTag.Template = _blockInfo.Template;

                Template.TagInfo tagInfo = new Template.TagInfo();

                tagInfo.Expression = item;
                // tagInfo.TagTypes = null;
                tagInfo.TemplateHolder = innerTag.Value;

                //tagInfo.Rnds = TemplatePageExtensions.ToRnds(tagInfo.TemplateHolder);

                // # DATA Item & Template.TagInfo #
                //
                // ? ! == List<RowItems> => List<Item> => Item => Rnd (GroupItems):
                //tagInfo.BoundColumns = null;

                tagInfo.FillTagTypes();
                tagInfo.FillRnds(innerTag);

                taginfos.Add(tagInfo);
            }

            #endregion

            int ___debug___ = 0;

            // ===

            #region this <= new Tags (string, taginfos) <= ReplaceHolderTemplateTag, taginfos

            // $Tag Group ( only 1 / n || 1 ) => ReplaceHolderTemplateTag
            //
            string tagInfosReplaceHolderTemplateTag = string.Format(Page.ReplaceHolderTemplateTag, "TagInfos", Guid.NewGuid().ToString());

            // ===

            if (_blockInfo.Tags == null) _blockInfo.Tags = new Dictionary<string, List<Template.TagInfo>>();

            _blockInfo.Tags.Add(tagInfosReplaceHolderTemplateTag, taginfos);

            // ===

            FirstNLast[1] = FirstNLast[1] ?? FirstNLast[0];

            #region # [abc] -> [a] [c]

            tmp1 = _blockInfo.Value.Substring(0, FirstNLast[0].OuterAIndex + 1 - 1);
            tmp2 = _blockInfo.Value.Substring(FirstNLast[1].OuterBIndex + 1);

            #endregion

            _blockInfo.Value = tmp1 + tagInfosReplaceHolderTemplateTag + tmp2;

            #endregion

            // ===

            _blockInfo.FillTagInfos();
        }

        public static bool OutOfChas(this string _str, string _strs)
        {
            var targetChars = _strs.ToCharArray();

            var tmp = _str.ToCharArray().FirstOrDefault(m =>
            {

                return (Array.IndexOf<char>(targetChars, m) == -1);
            });

            return (tmp != '\0');
        }

        public static void FillRnds(this Template.TagInfo _tagInfo, Template.BlockInfo _blockInfo)
        {
            _tagInfo.TemplateRnds = new List<Template.TagInfo.Rnd>();

            string tagInfoTemplateHolder = "";
            string templateContent = "";

            foreach (var item in _tagInfo.TagTypes)
            {
                var exp = _tagInfo.ExpressionItems[item];

                switch (item)
                {
                    //case Template.TagType.Fixed:
                    //    { 

                    //    }
                    //    break;

                    // "$$KeyWord"
                    case Template.TagType.Combo:
                        {
                            #region Plugin -> *.Rnd ($$KeyWord.Rnd)

                            tagInfoTemplateHolder = _tagInfo.TemplateHolder;
                            templateContent = _blockInfo.Template.ToString();

                            _tagInfo.TemplateRnds = Template.RndTags(ref tagInfoTemplateHolder, ref templateContent, string.Format("{0}.Rnd", exp), Guid.NewGuid().ToString(), () =>
                            {
                                return Guid.NewGuid().ToString();
                            });

                            _tagInfo.TemplateHolder = tagInfoTemplateHolder;
                            _blockInfo.Template.Content(ref templateContent);

                            #endregion
                        }
                        break;

                    // "$$ColumnType == string"
                    //case Template.TagType.ColumnValue:
                    //    { 

                    //    }
                    //    break;
                }
            }
        }

        public static void FillTagTypes(this Template.TagInfo _tagInfo)
        {
            if (String.IsNullOrEmpty(_tagInfo.Expression)) return;

            _tagInfo.TagTypes = new List<Template.TagType>();

            _tagInfo.ExpressionItems = new Dictionary<Template.TagType, string>();

            string[] tagExpressions = _tagInfo.Expression.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            Template.TagType tagType = Template.TagType.None;

            foreach (var item in tagExpressions)
            {
                //item = item.Trim();

                tagType = Template.TagType.None;

                if (item.ToLower() == "fixed")
                {
                    tagType = Template.TagType.Fixed;
                }
                if (item.ToLower() == "default")
                {
                    tagType = Template.TagType.Default;
                }
                else if (item.StartsWith(Template.TagTypeComboPreChars))
                {
                    if (item.OutOfChas(Template.tag_chars))
                    {
                        // contains == / != / ...
                        //
                        tagType = Template.TagType.ColumnValue;
                    }
                    else
                    {
                        tagType = Template.TagType.Combo;
                    }
                }

                if (tagType == Template.TagType.None) continue;

                if (_tagInfo.TagTypes.Contains(tagType)) continue;

                _tagInfo.TagTypes.Add(tagType);

                _tagInfo.ExpressionItems.Add(tagType, item.Trim());
            }
        }

        //public static List<Page.RenderModel> ToRenderModels(this Item _item)
        //{
        //    List<Page.RenderModel> renderModels = new System.Collections.Generic.List<Page.RenderModel>();

        //    renderModels.Add(new Page.RenderModel() { TagName = "ColumnName", Value = _item.ColumnName });
        //    renderModels.Add(new Page.RenderModel() { TagName = "LabelName", Value = _item.Name });

        //    List<Page.RenderModel> extendRenderModels = _item.ItemExtendParams.ConvertToRenderModels();

        //    renderModels = renderModels.Mixed(extendRenderModels);

        //    return renderModels;
        //}

        public static List<Page.RenderModel> Append(this List<Page.RenderModel> _renderModels, List<Page.RenderModel> _extendRenderModels)
        {
            if (_extendRenderModels == null) return _renderModels;

            _extendRenderModels.ForEach(m =>
            {

                if (_renderModels.FirstOrDefault(n => n.TagName == m.TagName) == null)
                {
                    _renderModels.Add(m);
                }
            });

            return _renderModels;
        }

        public static List<Page.DATA.Layer> dataLayer_TableTemplate_cache = null;

        public static int PrepareTrGroup(this Template.Table _table, object _data)
        {
            DataLayer dataLayer = null;

            List<Page.DATA.Layer> dataLayer_TableTemplate1;

            // ===

            if (TemplatePageExtensions.dataLayer_TableTemplate_cache == null)
            {
                dataLayer = new DataLayer(_table, _table.GetType(), new string[] { "InnerTables" });

                List<Page.DATA.Layer> dataLayer_TableTemplate = dataLayer.ToDataLayer();

                dataLayer_TableTemplate1 = dataLayer_TableTemplate.ConvertToDefaultDataLayer();

                TemplatePageExtensions.dataLayer_TableTemplate_cache = dataLayer_TableTemplate1;
            }
            else
            {
                dataLayer_TableTemplate1 = TemplatePageExtensions.dataLayer_TableTemplate_cache;
            }

            // ===

            dataLayer = new DataLayer(_data, _data.GetType(), new string[] { "InnerRowItems" });

            List<Page.DATA.Layer> dataLayer_SourceData = dataLayer.ToDataLayer();

            List<Page.DATA.Layer> dataLayer_TableTemplate2 = dataLayer_SourceData.ConvertToTableTemplateDataLayer();

            // ===

            int beginIndex = 0;
            int endIndex = 0;
            int prepareDataLayerContextFuncIndex = -1;

            dataLayer_TableTemplate1.IndexOf(dataLayer_TableTemplate2, ref beginIndex, ref endIndex, ref prepareDataLayerContextFuncIndex);

            if (beginIndex == -1 || endIndex == -1) return -1;

            if (prepareDataLayerContextFuncIndex == 0)
            {
                return dataLayer_TableTemplate1[beginIndex - 1].InnerTableIndex;
            }
            else
            {
                return dataLayer_TableTemplate1[beginIndex].InnerTableIndex;
            }
        }

        //public static List<Page.DATA.ILayer> ToDataLayer(this Template.Table _table)
        //{
        //    //List<Page.DATA.ILayer> dataLayer = new System.Collections.Generic.List<Page.DATA.ILayer>();

        //    //_table.ToDataLayer(ref dataLayer, 0);

        //    //return dataLayer;

        //    DataLayer dataLayer = new DataLayer(_table, _table.GetType(), new string[] { "InnerTables" });

        //    return dataLayer.ToDataLayer();
        //}

        //private static void ToDataLayer(this Template.Table _table, ref List<Page.DATA.ILayer> _dataLayer, int _zIndex)
        //{
        //    for (int i = 0, iMax = _table.GTRs.Count; i < iMax; i++)
        //    {
        //        if (_table.GTRs[i].InnerTables == null)
        //        {
        //            _dataLayer.Add(new Page.DATA.Layer()
        //            {
        //                ZIndex = _zIndex,
        //                InnerTableIndex = 0,
        //                //TrGroupIndex = i,
        //                IsEndPoint = true
        //            });
        //        }
        //        else
        //        {
        //            for (int ii = 0, iiMax = _table.GTRs[i].InnerTables.Count; ii < iiMax; ii++)
        //            {
        //                Template.Table table = _table.GTRs[i].InnerTables[ii];

        //                _dataLayer.Add(new Page.DATA.Layer()
        //                {
        //                    ZIndex = _zIndex,
        //                    InnerTableIndex = ii,
        //                    //TrGroupIndex = i,
        //                    IsEndPoint = false
        //                });

        //                table.ToDataLayer(ref _dataLayer, (_zIndex + 1));
        //            }
        //        }
        //    }
        //}

        //public static List<Page.DATA.Layer> ToDataLayer(this object _data, string[] _innerNodesFilter = null)
        //{
        //    //List<Page.DATA.Layer> dataLayer = new System.Collections.Generic.List<Page.DATA.Layer>();

        //    //_data.ToDataLayer(ref dataLayer, 0);

        //    //return dataLayer;

        //    DataLayer dataLayer = new DataLayer(_data, _data.GetType(), _innerNodesFilter);

        //    return dataLayer.ToDataLayer();
        //}

        // debug
        //public static List<Page.DATA.ILayer> ToDataLayer(this List<A4.DATA.Model.RowItems> _data, string[] _innerNodesFilter = null)
        //{
        //    List<Page.DATA.ILayer> dataLayer = new System.Collections.Generic.List<Page.DATA.ILayer>();

        //    _data.ToDataLayer(ref dataLayer, 0, 0);

        //    return dataLayer;

        //    //DataLayer dataLayer = new DataLayer(_data, _data.GetType(), _innerNodesFilter);

        //    //return dataLayer.ToDataLayer();
        //}

        // debug
        //private static void ToDataLayer(this List<A4.DATA.Model.RowItems> _data, ref List<Page.DATA.ILayer> _dataLayer, int _zIndex, int _innerTableIndex)
        //{
        //    if (_dataLayer.Count == 0)
        //    {
        //        int debug = 0;
        //    }

        //    for (int i = 0, iMax = _data.Count; i < iMax; i++)
        //    {
        //        if (_data[i].InnerRowItems == null)
        //        {
        //            _dataLayer.Add(new Page.DATA.Layer(_zIndex, i, true));
        //        }
        //        else
        //        {
        //            for (int ii = 0, iiMax = _data[i].InnerRowItems.Keys.Count; ii < iiMax; ii++)
        //            {
        //                List<A4.DATA.Model.RowItems> data = _data[i].InnerRowItems[ii];

        //                _dataLayer.Add(new Page.DATA.Layer(_zIndex, ii, false));

        //                data.ToDataLayer(ref _dataLayer, (_zIndex + 1), ii);
        //            }
        //        }
        //    }
        //}

        public static bool IsSame(this Page.DATA.Layer _sourceLayer, Page.DATA.Layer _targetLayer)
        {
            return (_sourceLayer.ZIndex == _targetLayer.ZIndex
                    &&
                    _sourceLayer.InnerTableIndex == _targetLayer.InnerTableIndex
                    &&
                    _sourceLayer.IsEndPoint == _targetLayer.IsEndPoint);
        }

        //public static bool IsSame(this Page.DATA.LayerRender _sourceLayer, Page.DATA.LayerRender _targetLayer)
        //{
        //    return (_sourceLayer.ZIndex == _targetLayer.ZIndex
        //            &&
        //            _sourceLayer.InnerTableIndex == _targetLayer.InnerTableIndex
        //            &&
        //            _sourceLayer.IsEndPoint == _targetLayer.IsEndPoint
        //            &&
        //            _sourceLayer.IsRendered == _targetLayer.IsRendered);
        //}

        public static Page.DATA.Layer FirstOrDefault(this List<Page.DATA.Layer> _sourceDataLayer, Func<Page.DATA.Layer, bool> _predicate, int _beginIndex = 0, int _endIndex = -1)
        {
            if (_endIndex > (_sourceDataLayer.Count - 1)
                ||
                _endIndex < 0)
            {
                _endIndex = _sourceDataLayer.Count - 1;
            }

            if (_beginIndex < 0) _beginIndex = 0;

            if (_beginIndex > _endIndex) _beginIndex = _endIndex;

            if (_beginIndex == _endIndex && _endIndex == 0) _endIndex = _sourceDataLayer.Count - 1;

            for (int i = _beginIndex; i <= _endIndex; i++)
            {
                if (_predicate(_sourceDataLayer[i]))
                {
                    return _sourceDataLayer[i];
                }
            }

            return null;
        }

        public static void IndexOf(this List<Page.DATA.Layer> _sourceDataLayer, List<Page.DATA.Layer> _targetDataLayer, ref int _beginIndex, ref int _endIndex, ref int _prepareDataLayerContextFuncIndex)
        {
            Func<List<Page.DATA.Layer>, List<Page.DATA.Layer>, int, int, bool> m1 = (sourceDataLayer, targetDataLayer, i, ii) =>
            {
                var m0 = sourceDataLayer[i + ii];

                return (m0.IsEndPoint == targetDataLayer[ii].IsEndPoint
                            &&
                            m0.InnerTableIndex == targetDataLayer[ii].InnerTableIndex
                            &&
                            m0.ZIndex == targetDataLayer[ii].ZIndex);
            };

            Func<List<Page.DATA.Layer>, List<Page.DATA.Layer>, int, int, bool> m2 = (sourceDataLayer, targetDataLayer, i, ii) =>
            {
                // 0 t n - 1 t n+1 // 0 t n - 1 f n+1

                var m0 = sourceDataLayer[i + ii];

                return (
                            ((m0.IsEndPoint && targetDataLayer[ii].IsEndPoint)
                            ||
                            (m0.IsEndPoint && (!targetDataLayer[ii].IsEndPoint)))

                            && m0.ZIndex == (targetDataLayer[ii].ZIndex + 1)
                       );
            };

            _sourceDataLayer.IndexOf(_targetDataLayer, ref _beginIndex, ref _endIndex, ref _prepareDataLayerContextFuncIndex, new System.Func<List<Page.DATA.Layer>, List<Page.DATA.Layer>, int, int, bool>[] { 
            
                m1, m2
            });
        }

        public static void IndexOf(this List<Page.DATA.Layer> _sourceDataLayer, List<Page.DATA.Layer> _targetDataLayer, ref int _beginIndex, ref int _endIndex, ref int _prepareDataLayerContextFuncIndex, Func<List<Page.DATA.Layer>, List<Page.DATA.Layer>, int, int, bool>[] _prepareDataLayerContextFuncs)
        {
            _beginIndex = -1;
            _endIndex = -1;

            if (_sourceDataLayer.Count == 1 && _targetDataLayer.Count == 1)
            {
                //if (_sourceDataLayer[0].IsSame(_targetDataLayer[0]))
                //{
                //    _beginIndex = 0;
                //    _endIndex = 0;
                //}

                if (_sourceDataLayer[0].InnerTableIndex == _targetDataLayer[0].InnerTableIndex
                    &&
                    _sourceDataLayer[0].IsEndPoint == _targetDataLayer[0].IsEndPoint)
                {
                    _beginIndex = 0;
                    _endIndex = 0;
                }

                return;
            }

            int prepareDataLayerContextFuncMax = _prepareDataLayerContextFuncs.Length;

            bool isMatch = false;

            int ii = 0;
            int iiMax = 0;

            foreach (var _prepareDataLayerContextFunc in _prepareDataLayerContextFuncs)
            {
                _prepareDataLayerContextFuncIndex++;

                for (int i = 0, iMax = _sourceDataLayer.Count - 1; i <= iMax; i++)
                {
                    isMatch = false;

                    ii = 0;
                    iiMax = _targetDataLayer.Count - 1;

                    for (; ii <= iiMax; )
                    {
                        if (ii == 0) _beginIndex = i;

                        if (!_prepareDataLayerContextFunc(_sourceDataLayer, _targetDataLayer, i, ii))
                        {
                            break;
                        }
                        else
                        {
                            if ((ii == iiMax)
                                ||
                                (i == iMax))
                            {
                                _endIndex = i + ii;

                                isMatch = true;

                                break;
                            }
                        }

                        ii++;

                        if ((_prepareDataLayerContextFuncIndex + 1) >= prepareDataLayerContextFuncMax
                            ||
                            isMatch) break;
                    }

                    if (isMatch) break;
                }

                if (isMatch) break;
            }
        }

        public static List<Page.DATA.Layer> Clone(this List<Page.DATA.Layer> _sourceDataLayer)
        {
            var tmp = new List<Page.DATA.Layer>();

            _sourceDataLayer.ForEach(m =>
            {

                tmp.Add(new Page.DATA.Layer(m));
            });

            return tmp;
        }

        // (data layer) ~> (table data layer)
        public static List<Page.DATA.Layer> ConvertToTableTemplateDataLayer(this List<Page.DATA.Layer> _sourceDataLayer)
        {
            var sourceDataLayer = _sourceDataLayer.Clone();

            sourceDataLayer.FixItemSort((_l_a, _l_b, _itemIndex, _layerIndex) =>
            {
                _l_a.InnerTableIndex = -1;
                _l_b.InnerTableIndex = -1;
            });

            List<Page.DATA.Layer> tmp = sourceDataLayer.Where(m => m.InnerTableIndex != -1).ToList();

            Page.DATA.Layer newLayer = null;

            int iMax = tmp.Count;

            for (int i = 0; i < iMax; i++)
            {
                newLayer = new Page.DATA.Layer(tmp[i].ZIndex, tmp[i].InnerTableIndex);

                if (tmp[i].IsEndPoint) continue;

                tmp.Insert((i + 1), newLayer);

                i++;
                iMax++;
            }

            return tmp;
        }

        public static List<Page.DATA.Layer> ConvertToDefaultDataLayer(this List<Page.DATA.Layer> _sourceDataLayer)
        {
            var sourceDataLayer = _sourceDataLayer.Clone();

            // [(table template) data layer] ~> [(data) data layer] => level key -- (ZIndex)
            //
            sourceDataLayer.FixLevelKey();

            // [(table template) data layer] ~> [(data) data layer] => item sort (InnerTableIndex)
            //
            sourceDataLayer.FixItemSort();

            return sourceDataLayer;
        }

        // (1) (table data layer) ~> (data layer) => level key --
        private static void FixLevelKey(this List<Page.DATA.Layer> _sourceDataLayer, int _index = 0)
        {
            if (_sourceDataLayer.Count == 0) return;

            if (_sourceDataLayer[_index].IsEndPoint)
            {
                _sourceDataLayer.ReadDataLayerEndNodesTPoint(_index, _sourceDataLayer.Count - 1);
            }
            else
            {
                _sourceDataLayer.ReadDataLayerEndNodesFPoint(_index, _sourceDataLayer.Count - 1);
            }
        }

        // (2) (table data layer) ~> (data layer) => item sort
        private static void FixItemSort(this List<Page.DATA.Layer> _sourceDataLayer)
        {
            _sourceDataLayer.FixItemSort((_l_a, _l_b, _itemIndex, _layerIndex) =>
            {

                _l_a.InnerTableIndex = _itemIndex;
                _l_b.InnerTableIndex = _itemIndex;
            });
        }

        // (2) (table data layer) ~> (data layer) => item sort
        private static void FixItemSort(this List<Page.DATA.Layer> _sourceDataLayer, Action<Page.DATA.Layer, Page.DATA.Layer, int, int> _actionFunc)
        {
            // ===

            int itemIndex = -1;
            int innerIndex = -1;
            int zIndex = 0;

            // ===

            int layerIndex = -1;

            Page.DATA.Layer l_a = null;
            Page.DATA.Layer l_b = null;

            Page.DATA.Layer tmp = null;

            // ===

            int ii = 0;

            //_sourceDataLayer[]

            while (true)
            {
                innerIndex = -1;

                while (true)
                {
                    innerIndex++;

                    //tmp = null;

                    if (tmp == null)
                    {
                        tmp = _sourceDataLayer.FirstOrDefault(m =>
                        {
                            return (m.ZIndex == zIndex && m.InnerTableIndex == innerIndex);

                        }, 0, -1);

                        // ### 001
                        if (tmp != null) layerIndex++;
                    }

                    if (tmp == null) break;

                    if (!tmp.IsEndPoint)
                    {
                        itemIndex = -1;

                        l_a = null;
                        l_b = null;

                        while ((layerIndex + 1) < (_sourceDataLayer.Count - 1))
                        {
                            itemIndex++;
                            layerIndex++;

                            // ===

                            l_a = _sourceDataLayer[layerIndex];
                            l_b = _sourceDataLayer[layerIndex + 1];

                            if (l_a.ZIndex == l_b.ZIndex && (tmp.ZIndex == l_a.ZIndex)
                                &&
                                ((!l_a.IsEndPoint) && l_b.IsEndPoint))
                            {
                                //l_a.InnerTableIndex = itemIndex;
                                //l_b.InnerTableIndex = itemIndex;

                                _actionFunc(l_a, l_b, itemIndex, layerIndex);

                                layerIndex++;

                                continue;
                            }

                            // ===

                            break;
                        }
                    }

                    // ### 001
                    if (tmp.ZIndex == _sourceDataLayer[layerIndex].ZIndex && (tmp.InnerTableIndex + 1) == _sourceDataLayer[layerIndex].InnerTableIndex) layerIndex--;

                    tmp = null;
                }

                tmp = null;

                // zIndex++ ?
                //
                tmp = _sourceDataLayer.FirstOrDefault(m =>
                {
                    return (m.ZIndex == (zIndex + 1) && m.InnerTableIndex == 0);

                }, 0, -1);

                if (tmp != null)
                {
                    zIndex++;
                    layerIndex++;

                    continue;
                }

                // none zIndex++
                //
                if (tmp == null)
                {
                    break;

                    #region debug

                    //zIndex--;

                    //Page.DATA.ILayer tmp2 = null;

                    //tmp2 = _sourceDataLayer.FirstOrDefault(m =>
                    //{
                    //    return (m.ZIndex == zIndex && m.InnerTableIndex == 0);

                    //}, 0, -1);

                    ////while (tmp2 == null || zIndex >= 1)
                    //while (tmp2 == null)
                    //{
                    //    zIndex--;

                    //    tmp2 = _sourceDataLayer.FirstOrDefault(m =>
                    //    {
                    //        return (m.ZIndex == zIndex && m.InnerTableIndex == 0);

                    //    }, 0, -1);
                    //}

                    #endregion
                }
            }
        }

        private static void ReadDataLayerEndNodesTPoint(this List<Page.DATA.Layer> _sourceDataLayer, int _beginIndex, int _endIndex)
        {
            _sourceDataLayer.ReadDataLayerEndNodes((dl) =>
            {
                //var dl_a = _sourceDataLayer[_beginIndex];

                Page.DATA.Layer[] endPointLayerRenders = new Page.DATA.Layer[] { 
                
                    new Page.DATA.Layer() { InnerTableIndex = dl.InnerTableIndex + 1, IsEndPoint = true, ZIndex = dl.ZIndex }
                    ,
                    new Page.DATA.Layer() { InnerTableIndex = dl.InnerTableIndex + 1, IsEndPoint = false, ZIndex = dl.ZIndex }
                };

                return endPointLayerRenders;

            }, _beginIndex, _endIndex);
        }

        private static void ReadDataLayerEndNodesFPoint(this List<Page.DATA.Layer> _sourceDataLayer, int _beginIndex, int _endIndex)
        {
            _sourceDataLayer.ReadDataLayerEndNodes((dl) =>
            {
                //var dl_a = _sourceDataLayer[_beginIndex];

                Page.DATA.Layer[] endPointLayerRenders = new Page.DATA.Layer[] { 
                
                    new Page.DATA.Layer() { InnerTableIndex = dl.InnerTableIndex + 1, IsEndPoint = false, ZIndex = dl.ZIndex }
                };

                return endPointLayerRenders;

            }, _beginIndex, _endIndex);
        }

        private static void ReadDataLayerEndNodes(this List<Page.DATA.Layer> _sourceDataLayer, Func<Page.DATA.Layer, Page.DATA.Layer[]> _endPointLayerRenders, int _beginIndex, int _endIndex, Page.DATA.Layer _searchByThisLayer = null)
        {
            Page.DATA.Layer dl_a = _searchByThisLayer ?? _sourceDataLayer[_beginIndex];

            //if (_beginIndex == 8)
            //{
            //    int debug = 0;
            //}

            //var dl_b_for_search = new Page.DATA.LayerRender() { InnerTableIndex = dl_a.InnerTableIndex + 1, IsEndPoint = dl_a.IsEndPoint, ZIndex = dl_a.ZIndex };
            //var dl_b_for_search2 = new Page.DATA.LayerRender() { InnerTableIndex = dl_a.InnerTableIndex + 1, IsEndPoint = !dl_a.IsEndPoint, ZIndex = dl_a.ZIndex };

            //Page.DATA.LayerRender[] endPointLayerRenders = new Page.DATA.LayerRender[] { 
            //
            //    new Page.DATA.LayerRender() { InnerTableIndex = dl_a.InnerTableIndex + 1, IsEndPoint = dl_a.IsEndPoint, ZIndex = dl_a.ZIndex }
            //    ,
            //    new Page.DATA.LayerRender() { InnerTableIndex = dl_a.InnerTableIndex + 1, IsEndPoint = !dl_a.IsEndPoint, ZIndex = dl_a.ZIndex }
            //};
            //
            Page.DATA.Layer[] endPointLayerRenders = _endPointLayerRenders(dl_a);

            Page.DATA.Layer dl_b = null;

            //dl_b = _sourceDataLayer.FirstOrDefault(m => m.IsSame(dl_b_for_search));
            //
            //if (dl_b == null)
            //{
            //    dl_b = _sourceDataLayer.FirstOrDefault(m => m.IsSame(dl_b_for_search2));
            //}

            foreach (var endPointLayerRender in endPointLayerRenders)
            {
                dl_b = _sourceDataLayer.FirstOrDefault(m => m.IsSame(endPointLayerRender), _beginIndex, _endIndex);

                if (dl_b == null) continue;

                break;
            }

            //int endIndex = 0;

            if (dl_b == null)
            {
                Page.DATA.ILayer l_a = null;
                Page.DATA.ILayer l_b = null;

                bool[] exp = new bool[3];

                int groupIndex = -1;

                if (_beginIndex == _endIndex)
                {
                    if (_beginIndex - 1 >= 0)
                    {
                        if (_sourceDataLayer[_beginIndex - 1].IsEndPoint != _sourceDataLayer[_endIndex].IsEndPoint)
                        {
                            _beginIndex--;
                        }
                    }
                }

                for (int i = _beginIndex, iMax = _endIndex; i < iMax; i++)
                {
                    l_a = _sourceDataLayer[i];
                    l_b = _sourceDataLayer[i + 1];

                    exp[0] = (l_a.ZIndex + 1) == l_b.ZIndex;
                    exp[1] = ((!l_a.IsEndPoint) && l_b.IsEndPoint);

                    if (exp[0] && exp[1])
                    {
                        i++;

                        groupIndex++;

                        if (groupIndex == 0) exp[2] = true;

                        l_a.InnerTableIndex = groupIndex;
                        l_b.InnerTableIndex = groupIndex;

                        l_a.ZIndex = _sourceDataLayer[0].ZIndex;
                        l_b.ZIndex = _sourceDataLayer[0].ZIndex;

                        continue;
                    }

                    if ((!exp[0] || !exp[1]) && exp[2])
                    {
                        _endIndex = i;

                        break;
                    }
                }

                if (_endIndex < _sourceDataLayer.Count - 1)
                {
                    if (_sourceDataLayer[_endIndex].IsEndPoint)
                    {
                        _sourceDataLayer.ReadDataLayerEndNodesTPoint((_endIndex + 1), _sourceDataLayer.Count - 1);
                    }
                    else
                    {
                        _sourceDataLayer.ReadDataLayerEndNodesFPoint((_endIndex + 1), _sourceDataLayer.Count - 1);
                    }
                }

                return;
            }

            if (dl_b != null)
            {
                int endIndex = _sourceDataLayer.IndexOf(dl_b);

                if ((_beginIndex + 1) == endIndex)
                {
                    _sourceDataLayer.FixLevelKey(endIndex);

                    return;
                }

                if (dl_a.IsEndPoint)
                {
                    _sourceDataLayer.ReadDataLayerEndNodesTPoint((_beginIndex + 1), (endIndex - 1));
                }
                else
                {
                    _sourceDataLayer.ReadDataLayerEndNodesFPoint((_beginIndex + 1), (endIndex - 1));
                }
            }
        }

        public static void AllCount(this Dictionary<int, List<RowItems>> _rowItems, ref int _allCount)
        {
            foreach (var rowItemsList in _rowItems.Values)
            {
                foreach (var rowItems in rowItemsList)
                {
                    if (rowItems.InnerRowItems == null)
                    {
                        _allCount += rowItems.Items.Count;
                    }
                    else
                    {
                        rowItems.InnerRowItems.AllCount(ref _allCount);
                    }
                }
            }
        }

        public static void Combine(this List<Template.Table> _tables)
        {
            _tables.ForEach(table =>
            {

                table.Combine();
            });
        }

        public static void Combine(this Template.Table _table)
        {
            int prevTrGroupIndex = -1;

            string groupContent = "";
            string tableContent = "";

            int tlogsCount = 0;

            _table.TLogs.ForEach(trGroup =>
            {

                tlogsCount++;

                if (prevTrGroupIndex == -1) prevTrGroupIndex = trGroup.TrGroupIndex;

                #region += TR CONTENT / A

                if (trGroup.TrGroupIndex == prevTrGroupIndex)
                {
                    if (_table.GTRs[trGroup.TrGroupIndex].InnerTables == null)
                    {
                        groupContent += _table.GTRs[trGroup.TrGroupIndex].TRs[trGroup.TrIndex].Content;
                    }
                    else
                    {
                        //_table.GTRs[trGroup.TrGroupIndex].InnerTable.Combine();
                        //
                        _table.GTRs[trGroup.TrGroupIndex].InnerTables.Combine();
                    }
                }

                #endregion

                if (trGroup.TrGroupIndex != prevTrGroupIndex
                    ||
                    tlogsCount == _table.TLogs.Count)
                {
                    TemplatePageExtensions.TrGroupCombine(ref _table, ref tableContent, ref groupContent, prevTrGroupIndex);

                    #region += TR CONTENT / B

                    if (tlogsCount != _table.TLogs.Count)
                    {
                        prevTrGroupIndex = trGroup.TrGroupIndex;

                        groupContent = "";

                        if (_table.GTRs[trGroup.TrGroupIndex].InnerTables == null)
                        {
                            groupContent += _table.GTRs[trGroup.TrGroupIndex].TRs[trGroup.TrIndex].Content;
                        }
                        else
                        {
                            _table.GTRs[trGroup.TrGroupIndex].InnerTables.Combine();
                        }
                    }

                    #endregion
                }
            });
            //
            prevTrGroupIndex = _table.TLogs[tlogsCount - 1].TrGroupIndex;
            ////
            //if (_table.GTRs[prevTrGroupIndex].InnerTable != null)
            //{
            //    _table.GTRs[prevTrGroupIndex].InnerTable.Combine();

            //    TemplatePageExtensions.InnerTableCombine(ref _table, ref tableContent, prevTrGroupIndex);
            //}
            //
            TemplatePageExtensions.TrGroupCombine(ref _table, ref tableContent, ref groupContent, prevTrGroupIndex, _endingCombine: true);
            //
            TemplatePageExtensions.TableGroupCombine(ref _table, tableContent);
        }

        public static void TrGroupCombine(ref Template.Table _table, ref string _tableContent, ref string _groupContent, int _prevTrGroupIndex, bool _endingCombine = false)
        {
            if (_endingCombine)
            {
                _groupContent = "";

                if (_table.GTRs[_prevTrGroupIndex].InnerTables == null)
                {
                    if (_table.TLogs.Count == 1
                        ||
                        (_table.TLogs.Count >= 2 && _table.TLogs[_table.TLogs.Count - 1].TrGroupIndex != _table.TLogs[_table.TLogs.Count - 2].TrGroupIndex))
                    {
                        _groupContent = _table.GTRs[_prevTrGroupIndex].TRs[_table.GTRs[_prevTrGroupIndex].TRs.Count - 1].Content;
                    }
                }
                else
                {
                    _table.GTRs[_prevTrGroupIndex].InnerTables.Combine();
                }
            }

            var trGroupTemplate = _table.GTRs[_prevTrGroupIndex];

            if (trGroupTemplate.InnerTables == null)
            {
                #region Prev Tr Group

                // Group Template Empty
                //
                if (trGroupTemplate.TemplateHolder == null)
                {
                    if (!_endingCombine)
                        _tableContent += _groupContent;
                }
                else
                {
                    // debug
                    //
                    if (!_endingCombine
                        ||
                        (_endingCombine && !String.IsNullOrEmpty(_groupContent)))
                    {
                        // # 2018.06.05 001 # begin

                        //Template.BlockInfo trBlockInfo = trGroupTemplate.TRs[0].Container ?? trGroupTemplate.TRs[0].TemplateHolder;

                        //var ba = trGroupTemplate.TemplateHolder.Value.Substring(0, trBlockInfo.OuterAIndex);

                        //var bc = trGroupTemplate.TemplateHolder.Value.Substring(trBlockInfo.OuterBIndex + 1);

                        //_tableContent += ba + _groupContent + bc;

                        // # 2018.06.05 001 # end

                        ((Template.Block)trGroupTemplate).Combine(ref _groupContent);

                        _tableContent += _groupContent;
                    }
                }

                #endregion
            }
            else
            {
                //if (_endingCombine)
                //{
                //    _table.GTRs[_prevTrGroupIndex].InnerTable.Combine();
                //}

                //TemplatePageExtensions.InnerTableCombine(ref _table, ref _tableContent, _prevTrGroupIndex);
                //
                TemplatePageExtensions.InnerTablesCombine(ref _table, ref _tableContent, _prevTrGroupIndex);
            }
        }

        public static void InnerTablesCombine(ref Template.Table _table, ref string _tableContent, int _trGroupIndex)
        {
            var content = "";

            _table.GTRs[_trGroupIndex].InnerTables.ForEach(table =>
            {

                content += table.Content;
            });

            // # 2018.06.05 001 # begin

            //var innerTablesFirst = _table.GTRs[_trGroupIndex].InnerTables[0];

            //var innerTablesLast = _table.GTRs[_trGroupIndex].InnerTables[_table.GTRs[_trGroupIndex].InnerTables.Count - 1];

            //var ba = _table.GTRs[_trGroupIndex].TemplateHolder.Value.Substring(0, innerTablesFirst.TemplateHolder.OuterAIndex);

            //var bc = _table.GTRs[_trGroupIndex].TemplateHolder.Value.Substring(innerTablesLast.TemplateHolder.OuterBIndex + 1);

            //_tableContent += ba + content + bc;

            // # 2018.06.05 001 # end

            ((Template.Block)_table.GTRs[_trGroupIndex]).Combine(ref content);

            _tableContent += content;
        }

        //public static void InnerTableCombine(ref Template.Table _table, ref string _tableContent, int _trGroupIndex)
        //{
        //    Template.BlockInfo innerTableTemplateBlockInfo = _table.GTRs[_trGroupIndex].InnerTable.TemplateHolder;

        //    var ba = _table.GTRs[_trGroupIndex].TemplateHolder.Value.Substring(0, innerTableTemplateBlockInfo.OuterAIndex);

        //    var bc = _table.GTRs[_trGroupIndex].TemplateHolder.Value.Substring(innerTableTemplateBlockInfo.OuterBIndex + 1);

        //    _tableContent += ba + _table.GTRs[_trGroupIndex].InnerTable.Content + bc;
        //}

        public static void TableGroupCombine(ref Template.Table _table, string _content)
        {
            int lastGTRsIndex = _table.GTRs.Count - 1;
            //
            Template.BlockInfo trGroupTemplateFirst = null;
            Template.BlockInfo trGroupTemplateLast = null;
            //
            string tableGroupTag = string.Format(Page.TemplateTag, "Template_TRGroup");
            //
            if (_table.TemplateHolder.Value.IndexOf(tableGroupTag) == -1)
            {
                int lastTRsIndex = _table.GTRs[lastGTRsIndex].TRs.Count - 1;

                // # Container == null -> throw new argument ...
                //trGroupTemplateFirst = _table.GTRs[0].TRs[0].Container;
                //trGroupTemplateLast = _table.GTRs[lastGTRsIndex].TRs[lastTRsIndex].Container;

                trGroupTemplateFirst = _table.GTRs[0].TRs[0].Container ?? _table.GTRs[0].TRs[0].TemplateHolder;
                trGroupTemplateLast = _table.GTRs[lastGTRsIndex].TRs[lastTRsIndex].Container ?? _table.GTRs[lastGTRsIndex].TRs[lastTRsIndex].TemplateHolder;
            }
            else
            {
                // TrGroup Tag
                //
                trGroupTemplateFirst = _table.GTRs[0].TemplateHolder;
                trGroupTemplateLast = _table.GTRs[lastGTRsIndex].TemplateHolder;
            }
            //
            // tr group s => table
            //
            var aa = _table.TemplateHolder.Value.Substring(0, trGroupTemplateFirst.OuterAIndex);
            //
            var ac = _table.TemplateHolder.Value.Substring(trGroupTemplateLast.OuterBIndex + 1);
            //
            _table.Content = aa + _content + ac;
        }

        public static void Combine(this Template.TR _tr)
        {
            // # 2018.06.05 001 # begin

            //_tr.TDs.ForEach(td =>
            //{
            //    int debug6 = 0;

            //    _tr.Content += td.Content;
            //});

            //// td s => tr template
            ////
            //Template.BlockInfo tdBlockInfo = _tr.TDs[0].Container ?? _tr.TDs[0].TemplateHolder;
            //////
            ////var ba = _tr.TemplateHolder.Value.Substring(0, tdBlockInfo.OuterAIndex);
            //////
            ////var bc = _tr.TemplateHolder.Value.Substring(tdBlockInfo.OuterBIndex + 1);
            //////
            ////_tr.Content = ba + _tr.Content + bc;

            //_tr.TemplateHolder.Combine(tdBlockInfo, _tr);

            //if (_tr.Container != null)
            //{
            //    // tr template s => tr container
            //    //
            //    Template.BlockInfo trTemplateBlockInfo = _tr.TemplateHolder;
            //    ////
            //    //var ca = _tr.Container.Value.Substring(0, trTemplateBlockInfo.OuterAIndex);
            //    ////
            //    //var cc = _tr.Container.Value.Substring(trTemplateBlockInfo.OuterBIndex + 1);
            //    ////
            //    //_tr.Content = ca + _tr.Content + cc;

            //    _tr.Container.Combine(trTemplateBlockInfo, _tr);
            //}

            // # 2018.06.05 001 # end

            _tr.TDs.ForEach(td =>
            {
                int debug6 = 0;

                ((Template.Block)td).Combine();

                _tr.Content += td.Content;
            });

            ((Template.Block)_tr).Combine();

            _tr.IsRendered = true;
        }

        public static void TableCombine(this Page _page)
        {
            string tableReplaceHolderTemplateTag = string.Format(Page.ReplaceHolderTemplateTag, "TABLE", "{0}");

            string pageContent = _page.Content;

            int tableIndex = -1;

            _page.Tables.ForEach(table =>
            {
                int debug8 = 0;

                tableIndex++;

                pageContent = pageContent.Replace(string.Format(tableReplaceHolderTemplateTag, tableIndex), table.Content);
            });

            _page.ContentRef(ref pageContent);
        }

        public static void Append(this List<Template.TagInfo> _tagInfos, List<Template.TagInfo> _extendTagInfos)
        {
            _extendTagInfos.ForEach(m =>
            {

                if (!_tagInfos.Contains(m))
                {
                    _tagInfos.Add(m);
                }
            });
        }

        public static void Combine(this Template.BlockInfo _blockInfoOuter, Template.BlockInfo _blockInfoInner, Template.TR _tr)
        {
            // var ba = _blockInfoOuter.Value.Substring(0, _blockInfoInner.OuterAIndex);
            //
            string ba, bc;
            //
            _blockInfoOuter.ToPairs(_blockInfoInner, out ba, out bc);
            //
            _tr.Content = ba + _tr.Content + bc;

            // ===

            //List<Template.TagInfo> tags = new List<Template.TagInfo>();

            //// # debug # => DATA2
            ////
            //var dataList = ((UnitTestProject2.Page.DATA2)_tr.DATA)[_tr.DATA.TemplateTableIndex];

            //foreach (var item in dataList)
            //{
            //    foreach (var item2 in item.Items)
            //    {
            //        var tag = _blockInfoOuter.Tags.Filter(item2, (IItemExtend)item2);

            //        tags.Append(tag);
            //    }
            //}

            //string tagContent = "";
            //string tagsContent = "";

            //foreach (var item in tags)
            //{
            //    // ########
            //    // get => List<Page.RenderModel> renderModelsExtend => by List<Template.TagInfo> (tags) for one item

            //    // #2 data & extend params => render => item (tagContent)

            //    tagContent = item.TemplateHolder;

            //    // tr => none renderModels
            //    //
            //    //renderModels.TemplateFilter(ref tagContent);

            //    tagsContent += tagContent;
            //}

            //if (!String.IsNullOrEmpty(tagsContent))
            //{
            //    ba = ba.Replace(td.TemplateHolder.TagInfosReplaceHolderTemplateTag, tagsContent);

            //    bc = bc.Replace(td.TemplateHolder.TagInfosReplaceHolderTemplateTag, tagsContent);
            //}

            //_tr.Content = ba + _tr.Content + bc;
        }

        public static void Combine(this Template.BlockInfo _blockInfoOuter, Template.BlockInfo _blockInfoInner, Template.TD _td)
        {
            //// var ba = _blockInfoOuter.Value.Substring(0, _blockInfoInner.OuterAIndex);
            ////
            //string ba, bc;
            ////
            //_blockInfoOuter.ToPairs(_blockInfoInner, out ba, out bc);
            ////
            //_td.Content = ba + _td.Content + bc;

            // # 2018.06.05 001 #

            ((Template.Block)_td).Combine();
        }

        public static void Combine(this Template.Block _block)
        {
            if (_block.Pairs == null) return;

            for (int i = 0, iMax = _block.Pairs.Count - 1; i <= iMax; )
            {
                _block.Content = _block.Pairs[i] + _block.Content + _block.Pairs[i + 1];

                i += 2;
            }
        }

        public static void Combine(this Template.Block _block, ref string _content)
        {
            if (_block.Pairs == null) return;

            //_content = _block.Content;

            for (int i = 0, iMax = _block.Pairs.Count - 1; i <= iMax; )
            {
                _content = _block.Pairs[i] + _content + _block.Pairs[i + 1];

                i += 2;
            }
        }

        public static List<string> ToPairs(this Template.BlockInfo _blockInfoOuter, Template.BlockInfo _blockInfoInner)
        {
            if (_blockInfoOuter == null) return null;

            //// var ba = _blockInfoOuter.Value.Substring(0, _blockInfoInner.OuterAIndex);
            ////
            //var ba = _blockInfoOuter.Value.Substring(0, _blockInfoInner.OuterAIndex + 1);
            ////
            //var bc = _blockInfoOuter.Value.Substring(_blockInfoInner.OuterBIndex + 1);
            ////
            string ba, bc;
            //
            _blockInfoOuter.ToPairs(_blockInfoInner, out ba, out bc);
            //
            return new List<string>() { ba, bc };
        }

        public static List<string> ToPairs(this Template.BlockInfo _blockInfoOuter, Template.BlockInfo _blockInfoFirst, Template.BlockInfo _blockInfoLast)
        {
            if (_blockInfoOuter == null) return null;

            string ba, bc;
            //
            _blockInfoOuter.ToPairs(_blockInfoFirst, _blockInfoLast, out ba, out bc);
            //
            return new List<string>() { ba, bc };
        }

        public static void ToPairs(this Template.BlockInfo _blockInfoOuter, Template.BlockInfo _blockInfoInner, out string _aStr, out string _bStr)
        {
            // : (_blockInfoFirst.OuterAIndex + 1) - 1
            //
            _aStr = _blockInfoOuter.Value.Substring(0, _blockInfoInner.OuterAIndex);

            _bStr = _blockInfoOuter.Value.Substring(_blockInfoInner.OuterBIndex + 1);
        }

        public static void ToPairs(this Template.BlockInfo _blockInfoOuter, Template.BlockInfo _blockInfoFirst, Template.BlockInfo _blockInfoLast, out string _aStr, out string _bStr)
        {
            // : (_blockInfoFirst.OuterAIndex + 1) - 1
            //
            _aStr = _blockInfoOuter.Value.Substring(0, _blockInfoFirst.OuterAIndex);

            _bStr = _blockInfoOuter.Value.Substring(_blockInfoLast.OuterBIndex + 1);
        }

        public static List<string> Append(this List<string> _list, List<string> _newList)
        {
            if (_newList == null) return _list;

            if (_list == null) _list = new List<string>();

            _newList.ForEach(m =>
            {

                _list.Add(m);
            });

            return _list;
        }
    }
}
