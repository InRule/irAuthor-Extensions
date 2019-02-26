
using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DecisionTableImporter
{
    class Utility
    {
        public static string BrowseForPath()
        {
            // Configure open file dialog box
            var openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "Excel files";
            openFileDialog.DefaultExt = ".xl*";
            openFileDialog.Filter = "Excel Files (.xl*)|*.xl*";

            // Show open file dialog box
            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return "";
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static string GetSheetName()
        {
            //TODO: create a window to capture this based on the spreadsheet
            // hard coding for now to simplify
            return "Sheet1";
        }
    }
}
