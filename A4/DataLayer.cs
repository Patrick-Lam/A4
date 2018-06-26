using System;
using System.Collections.Generic;
using System.Reflection;

namespace A4
{
    public class DataLayer
    {
        private DataLayer source = null;

        private List<Page.DATA.Layer> sourceDataLayer = null;

        private object obj = null;

        private Type objType = null;

        private string[] innerNodesFilter = null;

        private Exception exception = null;

        private Assembly ay = null;

        private bool isDataLayerRendered = false;

        public DataLayer(object _obj, Type _objType, string[] _innerNodesFilter = null)
        {
            //this.Init();

            this.obj = _obj;
            this.objType = _objType;
            this.ay = Assembly.GetExecutingAssembly();

            this.sourceDataLayer = new System.Collections.Generic.List<Page.DATA.Layer>();

            this.source = this;

            this.innerNodesFilter = _innerNodesFilter;
        }

        public DataLayer(object _obj, Type _objType, DataLayer _dataLayer, string[] _innerNodesFilter = null)
        {
            this.obj = _obj;
            this.objType = _objType;
            this.ay = _dataLayer.ay;
            this.sourceDataLayer = _dataLayer.sourceDataLayer;

            this.source = _dataLayer.source;

            this.innerNodesFilter = _innerNodesFilter;
        }

        public List<Page.DATA.Layer> ToDataLayer()
        {
            if (!this.isDataLayerRendered)
            {
                DataLayer.Render(this.source, 0, 0);

                this.isDataLayerRendered = true;
            }

            return this.sourceDataLayer;
        }

        private static void Render(DataLayer _dataLayer, int _zIndex, int _innerTableIndex, bool _innerTry = false)
        {
            //if (_dataLayer.sourceDataLayer.Count == 1)
            //{
            //    int debug_3 = 0;
            //}

            //if (_dataLayer.sourceDataLayer.Count == 7)
            //{
            //    int debug_3 = 0;
            //}

            //if (_zIndex == 1 && _innerTableIndex == 2)
            //{
            //    int debug_4 = 0;
            //}

            _dataLayer.exception = null;

            BindingFlags bindingFlags = BindingFlags.Default;

            #region # END TYPE

            switch (_dataLayer.objType.FullName)
            {
                case "System.String":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                case "System.Boolean":
                    {
                        // ... end

                        if (_dataLayer.source.innerNodesFilter == null)
                        {
                            _dataLayer.sourceDataLayer.Add(new Page.DATA.Layer(_zIndex, _innerTableIndex));
                        }

                        return;
                    }
                    break;
            }

            #endregion

            #region System List TYPE

            int tryIMax = -1;
            int tryI = -1;
            object tryObj = null;
            object tryObjItem = null;
            Type tryType = null;


            if (_innerTry)
            {
                //if (_dataLayer.objType.FullName.StartsWith("System."))
                //{
                //    return;
                //}

                #region # []

                object last_object = null;

                while (_dataLayer.exception == null)
                {
                    tryI++;

                    //if (_zIndex == 3 && tryI == 0)
                    //{
                    //    int datalayer_break_debug = 0;
                    //}

                    try
                    {
                        tryObjItem = _dataLayer.objType.InvokeMember("", BindingFlags.GetProperty, null, _dataLayer.obj, new Object[] { tryI });

                        if (last_object == tryObjItem)
                        {
                            tryI--;

                            break;
                        }

                        if (last_object != null && last_object.Equals(tryObjItem))
                        {
                            tryI--;

                            break;
                        }

                        last_object = tryObjItem;

                        if (tryObjItem != null)
                        {
                            tryType = tryObjItem.GetType();

                            //if (_zIndex >= 100)
                            //{
                            //    int debug_zindex_break = 0;

                            //    //throw new ArgumentException("zindex break");
                            //}

                            //var innerTryLockBreak = _dataLayer.sourceDataLayer.FirstOrDefault(m => (m.ZIndex == _zIndex && m.InnerTableIndex == tryI && m.IsLog));

                            //if (innerTryLockBreak == null)
                            //{
                            //    _dataLayer.sourceDataLayer.Add(new Page.DATA.Layer(_zIndex, tryI, _isEndPoint: false, _isLog: true));
                            //}
                            //else
                            //{
                            //    if (innerTryLockBreak.Count >= 100)
                            //    {
                            //        int debug_layer_break = 0;

                            //        //throw new ArgumentException("layer break");
                            //    }

                            //    innerTryLockBreak.Count++;
                            //}

                            //if (_dataLayer.sourceDataLayer.Count >= 100)
                            //{
                            //    int debug_log_break = 0;
                            //}

                            DataLayer innerDataLayer = new DataLayer(tryObjItem, tryType, _dataLayer);

                            // _zIndex + 1
                            //
                            DataLayer.Render(innerDataLayer, (_zIndex + 0), tryI);
                        }
                    }
                    catch (Exception e)
                    {
                        _dataLayer.exception = e;

                        tryI--;

                        break;
                    }
                }

                #endregion

                return;
            }
            else if (_dataLayer.objType.FullName.StartsWith("System.Collections.Generic.List`1"))
            {
                #region System => List

                // System.Collections.Generic.List`1[[A4.DATA.Model.RowItems, UnitTestProject1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
                // 
                // tryInnerTypesFullName == new string[] { "A4.DATA.Model.RowItems" }
                //
                //string[] tryInnerTypesFullName = DataLayer.GetInnerTypesFullName(_dataLayer.fullTypeName);

                try
                {
                    tryObj = _dataLayer.objType.InvokeMember("Count", BindingFlags.GetProperty, null, _dataLayer.obj, new Object[] { });

                    tryIMax = (int)tryObj;
                }
                catch (Exception e)
                {
                    _dataLayer.exception = e;
                }

                if (_dataLayer.exception == null && tryIMax >= 1)
                {
                    tryI = 0;

                    for (; tryI < tryIMax; tryI++)
                    {
                        tryObjItem = _dataLayer.objType.InvokeMember("", BindingFlags.GetProperty, null, _dataLayer.obj, new Object[] { tryI });

                        if (tryObjItem != null)
                        {
                            if (tryType == null)
                            {
                                //tryType = _dataLayer.GetType(tryInnerTypesFullName[0]);
                                //
                                tryType = tryObjItem.GetType();
                            }

                            // debug
                            //
                            _dataLayer.sourceDataLayer.Add(new Page.DATA.Layer(_zIndex, tryI, _isEndPoint: false));

                            DataLayer innerDataLayer = new DataLayer(tryObjItem, tryType, _dataLayer);

                            // _zIndex + 0
                            //
                            DataLayer.Render(innerDataLayer, (_zIndex + 0), tryI);
                        }
                    }
                }

                #endregion

                return;
            }
            else if (_dataLayer.objType.FullName.StartsWith("System.Collections.Generic.Dictionary`2"))
            {
                #region System => Dictionary

                // System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[UnitTestProject2.z_outer, UnitTestProject1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
                //
                //string[] tryInnerTypesFullName = DataLayer.GetInnerTypesFullName(_dataLayer.fullTypeName);

                // tryInnerTypesFullName[0] => Dic Key      Type
                // tryInnerTypesFullName[1] => Dic Value    Type

                object dic_Values = _dataLayer.objType.InvokeMember("Values", BindingFlags.GetProperty, null, _dataLayer.obj, new Object[] { });

                Type values_Type = dic_Values.GetType();

                object dic_GetEnumerator = null;

                Type dic_GetEnumerator_Type = null;

                try
                {
                    dic_GetEnumerator = values_Type.InvokeMember("GetEnumerator", BindingFlags.InvokeMethod, null, dic_Values, new Object[] { });

                    dic_GetEnumerator_Type = dic_GetEnumerator.GetType();
                }
                catch (Exception e)
                {
                    _dataLayer.exception = e;
                }

                if (_dataLayer.exception == null)
                {
                    bool tryBoolean = false;

                    try
                    {
                        tryObj = dic_GetEnumerator_Type.InvokeMember("MoveNext", BindingFlags.InvokeMethod, null, dic_GetEnumerator, new Object[] { });

                        tryBoolean = (bool)tryObj;
                    }
                    catch (Exception e)
                    {
                        _dataLayer.exception = e;
                    }

                    if (_dataLayer.exception == null && tryObj != null && tryBoolean)
                    {
                        while ((bool)tryObj)
                        {
                            tryI++;

                            tryObjItem = dic_GetEnumerator_Type.InvokeMember("Current", BindingFlags.GetProperty, null, dic_GetEnumerator, new Object[] { });

                            if (tryObjItem != null)
                            {
                                if (tryType == null)
                                {
                                    tryType = tryObjItem.GetType();
                                }

                                // debug
                                //
                                _dataLayer.sourceDataLayer.Add(new Page.DATA.Layer(_zIndex, tryI, _isEndPoint: false));

                                DataLayer innerDataLayer = new DataLayer(tryObjItem, tryType, _dataLayer);

                                // _zIndex + 1
                                //
                                DataLayer.Render(innerDataLayer, (_zIndex + 0), tryI);
                            }

                            tryObj = dic_GetEnumerator_Type.InvokeMember("MoveNext", BindingFlags.InvokeMethod, null, dic_GetEnumerator, new Object[] { });

                            if (tryObj == null) break;
                        }
                    }
                }

                #endregion

                return;
            }

            #endregion

            #region UserDefined Class TYPE

            MemberInfo[] dataLayerMemberInfos = _dataLayer.objType.GetMembers();

            bool isInnerDataLayer = false;

            foreach (var memberInfo in dataLayerMemberInfos)
            {
                isInnerDataLayer = true;

                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        {
                            bindingFlags = BindingFlags.GetProperty;
                        }
                        break;

                    case MemberTypes.Field:
                        {
                            bindingFlags = BindingFlags.GetField;
                        }
                        break;

                    //case MemberTypes.TypeInfo:
                    //    { 
                    //        // enum
                    //    }
                    //    break;

                    default:
                        {
                            isInnerDataLayer = false;
                        }
                        break;
                }

                if (!isInnerDataLayer) continue;

                if (_dataLayer.source.innerNodesFilter != null)
                {
                    //if (memberInfo.Name == "GTRs" || memberInfo.Name == "InnerTables" || memberInfo.Name == "InnerRowItems")
                    //{
                    //    int debuggtrs = 0;
                    //}

                    if (Array.IndexOf<string>(_dataLayer.source.innerNodesFilter, memberInfo.Name) == -1)
                    {
                        tryObjItem = _dataLayer.objType.InvokeMember(memberInfo.Name, bindingFlags, null, _dataLayer.obj, new Object[] { });

                        if (tryObjItem != null)
                        {
                            tryType = tryObjItem.GetType();

                            DataLayer innerDataLayer2 = new DataLayer(tryObjItem, tryType, _dataLayer);

                            DataLayer.Render(innerDataLayer2, (_zIndex + 1), _innerTableIndex, _innerTry: true);
                        }

                        continue;
                    }
                }

                tryObjItem = _dataLayer.objType.InvokeMember(memberInfo.Name, bindingFlags, null, _dataLayer.obj, new Object[] { });

                if (tryObjItem == null)
                {
                    if (_dataLayer.source.innerNodesFilter != null)
                    {
                        if (Array.IndexOf<string>(_dataLayer.source.innerNodesFilter, memberInfo.Name) != -1)
                        {
                            _dataLayer.sourceDataLayer.Add(new Page.DATA.Layer(_zIndex, _innerTableIndex));
                        }
                    }

                    continue;
                }

                tryType = tryObjItem.GetType();

                if (_dataLayer.sourceDataLayer.Count >= 1)
                {
                    var tmp = _dataLayer.sourceDataLayer[_dataLayer.sourceDataLayer.Count - 1];

                    if (!(tmp.ZIndex == _zIndex && tmp.InnerTableIndex == _innerTableIndex && tmp.IsEndPoint == false))
                    {
                        _dataLayer.sourceDataLayer.Add(new Page.DATA.Layer(_zIndex, _innerTableIndex, _isEndPoint: false));
                    }
                }

                DataLayer innerDataLayer = new DataLayer(tryObjItem, tryType, _dataLayer);

                DataLayer.Render(innerDataLayer, (_zIndex + 1), _innerTableIndex);
            }

            #endregion
        }
    }
}
