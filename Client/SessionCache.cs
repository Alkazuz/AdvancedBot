using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using AdvancedBot;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedBot.client
{
    public class SessionCache
    {
        private List<LoginResponse> users = new List<LoginResponse>();
        private const string FILENAME = "session_cache.json";

        public SessionCache()
        {
            if (File.Exists(FILENAME)) {
                using(JsonTextReader jr = new JsonTextReader(new StreamReader(FILENAME, Encoding.UTF8))) {
                    users = JsonSerializer.CreateDefault().Deserialize<List<LoginResponse>>(jr);
                }
            }
        }

        private int writeState = 0;
        public void Save()
        {
            if(Interlocked.CompareExchange(ref writeState, 1, 0) == 0) {
                long maxDif = new DateTime(1, 2, 1, 0, 0, 0).TotalMilliseconds();
                long now = DateTime.UtcNow.TotalMilliseconds();
                using (JsonTextWriter jw = new JsonTextWriter(new StreamWriter(FILENAME, false, Encoding.UTF8))) {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer.CreateDefault().Serialize(jw, users.Where(a => now - a.LastUse < maxDif).ToList());
                }
                Interlocked.Exchange(ref writeState, 0);
            }
        }

        public void Add(LoginResponse r)
        {
            for (int i = 0; i < users.Count; i++) {
                if (users[i].Username == r.Username) {
                    users.RemoveAt(i);
                    break;
                }
            }
            r.LastUse = DateTime.UtcNow.TotalMilliseconds();
            users.Add(r);
            Save();
        }
        private LoginResponse Get(string nameOrEmail)
        {
            for (int i = 0; i < users.Count; i++) {
                LoginResponse user = users[i];
                if (nameOrEmail == user.Email || nameOrEmail == user.Username) {
                    user.LastUse = DateTime.UtcNow.TotalMilliseconds();
                    return user;
                }
            }
            return null;
        }

        public LoginResponse Check(string email, Proxy p)
        {
            LoginResponse r = Get(email);
            if (r == null) return null;

            var jReq = new JObject(new JProperty("accessToken", r.AccessToken),
                                   new JProperty("clientToken", r.ClientToken));

            HttpConnection c = new HttpConnection(SessionUtils.REFRESH_URL);
            c.Headers["Content-Type"] = "application/json; charset=utf-8";
            c.Headers["Cache-Control"] = "no-cache";
            c.Headers["Pragma"] = "no-cache";
            c.Proxy = p;
            byte[] resp = c.Upload(Encoding.UTF8.GetBytes(jReq.ToString(Formatting.None)));
            JObject jResp = JObject.Parse(Encoding.UTF8.GetString(resp));
            if (jResp["error"] == null) {
                var profile = jResp["selectedProfile"];
                r = new LoginResponse {
                    Email = email,
                    ClientToken = jResp["clientToken"].AsStr(),
                    AccessToken = jResp["accessToken"].AsStr(),
                    UUID = profile["id"].AsStr(),
                    Username = profile["name"].AsStr()
                };

                Add(r);
                return r;
            }
            return null;
        }
        public async Task<LoginResponse> CheckAsync(string email, Proxy p)
        {
            LoginResponse r = Get(email);
            if (r == null) return null;

            var jReq = new JObject(new JProperty("accessToken", r.AccessToken),
                                   new JProperty("clientToken", r.ClientToken));

            var c = SessionUtils.CreateHttpConn(SessionUtils.REFRESH_URL, p);
            JObject jResp = JObject.Parse(await c.PostAsync(Encoding.UTF8.GetBytes(jReq.ToString(Formatting.None))).ConfigureAwait(false));
            if (jResp["error"] == null) {
                var profile = jResp["selectedProfile"];
                r = new LoginResponse {
                    Email = email,
                    ClientToken = jResp["clientToken"].AsStr(),
                    AccessToken = jResp["accessToken"].AsStr(),
                    UUID = profile["id"].AsStr(),
                    Username = profile["name"].AsStr()
                };
                Add(r);
                return r;
            }
            return null;
        }
    }
}
