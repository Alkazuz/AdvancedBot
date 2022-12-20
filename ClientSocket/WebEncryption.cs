namespace AdvancedBot.ClientSocket
{
    internal class WebEncryption
    {
        internal static string pass = "*t+=L[8JM75DElal";

        internal static string Encrypt(string text)
        {
            AES Aes = new AES();
            string EncryptedString = AES.EncryptAESToString(text, pass);
            // System.Web.HttpUtility.UrlEncode requires reference to System.Web.dll
            return System.Web.HttpUtility.UrlEncode(EncryptedString);
        }

        internal static string Decrypt(string text)
        {
            AES Aes = new AES();
            return AES.DecryptAESToString(text, pass);
        }
    }
}
