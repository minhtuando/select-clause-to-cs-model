using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SelectClauseToCsModel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            try
            {
                InitializeComponent();
                StringBuilder sb = new StringBuilder();
                var selectClause = Clipboard.GetText();

                selectClause = selectClause.Replace(" from ", " FROM ")
                                            .Replace("[", "")
                                            .Replace("]", "")
                                            .Replace("\nfrom ", "\nFROM ");

                if (selectClause.Contains("\nFROM "))
                {
                    if (CountStringOccurrences(selectClause, "\nFROM ") == 1)
                    {
                        selectClause = selectClause.Substring(0, selectClause.IndexOf("\nFROM "));
                    }
                    else
                    {
                        selectClause = selectClause.Substring(0, selectClause.LastIndexOf("\nFROM "));
                    }
                }

                var fields = selectClause.Split(',');

                foreach (var col in fields)
                {
                    string fieldName = col;

                    string type = "int";


                    if (fieldName.ToLower().Contains("."))
                    {
                        fieldName = fieldName.Substring(fieldName.IndexOf(".") + 1);
                    }

                    if (fieldName.ToLower().Contains(" as "))
                    {
                        string[] aliasMapKeys = { " as ", " As ", " aS ", " AS " };

                        foreach (var key in aliasMapKeys)
                        {
                            if (fieldName.Contains(key))
                            {
                                fieldName = fieldName.Substring(fieldName.IndexOf(key) + 4);
                            }
                        }
                    }

                    fieldName = fieldName.Replace("select", "");
                    fieldName = fieldName.Replace("SELECT", "");
                    fieldName = fieldName.Trim();

                    if (fieldName.Contains(" "))
                    {
                        fieldName = fieldName.Substring(0, fieldName.IndexOf(" "));
                    }

                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        type = GetTypeOfField(fieldName);

                        sb.AppendLine($"public { type } { fieldName } {{ get; set; }}");
                    }
                }

                Clipboard.SetText(sb.ToString());

                //AutoClosingMessageBox.Show("The result has been copied to system clipboard.", "Success!", 3000);

            }
            catch (Exception e)
            {
                MessageBox.Show("Sorry, there was an error: " + e.Message, "Opps!");
            }
        }

        private string GetTypeOfField(string fieldName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("ConfigDataType.xml");

            var nodes = doc.DocumentElement.SelectNodes("/config/contains/mapData");
       
            for(int i = 0; i < nodes.Count; i++)
            {
                if (string.IsNullOrEmpty(nodes[i].InnerText.Trim()))
                {
                    continue;
                }
                string[] mappingKeys = nodes[i].InnerText.Split(',');
                
                foreach (var key in mappingKeys)
                {
                    if (fieldName.ToLower().Contains(key.Trim()))
                    {
                        return nodes[i].Attributes["type"]?.InnerText;
                    }
                }
            }

            nodes = doc.DocumentElement.SelectNodes("/config/startsWith/mapData");
            
            for (int i = 0; i < nodes.Count; i++)
            {
                string[] mappingKeys = nodes[i].InnerText.Split(',');

                foreach (var key in mappingKeys)
                {
                    if (fieldName.StartsWith(key))
                    {
                        return nodes[i].Attributes["type"]?.InnerText;
                    }
                }
            }

            //default:
            return "int";
        }

        private int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        private void dispose(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
