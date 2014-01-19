using System;
namespace SQL
{
    public enum TransactionState
    {
        RejectedByBoth = -3,
        RejectedByBuyer = -2,
        RejectedBySeller = -1,
        Neutral = 0,
        Confirmed = 1,
        Completed = 2,
        Commented = 3,
        NoBuyerComment = 4,
        NoSellerComment = 5,
        NoComment = 6
    }

    public enum Rating
    {
        Unrated = 0,
        Bad = 1,
        Poor = 2,
        Neutral = 3,
        Good = 4,
        Perfect = 5
    }

    public class BitTransaction
    {
        /// <summary>
        /// Number of days, after which a Transaction expires
        /// </summary>
        public const int EXP_DAYS = 60;

        private string _addrBuyer, _addrSeller, _buyerComment, _sellerComment;
        private int _amount, _offer;
        private Rating _buyerRating, _sellerRating;
        private TransactionState _state;
        private DateTime _transactionTime;

        public int Index
        { get; private set; }

        public string AddressBuyer
        {
            get
            {
                return _addrBuyer;
            }
            set
            {
                if (Index >= 0)
                {
                    _addrBuyer = value;
                    MarketInterface.Exec("UPDATE BitTransaction SET AddressBuyer=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public string AddressSeller
        {
            get
            {
                return _addrSeller;
            }
            set
            {
                if (Index >= 0)
                {
                    _addrSeller = value;
                    MarketInterface.Exec("UPDATE BitTransaction SET AddressSeller=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public int Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (Index >= 0)
                {
                    if (value > 0)
                    {
                        _amount = value;
                        MarketInterface.Exec("UPDATE BitTransaction SET Amount=? WHERE ID=?", new object[] { value, Index });
                    }
                    else
                    {
                        throw new Exception("Invalid amount");
                    }
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public int Offer
        {
            get
            {
                return _offer;
            }
            set
            {
                _offer = value;
                if (Index >= 0)
                {
                    MarketInterface.Exec("UPDATE BitTransaction SET Offer=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    MarketInterface.Exec("INSERT INTO BitTransaction (Offer,State,BuyerRating,SellerRating,TransactionTime) VALUES(?,0,0,0,?)", new object[] { value, DateTime.Now.ToUniversalTime() });
                    Index = (int)MarketInterface.ExecReader("SELECT ID FROM BitTransaction ORDER BY ID DESC LIMIT 1")[0].Values["ID"];
                }
            }
        }

        public TransactionState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (Index >= 0)
                {
                    switch (value)
                    {
                        case TransactionState.Commented:
                        case TransactionState.Completed:
                        case TransactionState.Confirmed:
                        case TransactionState.Neutral:
                        case TransactionState.NoBuyerComment:
                        case TransactionState.NoComment:
                        case TransactionState.NoSellerComment:
                            break;
                        case TransactionState.RejectedByBoth:
                        case TransactionState.RejectedByBuyer:
                        case TransactionState.RejectedBySeller:
                            //On reject, remove comments and rating (if state was OK)
                            if ((int)_state >= 0)
                            {
                                SellerComment = null;
                                BuyerComment = null;
                                SellerRating = Rating.Unrated;
                                BuyerRating = Rating.Unrated;
                            }
                            break;
                    }
                    _state = value;
                    MarketInterface.Exec("UPDATE BitTransaction SET State=? WHERE ID=?", new object[] { (int)value, Index });
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public string BuyerComment
        {
            get
            {
                return _buyerComment;
            }
            set
            {
                if (Index >= 0)
                {
                    if ((int)State < 3 && ((int)State >= 0 || string.IsNullOrEmpty(_buyerComment)))
                    {
                        _buyerComment = value;
                        MarketInterface.Exec("UPDATE BitTransaction SET BuyerComment=? WHERE ID=?", new object[] { value, Index });
                        Update();
                    }
                    else
                    {
                        throw new Exception("You cannot comment or rate in this state");
                    }
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public string SellerComment
        {
            get
            {
                return _sellerComment;
            }
            set
            {
                if (Index >= 0)
                {
                    if ((int)State < 3 && ((int)State >= 0 || string.IsNullOrEmpty(_sellerComment)))
                    {
                        _sellerComment = value;
                        MarketInterface.Exec("UPDATE BitTransaction SET SellerComment=? WHERE ID=?", new object[] { value, Index });
                        Update();
                    }
                    else
                    {
                        throw new Exception("You cannot comment or rate in this state");
                    }
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public Rating BuyerRating
        {
            get
            {
                return _buyerRating;
            }
            set
            {
                if (Index >= 0)
                {
                    _buyerRating = value;
                    MarketInterface.Exec("UPDATE BitTransaction SET BuyerRating=? WHERE ID=?", new object[] { (int)value, Index });
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public Rating SellerRating
        {
            get
            {
                return _sellerRating;
            }
            set
            {
                if (Index >= 0)
                {
                    _sellerRating = value;
                    MarketInterface.Exec("UPDATE BitTransaction SET SellerRating=? WHERE ID=?", new object[] { (int)value, Index });
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public DateTime TransactionTime
        {
            get
            {
                return _transactionTime;
            }
            set
            {
                if (Index >= 0)
                {
                    _transactionTime = value;
                    MarketInterface.Exec("UPDATE BitTransaction SET TransactionTime=? WHERE ID=?", new object[] { value, Index });
                }
                else
                {
                    throw new Exception("Set Offer ID first!");
                }
            }
        }

        public bool Expired
        {
            get
            {
                return _transactionTime < DateTime.Now.Subtract(new TimeSpan(EXP_DAYS, 0, 0, 0, 0));
            }
        }

        /// <summary>
        /// Creates a Transaction for an offer
        /// </summary>
        /// <param name="BO">BitOffer you want to buy</param>
        public BitTransaction(BitOffer BO)
        {
            Offer = BO.Index;
            AddressSeller = BO.Address;
            TransactionTime = DateTime.Now;

            _buyerComment = _sellerComment = _addrBuyer = null;
            _amount = 0;
        }

        public BitTransaction(int tIndex)
        {
            SQLRow[] SR = MarketInterface.ExecReader("SELECT * FROM BitTransaction WHERE ID=?", new object[] { tIndex });
            if (SR != null && SR.Length > 0)
            {
                Index = tIndex;
                _addrBuyer = SR[0].Values["AddressBuyer"].ToString();
                _addrSeller = SR[0].Values["AddressSeller"].ToString();
                _amount = (int)SR[0].Values["Amount"];
                _offer = (int)SR[0].Values["Offer"];
                _state = (TransactionState)(int)SR[0].Values["State"];
                _buyerComment = SR[0].Values["BuyerComment"].ToString();
                _sellerComment = SR[0].Values["SellerComment"].ToString();
                _buyerRating = (Rating)(int)SR[0].Values["BuyerRating"];
                _sellerRating = (Rating)(int)SR[0].Values["SellerRating"];
                _transactionTime = (DateTime)SR[0].Values["TransactionTime"];
            }
            else
            {
                throw new Exception("Invalid Index");
            }
        }

        public bool canRate(string BMA)
        {
            if (!Expired && (BMA == _addrBuyer || BMA == _addrSeller))
            {
                //Transaction is valid for comment
                if ((int)State >= 0 && (int)State < 3)
                {
                    return true;
                }
                if (State < 0)
                {
                    //Transaction was rejected.
                    //Can only comment if not done already
                    if (BMA == _addrBuyer && (string.IsNullOrEmpty(_buyerComment) || _buyerRating == Rating.Unrated))
                    {
                        return true;
                    }
                    if (BMA == _addrSeller && (string.IsNullOrEmpty(_sellerComment) || _sellerRating == Rating.Unrated))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Update()
        {
            if ((int)State < 3 && (int)State >= 0)
            {
                if (State == TransactionState.Completed && !string.IsNullOrEmpty(_buyerComment) && !string.IsNullOrEmpty(_sellerComment) && _buyerRating != Rating.Unrated && _sellerRating != Rating.Unrated)
                {
                    State = TransactionState.Commented;
                }
                else if (Expired)
                {
                    if (State == TransactionState.Completed || State == TransactionState.Confirmed)
                    {
                        //TODO: Reset transaction after expiration
                    }
                }
            }
        }

        public void Confirm(string BMA)
        {
            if (!Expired)
            {
                if (BMA == _addrBuyer)
                {
                    if (State == TransactionState.Confirmed)
                    {
                        State = TransactionState.Completed;
                        Update();
                    }
                    else
                    {
                        throw new Exception("Seller has not yet confirmed the transaction");
                    }
                }
                else if (BMA == _addrSeller)
                {
                    if (State == TransactionState.Neutral)
                    {
                        State = TransactionState.Confirmed;
                        Update();
                    }
                    else
                    {
                        throw new Exception("You cannot confirm this transaction. Is not in Neutral state.");
                    }
                }
                else
                {
                    throw new Exception("This is not your transaction");
                }
            }
            else
            {
                throw new Exception("This transaction has expired and can no longer be confirmed");
            }
        }

        public void Reject(string BMA)
        {
            if (!Expired)
            {
                if ((int)State < 2)
                {
                    if (BMA == _addrBuyer)
                    {
                        if (State == TransactionState.RejectedBySeller)
                        {
                            State = TransactionState.RejectedByBoth;
                        }
                        else
                        {
                            State = TransactionState.RejectedByBuyer;
                        }
                    }
                    else if (BMA == _addrSeller)
                    {
                        if (State == TransactionState.RejectedByBuyer)
                        {
                            State = TransactionState.RejectedByBoth;
                        }
                        else
                        {
                            State = TransactionState.RejectedBySeller;
                        }
                    }
                    else
                    {
                        throw new Exception("This is not your transaction");
                    }
                }
                else
                {
                    throw new Exception("You cannot reject a confirmed or completed transaction");
                }
            }
            else
            {
                throw new Exception("This transaction has expired and can no longer be rejected");
            }
        }

        public void Rate(string BMA, Rating Rate)
        {
            if (canRate(BMA))
            {
                if (BMA == _addrBuyer)
                {
                    BuyerRating = Rate;
                }
                else
                {
                    SellerRating = Rate;
                }
            }
            else
            {
                throw new Exception("You cannot rate this Transaction");
            }
        }

        public void Comment(string BMA, string Comment)
        {
            if (canRate(BMA))
            {
                if (BMA == _addrBuyer)
                {
                    BuyerComment = Comment;
                }
                else
                {
                    SellerComment = Comment;
                }
            }
            else
            {
                throw new Exception("You cannot comment on this Transaction");
            }
        }
    }
}
