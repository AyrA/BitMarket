using GenericHandler;
using System;
using System.Text;
using System.IO;

namespace SQL
{
    public class MySQL : GenericProcessor
    {
        public event GenericServerLogHandler GenericServerLog;

        /// <summary>
        /// This Plugin is always running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return true;
            }
        }

        public MySQL()
        {
            //NOOP
        }

        public GenericMessage Process(GenericMessage M)
        {
            switch (M.Command.ToUpper().Split(' ')[0])
            {
                case "MAIN":
                    parseMain(M);
                    break;
                case "OFFER":
                    parseOffer(M);
                    break;
                case "FILE":
                    parseFile(M);
                    break;
                case "CATEGORY":
                    parseCategory(M);
                    break;
                case "TRANSACTION":
                    parseTransaction(M);
                    break;
                case "RATING":
                    parseRating(M);
                    break;
                default:
                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid command", Server = M.Server });
                    break;
            }
            return M;
        }

        public void Start()
        {
            //NOOP
        }

        public void Config(bool GUI)
        {
            Console.WriteLine(@"Basic MySQL Server Interface
This Plugin does not has any configuration. See BitMarket.sql");
        }

        public void Stop()
        {
            //NOOP
        }

        public override string ToString()
        {
            return "MySQL Server";
        }

        private void parseRating(GenericMessage M)
        {
            //RATING <ADDR> <ACTION>
            //ADDR: Bitmessage Address
            //ACTION: GET
            string[] Parts = M.Command.Split(' ');
            if (Parts.Length == 3)
            {
                switch (Parts[2].ToUpper())
                {
                    case "GET":
                        StringBuilder Ratings = new StringBuilder();
                        SQLRow[] Rows;
                        Rows = MarketInterface.ExecReader("SELECT * FROM BitTransaction WHERE AddressBuyer=?", new object[] { Parts[1] });
                        foreach (SQLRow r in Rows)
                        {
                            Ratings.AppendFormat("{0}\t{1}\t{2}\t{3}\r\n", (int)r.Values["State"], r.Values["AddressSeller"].ToString(), (int)r.Values["SellerRating"], Base.B64enc(r.Values["SellerComment"].ToString()));
                        }
                        Rows = MarketInterface.ExecReader("SELECT * FROM BitTransaction WHERE AddressSeller=?", new object[] { Parts[1] });
                        foreach (SQLRow r in Rows)
                        {
                            Ratings.AppendFormat("{0}\t{1}\t{2}\t{3}\r\n", (int)r.Values["State"], r.Values["AddressBuyer"].ToString(), (int)r.Values["BuyerRating"], Base.B64enc(r.Values["BuyerComment"].ToString()));
                        }
                        sendMsg(new GenericMessage() { Receiver = M.Sender, Command = M.Command, RawContent = Ratings.ToString(), Server = M.Server });
                        break;
                    default:
                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "invalid RATING command", Server = M.Server });
                        break;
                }
            }
        }

        private void parseCategory(GenericMessage M)
        {
            //creating and removing categories is
            //not allowed right now
            sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "The category command is not available", Server = M.Server });
        }

        private void parseTransaction(GenericMessage M)
        {
            //TRANSACTION <ID> <ACTION>
            //ACTION: GET,CONFIRM,REJECT,COMMENT
            string[] Parts = M.Command.Split(' ');
            if (Parts.Length == 3)
            {
                int index = -1;
                if (int.TryParse(Parts[1], out index))
                {
                    BitTransaction BT = null;
                    try
                    {
                        //check if transaction exists and if user is member of it.
                        BT = new BitTransaction(index);
                        if (BT.AddressBuyer != M.Sender && BT.AddressSeller != M.Sender)
                        {
                            throw new Exception("CRAP!");
                        }
                    }
                    catch
                    {
                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "invalid transaction ID", Server = M.Server });
                        return;
                    }
                    switch (Parts[2].ToUpper())
                    {
                        case "GET":
                            sendMsg(new GenericMessage()
                            {
                                Receiver = M.Sender,
                                Command = M.Command,
                                RawContent = string.Format(@"AddressBuyer={0}
AddressSeller={1}
Amount={2}
Offer={3}
State={4}
BuyerComment={5}
BuyerRating={6}
SellerComment={7}
SellerRating={8}
TransactionTime={9}",
                                    BT.AddressBuyer, BT.AddressSeller,
                                    BT.Offer, (int)BT.State, BT.Amount,
                                    Base.B64enc(BT.BuyerComment),
                                    (int)BT.BuyerRating,
                                    Base.B64enc(BT.SellerComment),
                                    (int)BT.SellerRating,
                                    BT.TransactionTime.ToString(Base.DT_FORMAT)),
                                Server = M.Server
                            });
                            break;
                        case "CONFIRM":
                            try
                            {
                                BT.Confirm(M.Sender);
                            }
                            catch (Exception ex)
                            {
                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "cannot confirm transaction: " + ex.Message, Server = M.Server });
                                break;
                            }
                            GenericMessage Clone1 = M.Clone();
                            Clone1.Command = "TRANSACTION " + BT.Index.ToString() + " GET";
                            Clone1.Sender = BT.AddressSeller;
                            parseTransaction(Clone1);
                            Clone1.Sender = BT.AddressBuyer;
                            parseTransaction(Clone1);
                            break;
                        case "REJECT":
                            try
                            {
                                BT.Reject(M.Sender);
                            }
                            catch (Exception ex)
                            {
                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "cannot reject transaction: " + ex.Message, Server = M.Server });
                                break;
                            }
                            GenericMessage Clone2 = M.Clone();
                            Clone2.Command = "TRANSACTION " + BT.Index.ToString() + " GET";
                            Clone2.Sender = BT.AddressSeller;
                            parseTransaction(Clone2);
                            Clone2.Sender = BT.AddressBuyer;
                            parseTransaction(Clone2);
                            break;
                        case "COMMENT":
                            var NVC = Base.ParseContent(M.RawContent);
                            if (BT.canRate(M.Sender))
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(NVC["Comment"]))
                                    {
                                        BT.Comment(M.Sender, Base.B64dec(NVC["Comment"]));
                                    }
                                    if (!string.IsNullOrEmpty(NVC["Rating"]))
                                    {
                                        BT.Rate(M.Sender, (Rating)int.Parse(NVC["Rating"]));
                                    }
                                }
                                catch
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "The value for 'Comment' or 'Rating' is invalid", Server = M.Server });
                                }
                            }
                            else
                            {
                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "You cannot rate the given transaction", Server = M.Server });
                            }
                            break;
                        default:
                            sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "invalid TRANSACTION command", Server = M.Server });
                            break;

                    }
                }
            }
        }

        private void parseFile(GenericMessage M)
        {
            //FILE <ID> <ACTION>
            //ACTION: GET,SET,DEL
            string[] Parts = M.Command.Split(' ');
            if (Parts.Length == 3)
            {
                int index = -1;
                if (int.TryParse(Parts[1], out index))
                {
                    switch (Parts[2].ToUpper())
                    {
                        case "GET":
                            try
                            {
                                BitFile BF = new BitFile(index);
                                sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "FILE", RawContent = string.Format("ID={0}\nNAME={1}\nCONTENT={2}", BF.Index, BF.Name, A85(BF.Contents)), Server = M.Server });
                            }
                            catch
                            {
                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "File not found", Server = M.Server });
                            }
                            break;
                        case "SET":
                            if (index >= 0)
                            {
                                BitFile BF;
                                //edit File contents
                                try
                                {
                                    BF = new BitFile(index);
                                }
                                catch
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "File not found", Server = M.Server });
                                    break;
                                }
                                if (BF.Address == M.Sender)
                                {
                                    string Name = getValue(M.RawContent, "NAME");
                                    byte[] content = A85(M.RawContent);

                                    if (!string.IsNullOrEmpty(Name))
                                    {
                                        BF.Name = Name;
                                        if (BF.Name == Name)
                                        {
                                            sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "FILE", RawContent = "File renamed", Server = M.Server });
                                        }
                                        else
                                        {
                                            sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid file name", Server = M.Server });
                                        }
                                    }

                                    if (content != null && !Base.compare(BF.Contents, content))
                                    {
                                        BF.Contents = content;
                                        if (Base.compare(BF.Contents, content))
                                        {
                                            sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "FILE", RawContent = "File contents updated", Server = M.Server });
                                        }
                                        else
                                        {
                                            sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot update file contents", Server = M.Server });
                                        }
                                    }
                                    else
                                    {
                                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid or empty A85 encoding", Server = M.Server });
                                    }
                                }
                                else
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Access denied for" + M.Sender, Server = M.Server });
                                }
                            }
                            else
                            {
                                //new file
                                string Name = getValue(M.RawContent, "NAME");
                                byte[] content = A85(M.RawContent);

                                if (!string.IsNullOrEmpty(Name) && content != null)
                                {
                                    BitFile BF = new BitFile();
                                    BF.Name = Name;
                                    BF.Contents = content;
                                    sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "FILE", RawContent = string.Format("ID={0}\nNAME={1}", BF.Index, BF.Name), Server = M.Server });
                                }
                                else
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid File name or A85 contents", Server = M.Server });
                                }
                            }
                            break;
                        case "DEL":
                            if (index >= 0)
                            {
                                try
                                {
                                    BitFile BF = new BitFile(index);
                                    if (BF.Address == M.Sender)
                                    {
                                        BF.Kill();
                                        BF = null;
                                        sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "FILE", RawContent = "File deleted", Server = M.Server });
                                    }
                                    else
                                    {
                                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Access denied for " + M.Sender, Server = M.Server });
                                    }
                                }
                                catch
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "File not found", Server = M.Server });
                                }
                            }
                            else
                            {
                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "File not found", Server = M.Server });
                            }
                            break;
                    }
                }
            }
        }

        private void parseOffer(GenericMessage M)
        {
            //OFFER <ID> <ACTION>
            //ACTION: GET,SET,DEL,BUY

            string[] Parts = M.Command.Split(' ');
            if (Parts.Length == 3)
            {
                int index = -1;
                if (int.TryParse(Parts[1], out index))
                {
                    BitOffer BO;
                    switch (Parts[2].ToUpper())
                    {
                        case "GET":
                            try
                            {
                                BO = new BitOffer(index);
                                sendMsg(new GenericMessage()
                                {
                                    Receiver = M.Sender,
                                    Command = M.Command,
                                    RawContent = string.Format(@"Title={0}
Description={1}
Address={2}
Category={3}
Files={4}
Stock={5},{6}
PriceMap={7}
LastModify={8}
",
                                        BO.Title, A85(Encoding.UTF8.GetBytes(BO.Description)),
                                        BO.Address, BO.Category, BO.Files, BO.Stock,
                                        BO.Stock > -1 ? BO.Stock - getUnconfirmedStock(BO.Index) : -1,
                                        BO.Prices, BO.LastModify.ToString("dd.MM.yyyy hh:mm:ss")),
                                    Server = M.Server
                                });
                            }
                            catch
                            {
                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid Offer ID", Server = M.Server });
                            }
                            break;
                        case "SET":
                            if (index >= 0)
                            {
                                try
                                {
                                    BO = new BitOffer(index);
                                }
                                catch
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid Offer ID", Server = M.Server });
                                    break;
                                }
                                if (BO.Address == M.Sender)
                                {
                                    if (setOffer(BO, M))
                                    {
                                        if (BO.Category == -2)
                                        {
                                            sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "OFFER " + BO.Index.ToString() + " SET", RawContent = BO.Index.ToString(), Server = M.Server });
                                        }
                                        else
                                        {
                                            sendBroadcast(new GenericMessage() { Command = "OFFER " + BO.Index.ToString() + " SET", RawContent = BO.Index.ToString(), Server = M.Server });
                                        }
                                    }
                                }
                                else
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "This is not your offer. You cannot modify it", Server = M.Server });
                                }
                            }
                            else
                            {
                                //new offer
                                BO = new BitOffer();
                                if (setOffer(BO, M))
                                {
                                    if (BO.Category == -2)
                                    {
                                        sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "OFFER -1 SET", RawContent = BO.Index.ToString(), Server = M.Server });
                                    }
                                    else
                                    {
                                        sendBroadcast(new GenericMessage() { Command = "OFFER -1 SET", RawContent = BO.Index.ToString(), Server = M.Server });
                                    }
                                }
                            }
                            break;
                        case "DEL":
                            if (index >= 0)
                            {
                                try
                                {
                                    BO = new BitOffer(index);
                                }
                                catch
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid Offer ID", Server = M.Server });
                                    break;
                                }
                                if (BO.Address == M.Sender)
                                {
                                    bool hidden = BO.Category == -2;
                                    BO.Kill();
                                    if (hidden)
                                    {
                                        sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "OFFER " + index.ToString() + " DEL", RawContent = index.ToString(), Server = M.Server });
                                    }
                                    else
                                    {
                                        sendBroadcast(new GenericMessage() { Command = "OFFER " + index.ToString() + " DEL", RawContent = index.ToString(), Server = M.Server });
                                    }
                                }
                                else
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "This is not your offer. You cannot modify it", Server = M.Server });
                                }
                            }
                            break;
                        case "BUY":
                            if (index >= 0)
                            {
                                try
                                {
                                    BO = new BitOffer(index);
                                }
                                catch
                                {

                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid Offer ID", Server = M.Server });
                                    break;
                                }

                                if (BO.Address != M.Sender)
                                {
                                    var NVC = Base.ParseContent(M.RawContent);
                                    int amount, understock;
                                    //check if valid amount
                                    if (!string.IsNullOrEmpty(NVC["Amount"]) && int.TryParse(NVC["Amount"], out amount) && amount > 0)
                                    {
                                        //check or create underStock
                                        if (!string.IsNullOrEmpty(NVC["UnderStock"]))
                                        {
                                            if (!int.TryParse(NVC["UnderStock"], out understock) || understock <= 0 || understock > amount)
                                            {
                                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid UnderStock value", Server = M.Server });
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            understock = amount;
                                        }

                                        if (BO.CanBuy(amount, understock))
                                        {
                                            BitTransaction BT = BO.Buy(amount, understock);
                                            if (BT != null)
                                            {
                                                BT.AddressBuyer = M.Sender;
                                                sendMsg(new GenericMessage() { Receiver = M.Sender, Command = M.Command, RawContent = BT.Index.ToString(), Server = M.Server });
                                                sendMsg(new GenericMessage()
                                                {
                                                    Receiver = BT.AddressSeller,
                                                    Command = M.Command,
                                                    RawContent = string.Format(@"ID={0}
AddressBuyer={1}
Amount={2}
Understock={3}
Message={4}",
                                                        BT.Index.ToString(), BT.AddressBuyer,
                                                        amount, understock, NVC["Message"]),
                                                    Server = M.Server
                                                });
                                            }
                                            else
                                            {
                                                sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot buy the specific amount. Did you match the pricemap?", Server = M.Server });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid stock amount", Server = M.Server });
                                    }
                                }
                                else
                                {
                                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "You cannot buy your own stuff", Server = M.Server });
                                }
                            }
                            break;
                    }
                }
            }
        }

        private bool setOffer(BitOffer BO, GenericMessage M)
        {
            if (getValue(M.RawContent, "Title") != null)
            {
                try
                {
                    BO.Title = getValue(M.RawContent, "Title");
                }
                catch (Exception ex)
                {
                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot edit the offer: " + ex.Message, Server = M.Server });
                    return false;
                }
            }
            if (getValue(M.RawContent, "Description") != null)
            {
                try
                {
                    BO.Description = Encoding.UTF8.GetString(A85(getValue(M.RawContent, "Description")));
                }
                catch (Exception ex)
                {
                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot edit the offer: " + ex.Message, Server = M.Server });
                    return false;
                }
            }
            if (getValue(M.RawContent, "Category") != null)
            {
                int cat = 0;
                if (int.TryParse(getValue(M.RawContent, "Category"), out cat) && cat > -3 && cat != 0)
                {
                    try
                    {
                        BitCategory BC = new BitCategory(cat);
                        BO.Category = cat;
                    }
                    catch (Exception ex)
                    {

                        sendErr(new GenericMessage() { Sender = M.Sender, RawContent = "Cannot edit the offer: " + ex.Message, Server = M.Server });
                        return false;
                    }
                }
            }
            if (getValue(M.RawContent, "Files") != null)
            {
                string[] fIndexes = getValue(M.RawContent, "Files").Split(',');
                if (fIndexes.Length > 1 || !string.IsNullOrEmpty(fIndexes[0].ToString()))
                {

                    try
                    {
                        for (int i = 0; i < fIndexes.Length; i++)
                        {
                            int.Parse(fIndexes[i]);
                        }
                        BO.Files = getValue(M.RawContent, "Files");
                    }
                    catch (Exception ex)
                    {
                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot edit the offer: " + ex.Message, Server = M.Server });
                        return false;
                    }
                }
                else
                {
                }
            }
            if (getValue(M.RawContent, "Stock") != null)
            {
                try
                {
                    int sCount = int.Parse(getValue(M.RawContent, "Stock"));
                    if (sCount > -2)
                    {
                        BO.Stock = sCount;
                    }
                    else
                    {
                        throw new Exception("Invalid stock count");
                    }
                }
                catch (Exception ex)
                {
                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot edit the offer: " + ex.Message, Server = M.Server });
                    return false;
                }
            }
            if (getValue(M.RawContent, "PriceMap") != null)
            {
                try
                {
                    PriceMap PM = new PriceMap(getValue(M.RawContent, "PriceMap"));
                    BO.Prices = PM.ToString();
                }
                catch (Exception ex)
                {
                    sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Cannot edit the offer: " + ex.Message, Server = M.Server });
                    return false;
                }
            }
            return true;
        }

        private void parseMain(GenericMessage M)
        {
            //MAIN <ACTION>
            //ACTION: INFO,SETTINGS,CATLIST,OFFERLIST
            string[] Parts = M.Command.Split(' ');
            if (Parts.Length == 2)
            {
                switch (Parts[1].ToUpper())
                {
                    case "INFO":
                        if (File.Exists("INFO.TXT"))
                        {
                            sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "MAIN INFO", RawContent = "AyrAs BitMarket\n\n" + File.ReadAllText("INFO.TXT"), Server = M.Server });
                        }
                        else
                        {
                            sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "MAIN INFO", RawContent = "AyrAs BitMarket\n\nThis Market has no informations published yet.", Server = M.Server });
                        }
                        break;
                    case "SETTINGS":
                        sendMsg(new GenericMessage()
                        {
                            Receiver = M.Sender,
                            Command = "MAIN SETTINGS",
                            RawContent = string.Format(@"FILE.NAME=50{0}RAW
FILE.CONTENT=-1{0}A85
OFFER.NAME=50{0}RAW
OFFER.FILES=50{0}RAW
OFFER.PRICEMAP=200{0}RAW
OFFER.DESCRIPTION=2000{0}A85
OFFER.MESSAGE=-1{0}A85
TRANSACTION.COMMENT=200{0}B64", '\t'),
                            Server = M.Server
                        });
                        break;
                    case "CATLIST":
                        SQLRow[] SRc = MarketInterface.ExecReader("SELECT ID,Parent,Name FROM BitCategory ORDER BY Parent,ID");
                        StringBuilder SBc = new StringBuilder();
                        foreach (SQLRow R in SRc)
                        {
                            SBc.AppendFormat("{0},{1},{2}\n", R.Values["ID"], R.Values["Parent"], R.Values["Name"]);
                        }
                        SRc = null;
                        sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "MAIN CATLIST", RawContent = SBc.ToString().Trim(), Server = M.Server });
                        break;
                    case "OFFERLIST":
                        SQLRow[] SRo = MarketInterface.ExecReader("SELECT ID,Category,Stock,PriceMap,Title FROM BitOffer WHERE Category!=-2 ORDER BY LastModify DESC");
                        StringBuilder SBo = new StringBuilder();
                        foreach (SQLRow R in SRo)
                        {
                            SBo.AppendFormat("{0},{1},{2},{3},{4}\n", R.Values["ID"], R.Values["Category"], R.Values["Stock"], R.Values["PriceMap"], R.Values["Title"]);
                        }
                        SRo = null;
                        sendMsg(new GenericMessage() { Receiver = M.Sender, Command = "MAIN OFFERLIST", RawContent = SBo.ToString().Trim(), Server = M.Server });
                        break;
                    default:
                        sendErr(new GenericMessage() { Receiver = M.Sender, RawContent = "Invalid MAIN command", Server = M.Server });
                        break;
                }
            }
        }

        private void sendBroadcast(GenericMessage MSG)
        {
            //broadcast to all servers
            foreach (GenericServer GS in AddonManager.Servers)
            {
                try
                {
                    GS.Broadcast(MSG);
                }
                catch (Exception ex)
                {
                    GenericServerLog(this,GenericLogType.Error,false,"Server '" + GS.ToString() + "' could not send a Broadcast: " + ex.Message);
                }
            }
        }

        private void sendMsg(GenericMessage MSG)
        {
            MSG.Server.Send(MSG);
        }

        private void sendErr(GenericMessage MSG)
        {
            MSG.Command = "ERROR";
            sendMsg(MSG);
        }

        private static string A85(byte[] source)
        {
            if (source != null)
            {
                return new Ascii85() { LineLength = 0 }.Encode(source);
            }
            return null;
        }

        private static byte[] A85(string source)
        {
            try
            {
                return new Ascii85() { LineLength = 0 }.Decode(source);
            }
            catch
            {
                return null;
            }
        }

        private static int getUnconfirmedStock(int ID)
        {
            var Rows = MarketInterface.ExecReader("SELECT SUM(Amount) AS Q FROM BitTransaction WHERE Offer=? AND State=0", new object[] { ID });
            return Rows[0].Values["Q"] == null ? 0 : (int)Rows[0].Values["Q"];
        }

        private static string getValue(string Message, string Field)
        {
            if (Message.ToLower().Contains("\n" + Field.ToLower() + "=") || Message.ToLower().StartsWith(Field + "="))
            {
                Message = Message.Substring(Message.ToLower().IndexOf(Field.ToLower()) + Field.Length + 1);
                //The last field probably has no \n terminator
                if (Message.Contains("\n"))
                {
                    Message = Message.Substring(0, Message.IndexOf('\n'));
                }
                return Message;
            }

            return null;
        }

    
    }
}
