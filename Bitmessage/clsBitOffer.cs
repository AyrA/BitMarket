using System;
using System.Collections.Generic;
namespace SQL
{
    public class BitOffer
    {
        private int _category, _stock;
        private string _title, _description, _address, _files, _pricemap;
        private DateTime _lastModify;

        public int Index
        { get; private set; }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value.Length <= 50)
                {
                    _title = value;
                    if (Index >= 0)
                    {
                        MarketInterface.Exec("UPDATE BitOffer SET Title=? WHERE ID=?", new object[] { value, Index });
                    }
                    else
                    {
                        MarketInterface.Exec("INSERT INTO BitOffer (Title) VALUES(?)", new object[] { value });
                        Index = (int)MarketInterface.ExecReader("SELECT ID FROM BitOffer ORDER BY ID DESC LIMIT 1")[0].Values["ID"];
                    }
                }
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value.Length <= 2000)
                {
                    if (Index >= 0)
                    {
                        _description = value;
                        MarketInterface.Exec("UPDATE BitOffer SET Description=? WHERE ID=?", new object[] { value, Index });
                    }
                    else
                    {
                        throw new Exception("Set Title first!");
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
                    MarketInterface.Exec("UPDATE BitOffer SET Address=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    throw new Exception("Set Title first!");
                }
            }
        }

        public int Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (Index >= 0)
                {
                    if (Category > -3 && Category != 0)
                    {
                        _category = value;
                        MarketInterface.Exec("UPDATE BitOffer SET Category=? WHERE ID=?", new object[] { value, Index });
                    }
                    else
                    {
                        throw new Exception("Invalid Category!");
                    }
                }
                else
                {
                    throw new Exception("Set Title first!");
                }
            }
        }

        public int Stock
        {
            get
            {
                return _stock;
            }
            set
            {
                if (Index >= 0)
                {
                    if (Stock > -2)
                    {
                        _stock = value;
                        MarketInterface.Exec("UPDATE BitOffer SET Stock=? WHERE ID=?", new object[] { value, Index });
                    }
                    else
                    {
                        throw new Exception("Invalid Stock count!");
                    }
                }
                else
                {
                    throw new Exception("Set Title first!");
                }
            }
        }

        public string Files
        {
            get
            {
                return _files;
            }
            set
            {
                if (value.Length <= 50)
                {
                    if (Index >= 0)
                    {
                        uint temp = 0;
                        foreach (string s in value.Split(','))
                        {
                            if (!uint.TryParse(s.Trim(), out temp))
                            {
                                throw new Exception("Invalid File index");
                            }
                        }
                        _files = value;
                        MarketInterface.Exec("UPDATE BitOffer SET Files=? WHERE ID=?", new object[] { value, Index });
                    }
                    else
                    {
                        throw new Exception("Set Title first!");
                    }
                }
            }
        }

        /// <summary>
        /// PriceMap.ToString()
        /// </summary>
        /// <remarks>We cannot name this PriceMap because it would match BitMarket.PriceMap</remarks>
        public string Prices
        {
            get
            {
                return _pricemap;
            }
            set
            {
                if (value.Length <= 200)
                {
                    if (Index >= 0)
                    {
                        PriceMap PM = new PriceMap(value);
                        MarketInterface.Exec("UPDATE BitOffer SET PriceMap=? WHERE ID=?", new object[] { PM.ToString(), Index });
                    }
                    else
                    {
                        throw new Exception("Set Title first!");
                    }
                }
            }
        }

        public DateTime LastModify
        {
            get
            {
                return _lastModify;
            }
            set
            {
                if (Index >= 0)
                {
                    _lastModify = value;
                    MarketInterface.Exec("UPDATE BitOffer SET LastModify=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    throw new Exception("Set Title first!");
                }
            }
        }

        public bool CanBuy(int Amount,int UnderStock)
        {
            if (Amount > 0 && UnderStock > 0 && UnderStock <= Amount && Amount<=Stock)
            {
                PriceMap PM = new PriceMap(Prices);
                if (!double.IsNaN(PM.getValue(Amount, 0)))
                {
                    return true;
                }
                return !double.IsNaN(PM.getValue(UnderStock, 0));
            }
            return false;
        }

        public void Update()
        {
            LastModify = DateTime.Now.ToUniversalTime();
        }

        public BitOffer()
        {
            _stock = _category = Index = -1;
            _pricemap = _files = _address = _description = _title = null;
            _lastModify = DateTime.Now;
        }

        public BitOffer(int i)
        {
            SQLRow[] SR = MarketInterface.ExecReader("SELECT * FROM BitOffer WHERE ID=?", new object[] { i });
            if (SR != null && SR.Length > 0)
            {
                Index = i;
                _title = SR[0].Values["Title"].ToString();
                _description = SR[0].Values["Description"].ToString();
                _address = SR[0].Values["Address"].ToString();
                _category = (int)SR[0].Values["Category"];
                _files = SR[0].Values["Files"].ToString();
                _stock = (int)SR[0].Values["Stock"];
                _pricemap = SR[0].Values["PriceMap"].ToString();
                _lastModify = (DateTime)SR[0].Values["LastModify"];
            }
            else
            {
                throw new Exception("Invalid Index");
            }
        }

        public void Kill()
        {
            MarketInterface.Exec("DELETE FROM BitOffer WHERE ID=?", new object[] { Index });
            _stock = _category = Index = -1;
            _pricemap = _files = _address = _description = _title = null;
            _lastModify = DateTime.Now;
        }

        public BitTransaction Buy(int amount, int understock)
        {
            BitTransaction BT = null;
            PriceMap PM = new PriceMap(Prices);
            if (CanBuy(amount, understock))
            {
                if (!double.IsNaN(PM.getValue(amount, 0)))
                {
                    BT = new BitTransaction(this);
                    BT.Amount = amount;
                }
                if (!double.IsNaN(PM.getValue(understock, 0)))
                {
                    BT = new BitTransaction(this);
                    BT.Amount = understock;
                }
            }
            return BT;
        }
    }
}
