using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AdvancedBot.client
{
    public class LoginResponse
    {
        public string Email;

        public string UUID;
        public string ClientToken;
        public string AccessToken;
        public string Username;

        public long LastUse;

        [JsonIgnore] public bool Error;
        [JsonIgnore] public Exception AuthError;
        public LoginResponse() { }
    }
}
