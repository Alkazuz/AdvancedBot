using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.client
{
    public class SessionUtils
    {
        public const string AUTH_URL = "https://authserver.mojang.com/authenticate";//"http://as0.craftlandia.com.br/authenticate"
        public const string REFRESH_URL = "https://authserver.mojang.com/refresh";
        public const string SESSION_URL = "https://sessionserver.mojang.com/session/minecraft/join";//"http://mcssv0.craftlandia.com.br/session/minecraft/join";

        private static SessionCache cache = new SessionCache();

        public static LoginResponse Login(string email, string password, Proxy p = null)
        {
            try {
                LoginResponse cr;
                if ((cr = cache.Check(email, p)) != null)
                    return cr;

                Debug.WriteLine(email + " login using password");

                JObject jReq = new JObject(
                    new JProperty("agent", new JObject(
                        new JProperty("name", "Minecraft"),
                        new JProperty("version", 1))),
                    new JProperty("username", email),
                    new JProperty("password", password),
                    new JProperty("clientToken", Guid.NewGuid().ToString()));

                byte[] resp = CreateHttpConn(AUTH_URL, p).Upload(jReq.ToString(Formatting.None).UTF8Bytes());
                JObject jResp = JObject.Parse(Encoding.UTF8.GetString(resp));

                LoginResponse r = new LoginResponse();
                if (jResp["error"] == null) {
                    r.ClientToken = jResp["clientToken"].AsStr();
                    r.AccessToken = jResp["accessToken"].AsStr();

                    var profile = jResp["selectedProfile"];
                    r.UUID = profile["id"].AsStr();
                    r.Username = profile["name"].AsStr();
                    r.Email = email;
                    r.Error = false;
                    cache.Add(r);
                } else {
                    r.Error = true;
                }
                return r;
            } catch (Exception e) {
                return new LoginResponse() {
                    Error = true,
                    AuthError = e
                };
            }
        }
        public static void CheckSession(string uuid, string accessToken, string serverHash, Proxy p = null)
        {
            JObject jReq = new JObject {
                { "accessToken", accessToken },
                { "selectedProfile", uuid },
                { "serverId", serverHash }
            };
            CreateHttpConn(SESSION_URL, p).Upload(jReq.ToString(Formatting.None).UTF8Bytes());
        }

        public static async Task<LoginResponse> LoginAsync(string email, string password, Proxy p = null)
        {
            try {
                LoginResponse cr;
                if ((cr = await cache.CheckAsync(email, p).ConfigureAwait(false)) != null)
                    return cr;

                Debug.WriteLine(email + " login using password");

                JObject jReq = new JObject(
                    new JProperty("agent", new JObject(
                        new JProperty("name", "Minecraft"),
                        new JProperty("version", 1))),
                    new JProperty("username", email),
                    new JProperty("password", password),
                    new JProperty("clientToken", Guid.NewGuid().ToString()));

                string resp = await CreateHttpConn(AUTH_URL, p).PostAsync(jReq.ToString(Formatting.None).UTF8Bytes()).ConfigureAwait(false);
                JObject jResp = JObject.Parse(resp);

                LoginResponse r = new LoginResponse();
                if (jResp["error"] == null) {
                    r.ClientToken = jResp["clientToken"].AsStr();
                    r.AccessToken = jResp["accessToken"].AsStr();

                    var profile = jResp["selectedProfile"];
                    r.UUID = profile["id"].AsStr();
                    r.Username = profile["name"].AsStr();
                    r.Email = email;
                    r.Error = false;
                    cache.Add(r);
                } else {
                    r.Error = true;
                }
                return r;
            } catch (Exception e) {
                return new LoginResponse() {
                    Error = true,
                    AuthError = e
                };
            }
        }
        public static async Task CheckSessionAsync(string uuid, string accessToken, string serverHash, Proxy p = null)
        {
            JObject jReq = new JObject {
                { "accessToken", accessToken },
                { "selectedProfile", uuid },
                { "serverId", serverHash }
            };
            await CreateHttpConn(SESSION_URL, p).PostAsync(jReq.ToString(Formatting.None).UTF8Bytes()).ConfigureAwait(false);
        }

        public static HttpConnection CreateHttpConn(string url, Proxy p)
        {
            HttpConnection c = new HttpConnection(url);
            c.Headers["Content-Type"] = "application/json; charset=utf-8";
            c.Headers["Cache-Control"] = "no-cache";
            c.Headers["Pragma"] = "no-cache";
            c.Proxy = p;
            return c;
        }
    }
}
