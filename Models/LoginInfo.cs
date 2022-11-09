namespace incidents.Models
{
    public sealed class LoginInfo
    {
        //ISession session;
        //public LoginInfo(ISession session)
        //{
        //    this.session = session;
        //}
        private HttpContext ses;
        public LoginInfo()
        {
            this.username = "";
            this.uid = null;
        }
        public string username
        {
            get { return (ses.Session.GetString("username") ?? string.Empty).ToString(); }
            set { ses.Session.SetString("Username", value); }
        }
        //public string FullName
        //{
        //    get { return (this._session["FullName"] ?? string.Empty).ToString(); }
        //    set { this._session["FullName"] = value; }
        //}
        public String uid
        {
            get { return (ses.Session.GetString("uid") ?? string.Empty).ToString(); }
            set { ses.Session.SetString("uid", value); }
        }
        //public UserAccess AccessLevel
        //{
        //    get { return (UserAccess)(this._session["AccessLevel"]); }
        //    set { this._session["AccessLevel"] = value; }
        //}
    }
}
