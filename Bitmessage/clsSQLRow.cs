using System.Collections.Generic;

namespace SQL
{
    public class SQLRow
    {
        private Dictionary<string, object> _cols;

        public Dictionary<string, object> Values
        {
            get
            {
                return _cols;
            }
        }

        public SQLRow()
        {
            _cols = new Dictionary<string, object>();
        }

        ~SQLRow()
        {
            _cols = null;
        }

        public void Add(string col,object value)
        {
            _cols.Add(col, value);
        }
    }
}
