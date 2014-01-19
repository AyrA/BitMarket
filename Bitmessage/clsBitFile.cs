using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace SQL
{
    public class BitFile
    {
        private const string FILES_DIR = "FILES";

        private string _name, _address;

        public int Index
        { get; private set; }

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
                            MarketInterface.Exec("INSERT INTO BitFile (Name) VALUES(?)", new object[] { value });
                            Index = (int)MarketInterface.ExecReader("SELECT ID FROM BitFile ORDER BY ID DESC LIMIT 1")[0].Values["ID"];
                        }
                        else
                        {
                            MarketInterface.Exec("UPDATE BitFile SET Name=? WHERE ID=?", new object[] { value, Index });
                        }
                    }
                }
                else
                {
                    _name = null;
                    if (Index >= 0)
                    {
                        MarketInterface.Exec("DELETE FROM BitFile WHERE ID=?", new object[] { Index });
                        Index = -1;
                    }
                }
            }
        }

        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (Index >= 0)
                {
                    _address = value;
                    MarketInterface.Exec("UPDATE BitFile SET Address=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    throw new Exception("Set Name first!");
                }
            }
        }

        public byte[] Contents
        {
            get
            {
                if (File.Exists(vName))
                {
                    return File.ReadAllBytes(vName);
                }
                return null;
            }
            set
            {
                if (Index >= 0)
                {
                    if (value != null)
                    {
                        if (!Directory.Exists(FILES_DIR))
                        {
                            Directory.CreateDirectory(FILES_DIR);
                        }
                        if (File.Exists(vName))
                        {
                            File.Delete(vName);
                        }
                        File.WriteAllBytes(vName, value);
                    }
                }
                else
                {
                    throw new Exception("Set the name first!");
                }
            }
        }

        private string vName
        {
            get
            {
                if (Index >= 0)
                {
                    return string.Format(@"{0}\{1:00000000}.bin", FILES_DIR, Index);
                }
                throw new Exception("Set the name first!");
            }
        }

        public BitFile()
        {
            Index = -1;
            _name = null;
        }

        public BitFile(int i)
        {
            SQLRow[] SR = MarketInterface.ExecReader("SELECT Name,Address FROM BitFile WHERE ID=?", new object[] { i });
            if (SR != null && SR.Length > 0)
            {
                Index = i;
                _name = SR[0].Values["Name"].ToString();
                _address = SR[0].Values["Address"].ToString();
            }
            else
            {
                throw new Exception("Invalid Index");
            }
        }

        public void Kill()
        {
            if (Index >= 0)
            {
                MarketInterface.Exec("DELETE FROM BitFile WHERE ID=?", new object[] { Index });
                if (File.Exists(vName))
                {
                    File.Delete(vName);
                }
            }
            Index = -1;
        }
    }
}
