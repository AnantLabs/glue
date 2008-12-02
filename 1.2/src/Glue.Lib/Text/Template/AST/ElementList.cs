using System;
using System.Collections;

namespace Glue.Lib.Text.Template.AST
{
	public class ElementList : IEnumerable
	{
        ArrayList list = new ArrayList();

        public ElementList()
        {
        }

        public void Add(Element element)
        {
            list.Add(element);
        }

        public void Clear()
        {
            list.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int Count
        {
            get { return list.Count; }
        }

        public Element this[int index]
        {
            get { return (Element)list[index]; }
        }
    }
}
