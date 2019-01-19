/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;

namespace Neurable.Core
{
    // Class for flexibly handling Metadata Interactions with the User
    public class NeurableMetadata
    {
        NeurableUser _user;
        System.Collections.Specialized.OrderedDictionary dictionary;

        public NeurableMetadata(NeurableUser user, string[] headerColumns) : base()
        {
            _user = user;
            dictionary = new System.Collections.Specialized.OrderedDictionary();
            SetHeader(headerColumns);
            Update();
        }

        // Adds a Column to the Header by creating an entry in the dictionary
        // Adding new Headers will wipe a file, take care when adding keys
        public void AddHeaderColumn(string columnTitle, bool updateNow)
        {
            if (columnTitle == "") return;
            Validate(ref columnTitle);
            if (dictionary.Contains(columnTitle)) return;
            dictionary[columnTitle] = "-1";
            newHeader = true;
            if (updateNow) UpdateHeader();
        }

        // Clears the current Metadata and Resets the Header with the given column titles
        public void SetHeader(string[] columnTitles)
        {
            if (columnTitles == null || columnTitles.Length <= 0) return;
            dictionary.Clear();
            foreach (var column in columnTitles)
            {
                AddHeaderColumn(column, false);
            }

            UpdateHeader();
        }

        // When new metadata keys are discovered, add to the header
        // stringify and validate input
        // update User Metadata when updateNow is true
        public void SetMetadata(string key, object value, bool updateNow = true)
        {
            string s_val = value.ToString();
            Validate(ref key);
            Validate(ref s_val);
            if (!dictionary.Contains(key))
            {
                AddHeaderColumn(key, true);
            }

            if (dictionary[key].ToString() != s_val)
            {
                dictionary[key] = s_val;
                newMetadata = true;
            }

            if (updateNow) UpdateMetadata();
        }

        // csv relies on precise comma spacing. validate replaces all commas
        void Validate(ref string input, char commaSubstitute = ' ')
        {
            if (commaSubstitute == ',')
            {
                Debug.LogError("Cannot replace commas with commas!");
                commaSubstitute = ' ';
            }

            while (input.Contains(","))
            {
                input = input.Replace(',', commaSubstitute);
            }
        }

        // Update Header for User (WIPES FILE)
        bool newHeader = false;

        protected void UpdateHeader()
        {
            if (!newHeader) return;
            if (_user != null && _user.User != null)
            {
                if (dictionary.Count > 0)
                {
                    string[] headers = new string[dictionary.Count];
                    dictionary.Keys.CopyTo(headers, 0);
                    string headerstring = string.Join(",", headers);
                    _user.User.SetMetaDataHeader(headerstring);
                }

                newHeader = false;
            }
        }

        // Update entire Metadata line
        bool newMetadata = false;

        protected void UpdateMetadata()
        {
            if (!newMetadata) return;
            if (_user != null && _user.User != null)
            {
                if (dictionary.Count > 0)
                {
                    string[] values = new string[dictionary.Count];
                    dictionary.Values.CopyTo(values, 0);
                    string data = string.Join(",", values);
                    _user.User.SetMetaData(data);
                }

                newMetadata = false;
            }
        }

        // Update all
        public void Update()
        {
            UpdateHeader();
            UpdateMetadata();
        }
    }
}
