using System;
using c = System.Console;
using System.Collections.Generic;
using System.Linq;

public class FunctionMinimizer
{
    // Quine–McCluskey algorithm
    public class QMC
    {
        private string[] vars; // variables
        private int var_count;

        private int[,] truth_table; // truth table where last column - F
        private bool[,] table; // table of cover

        private int const_count; // implicants count
        private List<string> const_list; // prime implicants list
        private SortedDictionary<int, List<string>> const_groups; // minterm table by number of "1" in binary representation
        private bool[] undisc; // uncovered implicants

        List<string> pastes = new List<string>(); // size 2 implicant table
        private SortedDictionary<int, List<string>> paste_groups; // groups of size 2 implicants
        private List<string> res = new List<string>(); // result


        //---------------------------------------------------------------------------------

        public QMC(string[] vars, bool t = true)
        {
            // Init
            this.vars = vars;
            var_count = vars.Length;
            const_groups = new SortedDictionary<int, List<string>>();
            paste_groups = new SortedDictionary<int, List<string>>();
            const_list = new List<string>();
            const_count = Convert.ToInt32(Math.Pow(2, var_count));
            truth_table = new int[const_count, var_count + 1];
            // fill truth table (F=0)
            for (int i = 0; i < const_count; i++)
            {
                string current_row = byte_form(i, var_count);
                for (int j = 0; j < var_count; j++) truth_table[i, j] = Convert.ToInt32(current_row[j] - '0');
                truth_table[i, var_count] = 0;
            }
            if (t) show_truth_table();
        }

        // filling F column and update implicant list
        private void insert_f_values(int[] values) 
        {
            for (int i = 0; i < values.Length; i++) truth_table[i, var_count] = values[i];
            update_const_dict(); 
        }

        // GROUPING IMPLICANTS
        private bool consts_to_group()
        {
            foreach (string el in const_list) add_to_dict(const_groups, ones_in_number(from_2_to_10(el)), el);
            if (const_groups.Count == 0) // if no grouping available
            {
                for (int i = 0; i < const_count; i++) res.Add(byte_form(i, var_count));
                return false;
            }
            return true;
        }

        // SIZE 2 IMPLICANT TABLE FILLING
        private bool paste_consts()
        {
            int[] keys = const_groups.Keys.ToArray();
            for (int i = 0; i < keys.Length - 1; i++)
            {
                List<string> first = const_groups[keys[i]];
                List<string> second = const_groups[keys[i + 1]];
                for (int x = 0; x < first.Count; x++)
                {
                    for (int y = 0; y < second.Count; y++)
                    {
                        int index = compare(first[x], second[y]);
                        if (Array.IndexOf(new int[] { -1, -2 }, index) == -1)
                        {
                            string s = first[x];
                            s = s.Remove(index, 1).Insert(index, "X");
                            add_to_dict(paste_groups, index, s);
                        }

                    }
                }
            }
            if (paste_groups.Count == 0) // if no size 2 implicants
            {
                foreach (string el in const_list)
                {
                    res.Add(el);
                }

                return false;
            }

            return true;
        }

        // TABLE OF COVER
        private void coating()
        {
            // Init
            bool[] undisc = new bool[const_list.Count];
            const_list.Sort();
            foreach (KeyValuePair<int, List<string>> pair in paste_groups) foreach (string el in pair.Value) pastes.Add(el);
            table = new bool[pastes.Count, const_list.Count];
            pastes.Sort();
            for (int i = 0; i < undisc.Length; i++) undisc[i] = true;

            for (int i = 0; i < pastes.Count; i++) // filling table of cover
            {
                int index = pastes[i].IndexOf("X");
                for (int j = 0; j < const_list.Count; j++)
                {
                    if (compare(pastes[i], const_list[j]) == index) table[i, j] = true;
                    else table[i, j] = false;
                }
            }

            for (int j = 0; j < const_list.Count; j++) // finding cores and getting active rows
            {
                int count = 0;
                int ii = -1;
                int jj = -1;
                for (int i = 0; i < pastes.Count; i++)
                {
                    if (table[i, j])
                    {
                        count++;
                        ii = i;
                        jj = j;
                    }
                }
                if (count == 1)
                {
                    if (!res.Contains(pastes[ii])) res.Add(pastes[ii]);
                    undisc[jj] = false;
                    for (int x = 0; x < const_list.Count; x++) if (table[ii, x]) undisc[x] = false;
                }

            }

            for (int j = 0; j < undisc.Length; j++) // finding active rows from uncovered implicants
            {
                if (undisc[j])
                {
                    for (int i = 0; i < pastes.Count; i++)
                    {
                        if (table[i, j])
                        {
                            if (!res.Contains(pastes[i])) res.Add(pastes[i]);
                            undisc[j] = false;
                            for (int x = 0; x < const_list.Count; x++) if (table[i, x]) undisc[x] = false;
                        }
                    }
                }
            }

            for (int j = 0; j < undisc.Length; j++) if (undisc[j]) res.Add(const_list[j]); // still uncovered implicants are added manually
        }

        //---------------------------------------------------------------------------------

        // SUPPORT METHODS
        private int compare(string s1, string s2) // comparing to strings
        {
            // -2 => equal strings
            // -1 => differ > 1 symbol
            // index => differ = 1 symbol
            int count = 0;
            int index = -2;
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] != s2[i])
                {
                    count++;
                    if (count > 1) return -1;
                    else index = i;
                }
            }
            return index;
        }

        private void add_to_dict(SortedDictionary<int, List<string>> dict, int key, string value) // adding to dictionary
        {
            if (!dict.ContainsKey(key)) dict[key] = new List<string>();
            dict[key].Add(value);
        }

        private void update_const_dict() // finding prime implicants
        {
            const_list.Clear(); // clearing from previous input
            string s = "";
            for (int i = 0; i < const_count; i++)
            {
                if (truth_table[i, var_count] == 1)
                {
                    for (int j = 0; j < var_count; j++) s += Convert.ToString(truth_table[i, j]);
                    const_list.Add(s);
                    s = "";
                }
            }
        }

        private string byte_form(int a, int bytes) // 10cc => 2cc
        {
            return Convert.ToString(a, 2).PadLeft(bytes, '0');
        }

        private int from_2_to_10(string n) // 2cc => 10cc
        {
            int r = 0;
            for (int i = 0; i < n.Length; i++) if (n[i] == '1') r += Convert.ToInt32(Math.Pow(2, i));
            return r;
        }

        private int ones_in_number(int n) // ammount of "1" in binary form of number
        {
            int count;
            for (count = 0; n > 0; ++count) n &= (n - 1);
            return count;
        }

        //---------------------------------------------------------------------------------

        //OUTPUT METHODS

        private void fancy_print(string txt) // fancy text output
        {
            c.WriteLine("-----------------------------------");
            c.WriteLine(txt);
            c.WriteLine("-----------------------------------");
        }

        private void show_truth_table() // truth table output
        {
            for (int j = 0; j < var_count; j++) c.Write(vars[j] + "\t");
            c.WriteLine("F");
            c.WriteLine(new string('-', var_count * 10));
            for (int i = 0; i < const_count; i++)
            {
                for (int j = 0; j < var_count + 1; j++) c.Write(truth_table[i, j] + "\t");
                c.WriteLine("\n" + new string('-', var_count * 10));
            }
        }

        private void show_const_dict() // implicants groups output
        {
            fancy_print("Grouping implicants");
            foreach (KeyValuePair<int, List<string>> pair in const_groups)
            {
                c.WriteLine(pair.Key + ":");
                foreach (string el in pair.Value) c.Write(el + "\t");
                c.WriteLine();
            }
        }

        private void show_pasted_groups() // size-2 implicants output
        {
            fancy_print("Size-2 implicants");
            foreach (KeyValuePair<int, List<string>> pair in paste_groups)
            {
                string s = new String('_', var_count);
                int index = pair.Key;
                s = s.Remove(index, 1).Insert(index, "X");
                c.WriteLine(s + ":");
                foreach (string el in pair.Value) c.Write(el + "\t");
                c.WriteLine();
            }
        }

        private void show_table() // table of cover output
        {
            fancy_print("TABLE OF COVER");
            c.Write("\t");
            foreach (string el in const_list) c.Write(el + "\t");
            c.WriteLine();
            for (int i = 0; i < table.GetLength(0); i++)
            {
                c.Write(pastes[i] + "\t");
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    string s = "X";
                    if (table[i, j]) s = "V";
                    c.Write(s + "\t");
                }
                c.WriteLine();
            }
        }

        private string get_answer(bool write = true) // answer output
        {
            string s = "";
            foreach (string el in res)
            {
                s += "(";
                for (int i = 0; i < el.Length; i++)
                {
                    if (el[i] == 'X') continue;
                    if (el[i] == '1') s += vars[i];
                    else s += "¬" + vars[i];
                    s += " ^ ";
                }
                s = s.Remove(s.Length - 3, 3);
                s += ") V ";
            }
            s = s.Remove(s.Length - 3, 3);
            if (write)
            {
                fancy_print("Function");
                c.WriteLine(s);
            }
            return s;
        }

        //---------------------------------------------------------------------------------
        //CALLING METHOD

        public string Generate(int[] f_values, bool truth_table = false, bool const_groups = false, bool paste_groups = false, bool coating_table = false, bool answer = false)
        {
            insert_f_values(f_values);
            if (truth_table) show_truth_table();
            if (consts_to_group())
            {
                if (const_groups) show_const_dict();
                if (paste_consts())
                {
                    if (paste_groups) show_pasted_groups();
                    coating();
                    if (coating_table) show_table();
                }
                else
                {
                    if (paste_groups) c.WriteLine("No size-2 implicants");
                }
            }
            else
            {
                if (const_groups) c.WriteLine("No prime implicants");
            }
            if (answer) get_answer(true);
            return get_answer(false);
        }


    }
}
