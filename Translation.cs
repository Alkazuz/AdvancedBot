using AdvancedBot.client.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot
{
    class Translation
    {

        private static Dictionary<string, string> TranslationRules = new Dictionary<string, string>();

        public static void setupCommand(CommandBase cmdb)
        {

            String keyDesc = $"Command.{cmdb.DisplayName}.Description";
            if (getStringKey(keyDesc).Equals(keyDesc))
            {
                write(keyDesc + $"={cmdb.Description}");
            }
            else
            {
                cmdb.Description = getStringKey(keyDesc);
            }

        }

        private static void write(String n)
        {
            String filePath = System.IO.Directory.GetCurrentDirectory() + @"\languages\" + Program.Config.GetStringOrDefault("program-language", "en") + ".lang";

            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                try
                {
                    //Writing text to the file.
                    sw.WriteLine(n);
                    
                    //Close the file.
                    sw.Close();
                }
                catch (Exception ex) { write(n); }
            }
        }

        public static void setupContext(ContextMenuStrip context, Form form)
        {
            try
            {
                foreach (String cnd in Translation.getListFromForm(form))
                {
                    foreach (ToolStripMenuItem toolStripMenuItem in context.Items)
                    {
                        string[] splitted = cnd.Split('=');
                        String control = splitted[0].Split(new char[] { '.' })[1];

                        if (toolStripMenuItem.Name.Equals(control))
                        {
                            if (splitted[0].Split(new char[] { '.' })[2].Equals("Text"))
                            {
                                toolStripMenuItem.Text = splitted[1];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

            public static void setup(Form form)
        {
            foreach (String cnd in Translation.getListFromForm(form))
            {
                try
                {
                    string[] splitted = cnd.Split('=');
                    String control = splitted[0].Split(new char[] { '.' })[1];

                    
                    if (splitted[0].Split(new char[] { '.' })[0].Equals(form.Name) && splitted[0].Split(new char[] { '.' })[1].Equals("Text"))
                    {
                        form.Text = splitted[1];
                        continue;
                    }

                    List<Control> controls = new List<Control>();

                    foreach (MenuStrip menu in form.Controls.OfType<MenuStrip>())
                    {

                        foreach (ToolStripMenuItem tool in menu.Items)
                        {

                            foreach (ToolStripItem i in tool.DropDownItems)
                            {
                                if (i.Name.Equals(control))
                                {
                                    if (splitted[0].Split(new char[] { '.' })[2].Equals("Text"))
                                    {
                                        i.Text = splitted[1];
                                        break;
                                    }
                                }
                            }

                                if (tool.Name.Equals(control))
                            {
                                if (splitted[0].Split(new char[] { '.' })[2].Equals("Text"))
                                {
                                    tool.Text = splitted[1];
                                    break;
                                }
                            }
                        }
                    }

                        foreach (GroupBox groupBox in form.Controls.OfType<GroupBox>()) 
                    {
                        foreach (Control c in groupBox.Controls)
                        {
                            if (!controls.Contains(c))
                            {
                                controls.Add(c);
                            }
                        }
                    }

                    foreach (ContextMenuStrip c in form.Controls.OfType<ContextMenuStrip>())
                    {
                        Debug.WriteLine(c.Name);
                    }

                        foreach (Control c in form.Controls)
                    {
                        if (!controls.Contains(c))
                        {
                            controls.Add(c);
                        }
                    }


                    foreach (Control c in controls)
                    {

                         if (c.Name.Equals(control))
                         {
                             if (splitted[0].Split(new char[] { '.' })[2].Equals("Text"))
                              {
                                  c.Text = splitted[1];
                                  break;
                             }
                         }
                    }
                }
                catch { }
            }
        }

        public static List<String> getListFromForm(Form form)
        {
            List<String> n = new List<string>();

            String programPath = System.IO.Directory.GetCurrentDirectory();
            String filePath = programPath + @"\languages\" + Program.Config.GetStringOrDefault("program-language", "en") + ".lang";

            string line;
            Debug.WriteLine(form.Name);
            System.IO.StreamReader file =
                new System.IO.StreamReader(filePath, System.Text.Encoding.UTF8);
            while ((line = file.ReadLine()) != null)
            {

                if (!line.Contains("=")) continue;
                if (line.Length > 0)
                {
                    if (line.Contains("."))
                    {
                        string[] splitted = line.Split('.');
                        if (splitted[0].Equals(form.Name))
                        {
                            n.Add(line);
                        }
                    }
                }
            }
              return n;
        }

        public static String getStringKeyOrDefault(String key, String deault)
        {
            String filePath = System.IO.Directory.GetCurrentDirectory() + @"\languages\" + Program.Config.GetStringOrDefault("program-language", "en") + ".lang";
            if (!File.Exists(filePath))
            {
                return key;
            }
            if (TranslationRules.ContainsKey(key))
            {
                return TranslationRules[key].Replace("\\n", "\n").Replace("\\r", "\r");
            }

            string line;

            System.IO.StreamReader file =
                new System.IO.StreamReader(filePath, System.Text.Encoding.UTF8);
            while ((line = file.ReadLine()) != null)
            {

                if (!line.Contains("=")) continue;
                if (line.Length > 0)
                {
                    string[] splitted = line.Split('=');
                    if (splitted.Length == 2)
                    {
                        if (splitted[0].Equals(key))
                        {
                            file.Close();
                            TranslationRules[key] = splitted[1];
                            return splitted[1];
                        }


                    }
                }
            }

            file.Close();

            //write($"{key}={deault}");

            return deault;
        }

            public static String getStringKey(String key)
        {
            String filePath = System.IO.Directory.GetCurrentDirectory() + @"\languages\" + Program.Config.GetStringOrDefault("program-language", "en") + ".lang";
            if (!File.Exists(filePath))
            {
                return key;
            }
            if (TranslationRules.ContainsKey(key))
            {
                return TranslationRules[key].Replace("\\n", "\n").Replace("\\r", "\r");
            }

            string line;

            System.IO.StreamReader file =
                new System.IO.StreamReader(filePath, System.Text.Encoding.UTF8);
            while ((line = file.ReadLine()) != null)
            {
               
                if (!line.Contains("=")) continue;
                if (line.Length > 0)
                {
                    string[] splitted = line.Split('=');
                    if (splitted.Length == 2)
                    {
                        if (splitted[0].Equals(key))
                        {
                            file.Close();
                            TranslationRules[key] = splitted[1];
                            return splitted[1];
                        }
                       
                        
                    }
                }
            }

            file.Close();

            return key;
        }
    }
}
