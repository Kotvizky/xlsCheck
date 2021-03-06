﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Data;
using System.Text.RegularExpressions;

namespace Check
{
    class Verifier
    {

        public Verifier(dynamic _schema, ITable iTable)
        {
            schema = _schema;

             ICheckTable = iTable;
//            ICheckTable.Open(schema[JOIN]);

            if (runMethod(JOIN, ICheckTable.Open))
            {
                runMethod(COMPARE, compere);
                runMethod(SELECT, select);
            }


            if (runMethod(OPEN, ICheckTable.Open))
            {
                runMethod(IDROWS, inRows);
                runMethod(PHONE, phone);
                runMethod(SMS_OBJ, sms);
                runMethod(REPLACE_REGEX, replaceRegex);
                runMethod(REDEX, redex);
                runMethod(ADDRESS, address);
                runMethod(PROPERTY, property);
            }
        }

        delegate void method(object[] fieldsList);

        bool runMethod(string name, method method)
        {

            if ((schema is Dictionary<string, object>) && (schema as Dictionary<string, object>).ContainsKey(name))
            {
                method(schema[name]);
                return true;
            }
            else
            {
                return false;
            }
        }

        ITable ICheckTable;

        public IRulesAction RuleAction { get; private set; }

        public DataTable CheckTable
        {
            get
            {
                return ICheckTable.table;
            }
        }

        const string COMPARE = "compare";
        const string JOIN = "join";
        const string OPEN = "open";
        const string SELECT = "select";
        const string PHONE = "phone";
        const string REDEX = "regex";
        const string IDROWS = "idRows";
        const string SMS_OBJ = "sms";
        const string ADDRESS = "address";
        const string PROPERTY = "property";
        const string REPLACE_REGEX = "replace regex";

        const string PHONE_ERRORS = "phone_errors";

        dynamic schema;

        public List<string> report { get; private set; } = new List<string>();

        void compere(object[] fieldsList)
        {
            int descPosition = 2;
            for (int i = 0; i < fieldsList.Count(); i++)
            {

                dynamic pair = fieldsList[i];

                if (IsNotArray(i, pair) || IsNotEnoughParam(i, descPosition, pair))
                {
                    continue;
                }

                string ruleName = $"\r\n[{COMPARE}]:\r\n{pair[0].ToString()};{pair[1].ToString()}";
                report.Add(ruleName);
                bool existReportFields = addFieldsToReport(pair, descPosition);
                string[] result = ICheckTable.Select(
                    new object[] { pair[0], pair[1] },
                    existReportFields ? pair[descPosition] : null
                    );
                report.AddRange(result);
                report.Add("----------");
            }
        }

        void select(object[] fieldsList)
        {
            for (int i = 0; i < fieldsList.Count(); i++)
            {

                dynamic pair = fieldsList[i];

                if (IsNotArray(i, pair) || IsNotEnoughParam(i, 1, pair))
                {
                    continue;
                }

                report.Add($"\r\n[{SELECT}]:\r\n{pair[0].ToString()}");

                bool existReportFields = addFieldsToReport(pair, 1);

                string[] result = ICheckTable.Select(
                    pair[0],
                    existReportFields ? pair[1] : null
                    );
                report.AddRange(result);
                report.Add("----------");
            }
        }

        void phone(object[] phoneObjects)
        {
            foreach (dynamic jsonObj in phoneObjects)
            {
                try
                {
                    dynamic phoneParam = ((Dictionary<string, object>)jsonObj)["split"];

                    List<string> fieldsList = getFieldsFromTable(phoneParam, "fieldMask", "fields");
                    if (fieldsList.Count == 0)
                        return;

                    try
                    {
                        System.Collections.Specialized.OrderedDictionary fields 
                            = new System.Collections.Specialized.OrderedDictionary();
                        foreach (string ColumnName in fieldsList)
                        {
                            fields.Add(ColumnName,null);
                        }
                        Phone.Fields = fields;
                        Phone.extraFieldsCount = 0;
                        Phone.separator = Array.ConvertAll<object, char>((phoneParam["symbols"] as object[]), x => Convert.ToChar(x));
                        report.Add($"<h2>Phone split</h2>");
                        DataTable table = ICheckTable.table;
                        DataColumn errorColumn;
                        errorColumn = (table.Columns.Contains(PHONE_ERRORS)) ?
                            table.Columns[PHONE_ERRORS] :
                            ICheckTable.table.Columns.Add(PHONE_ERRORS, typeof(string));
                        foreach (DataRow row in ICheckTable.table.Rows)
                        {
                            Phone.currentRow = row;
                            Phone.splitPhones();
                            if (Phone.report != String.Empty)
                            {
                                row[errorColumn] += Phone.report ;
                            }
                        }
                        Phone.placeFieldToEnd(table,PHONE_ERRORS);
                        report.Add($"<h2>Phone split</h2>");
                    }
                    catch (Exception e)
                    {
                        report.Add($"{e} -- {e.Message}");
                    }
                    break;
                }
                catch (Exception e)
                {
                    report.Add(e.Message);
                }
            }
        }

        void sms(object[] phoneObjects)
        {
            foreach (dynamic jsonObj in phoneObjects)
            {
                try
                {
                    dynamic phoneParam = ((Dictionary<string, object>)jsonObj)["expand"];
                    bool add_380 = false;
                    try
                    {
                        if ((int)((Dictionary<string, object>)jsonObj)["add_380"] == 1)
                        {
                            add_380 = true;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    SMS sms = new SMS(phoneParam, ICheckTable.table, report,add_380);
                    RuleAction = sms;
                    sms.fillPhoneLines();

                    break;
                }
                catch (Exception e)
                {
                    report.Add(e.Message);
                }
            }
        }

        void address(object[] addressObject)
        {
            Address.Init(ICheckTable.table, addressObject);

        }

        void property(object[] propertyObjects)
        {
            Property property = new Property(ICheckTable.table, propertyObjects);

        }



        void replaceRegex(object[] regxObjects)
        {

            List<string> fields = getFieldsFromTable(regxObjects[0], "fieldMask", "fields");

            object[] replacements = null;

            try
            {
                replacements = (((Dictionary<string, object>)regxObjects[0])["rules"] as object[]);
            }                
            catch (KeyNotFoundException e)
            {
            }

            if ((fields.Count == 0) || (replacements == null))
                return;

            List<ReplaceRegex> replaceRegList = new List<ReplaceRegex>();

            for (int i = 0; i < replacements.Length; i++)
            {
                if ((replacements[i] is object[]) && ((replacements[i] as object[]).Length == 3) ) {
                    string[] replaceParameters = Array.ConvertAll<object, string>((object[])replacements[i], x =>x.ToString());
                    replaceRegList.Add(new ReplaceRegex(replaceParameters));
                }
            }

            if (replaceRegList.Count == 0)
                return;

            report.Add($"<h2>{REPLACE_REGEX}</h2>");
            foreach (ReplaceRegex replaceRegex in replaceRegList)
            {
                report.Add($"<b>{replaceRegex.getParameters()}</b></br>");
            }
            report.Add($"</br>");

            foreach (DataRow row in ICheckTable.table.Rows)
            {
                foreach (string fieldName in fields)
                {
                    if ((row[fieldName] != DBNull.Value) && row[fieldName].ToString().Length > 0) {
                        string newValue = row[fieldName].ToString();
                        string oldValue = newValue;
                        foreach (ReplaceRegex replaceReg in replaceRegList)
                        {
                            newValue = replaceReg.Replace(newValue);
                        }
                        row[fieldName] = newValue;
                        if (oldValue != newValue)
                            report.Add($"{oldValue} -> {newValue} <br>");
                    }
                }
            }

        }

        void redex(object[] regxObjects) {
            try
            {
                foreach (dynamic jsonObj in regxObjects)
                {

                    DataTable table = ICheckTable.table;

                    List<string> fields = getFieldsFromTable(jsonObj, "fieldMask", "fields");

                    object[] isMatchJson = (((Dictionary<string, object>)jsonObj)["IsMatch"] as object[]);
                    object[] notMatchJson = (((Dictionary<string, object>)jsonObj)["NotMatch"] as object[]);
                    if ( (fields.Count() == 0) || (isMatchJson.Count() == 0) ) {
                        return;
                    }

                    htmlTitlePhone("Redex",fields.ToArray(),
                        $" isMatch: { string.Join(" ; ", isMatchJson)}, notMatch: { string.Join(" ; ", notMatchJson)}");

                    if (fields.Count == 0 ) 
                    {
                        return;
                    }

                    List<RegexInv> match = new List<RegexInv>();
                    foreach( string rule in isMatchJson )
                    {
                        match.Add(new RegexInv(rule,false));
                    }

                    foreach (string rule in notMatchJson)
                    {
                        match.Add(new RegexInv(rule,true));
                    }

                    DataColumn errorColumn = (table.Columns.Contains(PHONE_ERRORS)) ?
                        table.Columns[PHONE_ERRORS] :
                        ICheckTable.table.Columns.Add(PHONE_ERRORS, typeof(string));


                    for (int rowNumber = 0; rowNumber < table.Rows.Count; rowNumber++)
                    {
                        DataRow row = table.Rows[rowNumber];
                        string rowReport = String.Empty;
                        foreach (string field in fields)
                        {
                            if ((row[field] != DBNull.Value) && (row[field].ToString() != String.Empty))
                            {
                                string value = row[field].ToString();
                                List<RegexInv> errorRules = match.FindAll(x => x.ExMatch(value));

                                if (errorRules.Count > 0)
                                {
                                    string[] errors = Array.ConvertAll<object, string>(errorRules.ToArray(), x => x.ToString());
                                    //rowReport += $"<b>{value}</b> -- {string.Join(", ", errors)}; ";
                                    rowReport += $"[{value}] -- {string.Join(", ", errors)}\t\t";
                                }
                            }
                        }
                        if (rowReport != string.Empty)
                        {
                            //report.Add($"# {rowNumber + 2}. {rowReport.Remove(rowReport.Length-2)} <br>");
                            row[errorColumn] =rowReport.Remove(rowReport.Length - 2);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                report.Add(e.Message);
            }
        }

        List<string> getFieldsFromTable(dynamic jsonObj, string mask, string names)
        {
            List<string> fields = new List<string>();
            DataTable table = ICheckTable.table;
            if (mask != string.Empty)
            {
                try
                {
                    object[] fieldsMaskJson = (((Dictionary<string, object>)jsonObj)[mask] as object[]);
                    foreach (string field in fieldsMaskJson)
                    {
                        foreach (DataColumn column in table.Columns)
                        {
                            if (column.ColumnName.Contains(field) && (!field.Contains(column.ColumnName)))
                                fields.Add(column.ColumnName);
                        }
                    }
                }
                catch (KeyNotFoundException e) {}
            }
            if (names != string.Empty)
            {
                try
                {
                    object[] fieldsJson = (((Dictionary<string, object>)jsonObj)[names] as object[]);
                    foreach (string field in fieldsJson)
                        if (table.Columns.Contains(field)) fields.Add(field);
                }
                catch (KeyNotFoundException e) { }
            }
            return fields;
        }

        void inRows(object[] field)
        {
            try
            {
                if ((field.Count() == 1) && (field[0].ToString() != string.Empty))
                {
                    string fieldName = field[0].ToString();
                    DataTable table = ICheckTable.table;
                    if (table.Columns.Contains(fieldName)) {
                        table.Columns.Remove(table.Columns[fieldName]);
                    }
                    DataColumn Col = table.Columns.Add(fieldName, typeof(Int32));
                    Col.SetOrdinal(0);
                    for (int i = 0; i < table.Rows.Count; i++ )
                    {
                        table.Rows[i][fieldName] = i + 2;
                    }
                }
            } 
            catch (Exception e)
            {
                report.Add($"<pre>{e.Message}</pre>");
            }
        }

        void htmlTitlePhone(string name, object[] fields, string comment) {
            report.Add($"<h2>{name}</h2> <p>{comment}<br>");
            report.Add($"fields: {string.Join(";", fields)}</p>");
            report.Add($"<button id =\"redex|{string.Join(";", fields)};{PHONE_ERRORS};\">Copy fields</button><hr>");
        }

        bool addFieldsToReport(dynamic rules, int position)
        {
            bool result = ((rules as object[]).Count() > position  )
                    && (rules[position] is object[]);
            if (result)
            {
                string[] strFields = (rules[position] as object[]).Select(x => x.ToString()).ToArray();
                report.Add(String.Join(";", strFields));
            }
            return result;
        }

        private bool IsNotEnoughParam(int i, int paramNumber, dynamic pair)
        {
            bool result = (pair as object[]).Count() < paramNumber;
            if (result)
            {
                report.Add($"\r\n!{i + 1} - not enough parameters");
            }
            return result;
        }

        private bool IsNotArray(int i, dynamic pair)
        {
            bool result = !(pair is object[]);
            if (result)
            {
                report.Add($"\r\n!{i + 1} - not array of field");
            }
            return result;
        }

        class RegexInv : Regex
        {
            public RegexInv(string expretion, bool inversion) : base(expretion)
            {
                this.inversion = inversion;
            }
            public bool inversion { get; private set; }

            public bool ExMatch(string value)
            {
                return inversion ^ IsMatch(value);
            }

            public override string ToString()
            {
                string prefix = (inversion) ? "(-)" : "";
                return prefix + base.ToString();
            }

        };

        /// <summary>
        /// searchPattern = param[0]
        /// replacePattern = param[0]
        /// newValue = param[0]
        /// </summary>
        class ReplaceRegex {
            public ReplaceRegex(string[] param)
            {
                evaluator = new MatchEvaluator(replaceInPattern);
                this.searchPattern = param[0];
                this.replacePattern = param[1];
                this.newValue = param[2];
            }

            public string Replace(string search)
            {
                return Regex.Replace(search, searchPattern, 
                                evaluator,RegexOptions.IgnorePatternWhitespace);
            }

            string searchPattern;
            string replacePattern;
            string newValue;

            MatchEvaluator evaluator;

            string replaceInPattern(Match match)
            {
                string res = Regex.Replace(match.Value, replacePattern, newValue);

                return res;
            }

            public string getParameters()
            {
                return $"[{searchPattern}], [{replacePattern}], [{newValue}]";
            }

        }

    }
}

