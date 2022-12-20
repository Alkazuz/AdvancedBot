using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace AdvancedBot
{
    internal class HWID
    {
        static internal Dictionary<String, Boolean> blacklist = new Dictionary<String, Boolean>();
        internal static Boolean allowed = false;

        internal void SetAllowed(Boolean value)
        {
            allowed = value;
        }

        internal Boolean isAllowed()
        {
            return allowed;
        }

        internal string cachedHwid = null;
        internal string getHWID()
        {
            if (cachedHwid != null) return cachedHwid;

            SHA1Managed sha1 = new SHA1Managed();
            string[] queries = new string[] { "SELECT SerialNumber FROM Win32_BIOS", //"SELECT SerialNumber FROM Win32_DiskDrive"
                                          "SELECT SerialNumber FROM Win32_BaseBoard",
                                          "SELECT ProcessorId FROM Win32_Processor" };
            foreach (string q in queries)
            {
                using (var mbs = new ManagementObjectSearcher(q))
                {
                    using (ManagementObjectCollection mbsList = mbs.Get())
                    {
                        foreach (ManagementObject mo in mbsList)
                        {
                            byte[] block = Encoding.UTF8.GetBytes(mo[q.Split(' ')[1]].ToString());
                            sha1.TransformBlock(block, 0, block.Length, null, 0);
                            mo.Dispose();
                        }
                    }
                }
            }
            sha1.TransformFinalBlock(new byte[0], 0, 0);
            return cachedHwid = BitConverter.ToString(sha1.Hash, 0, 12).Replace("-", "");
        }
        internal string getComputerUsername()
        {
            return Environment.UserName;
        }
    }
}
