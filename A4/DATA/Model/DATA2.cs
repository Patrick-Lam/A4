using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4.DATA
{
    namespace Model
    {
        public class RowItems
        {
            public RowItems()
            {
                this.Items = new List<Item>();
            }

            public List<Item> Items { get; set; }

            public Dictionary<int, List<RowItems>> InnerRowItems { get; set; }
        }

        public class Item : IItemExtend
        {
            public Item()
            {
                this.ColumnName = null;
            }

            [ItemExtendName("LabelName")]
            public string Name { get; set; }
            public object Value { get; set; }

            public string ColumnName { get; set; }

            public ItemExtendParams ItemExtendParams { get; set; }

            public Rnd<IItemExtend> GroupItems { get; set; }

            public List<ItemExtendAttribute> ItemExtendAttributes { get; set; }

            public enum ItemExtendAttribute
            {
                Hidden = 0
            }
        }

        public class Rnd<T> where T : IItemExtend
        {
            public Rnd(string _expression)
            {
                this.BindingColumns = new List<T>();

                this.Expression = _expression;
            }

            public string Expression { get; set; }

            public List<T> BindingColumns { get; set; }
        }

        public class ItemExtendParams
        {
            public ItemExtendParams(object _value, Type _valueType, Func<object, Type, List<A4.Page.RenderModel>> _toRenderModelsFunc)
            {
                this.Value = _value;
                this.ValueType = _valueType;
                this.ToRenderModelsFunc = _toRenderModelsFunc;
            }

            public object Value { get; set; }

            public Type ValueType { get; set; }

            public Func<object, Type, List<A4.Page.RenderModel>> ToRenderModelsFunc { get; set; }
        }

        public interface IItemExtend
        {
            A4.DATA.Model.Rnd<IItemExtend> GroupItems { get; set; }
            ItemExtendParams ItemExtendParams { get; set; }
        }

        public class ItemExtendNameAttribute : Attribute
        {
            public ItemExtendNameAttribute(string _extendName)
            {
                this.extendName = _extendName;
            }

            protected string extendName;

            public string ExtendName
            {
                get { return this.extendName; }
                set { this.extendName = value; }
            }
        }

        public static class ItemExtendParamsExtend
        {
            public static void Add<T>(this List<T> _listT, T _item)
            {
                if (_listT == null)
                {
                    _listT = new List<T>();
                }

                _listT.Add(_item);
            }

            public static List<A4.Page.RenderModel> ConvertToRenderModels(this ItemExtendParams _itemExtendParams)
            {
                if (_itemExtendParams.ToRenderModelsFunc != null)
                {
                    return _itemExtendParams.ToRenderModelsFunc(_itemExtendParams.Value, _itemExtendParams.ValueType);
                }

                return null;
            }
        }
    }
}
