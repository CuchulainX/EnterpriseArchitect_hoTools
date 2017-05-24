﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace hoTools.Utils.Diagram
{
    public class DiagramGeneralStyle
    {
        public string[] Style;
        public string[] Property;
        public string[] Type;
        public EA.Repository Rep;

        
        public DiagramGeneralStyle(EA.Repository rep, string type, string style, string property)
        {
            Style = style.Trim().Replace(",", ";").Replace(";;", ";").TrimEnd(';').Split(';');
            Property = property.Trim().Replace(",", ";").Replace(";;", ";").TrimEnd(';').Split(';');

            type = type.Trim().TrimEnd(';').Replace(";;", ";");
            Type = type.Split(';');
            Rep = rep;
        }

        /// <summary>
        /// Get value from string: Name=Value; extracts name and value. You can change the default delimiter "=" to whatever character you like
        /// </summary>
        /// <param name="myString"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        protected static bool GetNameValueFromString(string myString, out string name, out string value, char delimiter='=')
        {

            myString = myString.Trim();
            Regex rx = new Regex($@"([^=]*){delimiter}(.*)");
            Match match = rx.Match(myString.Trim());
            name = "";
            value = "";
            if (!match.Success && match.Groups.Count != 3)
            {
                MessageBox.Show($@"Tag: '{myString}'

Examples:
'LLB=HDN=1;'    Update tag HDN to 1 (hide) in Property LLB
'LLB=Set=;'     Reset LLB Property
'LLB=SET=HDN=1; Reset LLB Property to 'LLB=HDN=1';

Edit Setting.json:
- in %APPDATA%\ho\hoTools\Setting.json
- File, Setting

",$@"Invalid tag, has to be 'TagName=TagValue'!");
                return false;
            }
            name = match.Groups[1].Value;
            value = match.Groups[2].Value;
            return true;
        }
        /// <summary>
        /// Converts a string to integer. It also allows hexadecimal values (0xaaafffff), or the web format #ffffff.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="intValue"></param>
        /// <returns></returns>
        protected static bool ConvertInteger(string name, string value, out int intValue)
        {
            // ReSharper disable once RedundantAssignment
            intValue = 0;
            value = value.Trim().ToLower();
            // simple integer
            if (Int32.TryParse(value, out intValue)) return true;

            // more complex types like: Hexa, Color
            if (value.Length > 2)
            {
                if (value.Substring(0, 2) == "0x")
                    if (Int32.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber, null,
                        out intValue)) return true;
                if (value.Substring(0, 1) == "#")
                    if (Int32.TryParse(value.Substring(1), System.Globalization.NumberStyles.HexNumber, null,
                        out intValue)) return true;
                if (Int32.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out intValue)) return true;

                // handle colors like green
                string htmlColor = ColorHtmlFromName(value);
                if (htmlColor != "")
                {
                    return Int32.TryParse(htmlColor.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out intValue);

                }
            }


            MessageBox.Show($"Value '{value}' of Style/Property '{name}' must be Integer or Hexadecimal (e.g. 0xFA or #FA)",
                $"Invalid Integer/Hexa Style/Property in Settings.json");
            return false;
        }
        protected static bool ConvertBool(string name, string value, out bool boolValue)
        {
            // ReSharper disable once RedundantAssignment
            boolValue = false;
            if (Boolean.TryParse(value.Trim(), out boolValue)) return true;
            MessageBox.Show($"Value '{value}' of Style/Property '{name}' must be Boolean",
                $"Invalid Boolean Style/Property in Settings.json");
            return false;
        }

        /// <summary>
        /// Update local Styles (standard)
        /// </summary>
        /// <param name="oldStyle"></param>
        /// <param name="withInsert"></param>
        /// <returns></returns>
        protected string UpdateStyles(string oldStyle, bool withInsert=true)
        {
            return UpdateStyles(oldStyle, Style, withInsert);
           
        }

        /// <summary>
        /// Update arbitrary Style (used for e.g. diagram style which has an additional PDATA style
        /// </summary>
        /// <param name="oldStyle"></param>
        /// <param name="newStyles"></param>
        /// <param name="withInsert"></param>
        /// <returns></returns>
        protected string UpdateStyles(string oldStyle, string[] newStyles, bool withInsert = true)
        {
            string newStyle = oldStyle;
            foreach (var s in newStyles)
            {
                string style = s.Trim();
                string name;
                string value;
                if (!GetNameValueFromString(style, out name, out value)) continue;
                // substitutes colors
                value = ColorIntegerFromName(value);

                if (newStyle.Contains($"{name}="))
                {
                    // update style 
                    newStyle = Regex.Replace(newStyle, $@"{name}=[^;]*;", $"{name}={value};");
                }
                else
                {
                    // insert style
                    if (withInsert) newStyle = $"{newStyle};{name}={value};";
                }
            }
            return newStyle.Replace(";;;", ";").Replace(";;;", ";").Replace(";;", ";");
        }

        /// <summary>
        /// Returns the hexadecimal color (html color) like '#FFEEDD' or 'red'). If the value is a simple integer nothing is done
        /// </summary>
        /// <param name="htmlColor"></param>
        /// <returns></returns>
        protected static string ColorHtmlFromName(string htmlColor)
        {
            int dummy;
            if (int.TryParse(htmlColor, out dummy)) return htmlColor;
            var color = Color.FromName(htmlColor);
            if (color.IsEmpty) return htmlColor;
            return $@"#{color.B:X2}{color.G:X2}{color.R:X2}";
        }
        /// <summary>
        /// Returns the decimal color code from a (html color) like '#FFEEDD' or 'red'). If the value is a simple integer nothing is done
        /// </summary>
        /// <param name="htmlColor"></param>
        /// <returns></returns>
        protected static string ColorIntegerFromName(string htmlColor)
        {

            int dummy;
            if (int.TryParse(htmlColor, out dummy)) return htmlColor;
            var color = Color.FromName(htmlColor);
            if (color.IsEmpty) return htmlColor;
            return (color.B*(256*256) + color.G*256 + color.R).ToString();
        }




    }
}
