using System;
using System.Collections.Generic;
using System.Text;

namespace SQL
{
    public class BitCategory
    {
        private string _name;
        private BitCategory _parent;
        private int _parentID;

        public int Index
        { get; private set; }

        public BitCategory Parent
        {
            get
            {
                if (_parent == null)
                {
                    if (_parentID >= 0)
                    {
                        SQLRow[] SR = MarketInterface.ExecReader("SELECT * FROM BitCategory WHERE ID=" + _parentID.ToString());
                        if (SR != null && SR.Length > 0)
                        {
                            _parent = new BitCategory((int)SR[0].Values["ID"], SR[0].Values["Name"].ToString(), (int)SR[0].Values["Parent"]);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return _parent;
            }
            set
            {
                if (Index < 0)
                {
                    throw new Exception("Set name first");
                }
                _parent = value;
                if (_parent == null)
                {
                    _parentID = -1;
                    MarketInterface.Exec("UPDATE BitCategory SET Parent=NULL WHERE ID=?", new object[] { Index });
                }
                else
                {
                    _parentID = _parent.Index;
                    MarketInterface.Exec("UPDATE BitCategory SET Parent=? WHERE ID=?", new object[] { _parentID, Index });
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Length <= 50)
                    {
                        _name = value;
                        if (Index < 0)
                        {
                            MarketInterface.Exec("INSERT INTO BitCategory (Name) VALUES(?)", new object[] { value });
                            Index = (int)MarketInterface.ExecReader("SELECT ID FROM BitCategory ORDER BY ID DESC LIMIT 1")[0].Values["ID"];
                        }
                        else
                        {
                            MarketInterface.Exec("UPDATE BitCategory SET Name=? WHERE ID=?", new object[] { value, Index });
                        }
                    }
                }
                else
                {
                    _name = null;
                    if (Index >= 0)
                    {
                        MarketInterface.Exec("DELETE FROM BitCategory WHERE ID=?", new object[] { Index });
                        Index = -1;
                    }
                }
            }
        }

        public BitCategory()
        {
            Index = -1;
            _name = null;
            _parent = null;
        }

        public BitCategory(int cIndex)
        {
            SQLRow[] RR = MarketInterface.ExecReader("SELECT * FROM BitCategory WHERE ID=?", new object[] { cIndex });
            if (RR.Length == 1)
            {
                _name = RR[0].Values["Name"].ToString();
                Index = cIndex;
                _parentID = RR[0].Values["Parent"] == null ? -1 : (int)RR[0].Values["Parent"];
                if (_parentID > -1)
                {
                    _parent = new BitCategory(_parentID);
                }
            }
            else
            {
                throw new Exception("Invalid Category ID");
            }
        }

        public BitCategory(string cName)
        {
            Index = -1;
            Name = cName;
            _parent = null;
            _parentID = -1;
        }

        public BitCategory(int cIndex, string cName,int cParentID)
        {
            Index = cIndex;
            Name = cName;
            _parentID = cParentID;
            _parent = null;
        }

        public void Kill()
        {
            MarketInterface.Exec("DELETE FROM BitCategory WHERE ID=?", new object[] { Index });
            Index = _parentID = -1;
            _name = null;
            _parent = null;
        }
    }
}
