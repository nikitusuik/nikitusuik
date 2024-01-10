﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using System.Data.SQLite;
using System.Data;
public class Calculator
{
    private static SQLiteConnection m_dbConn;
    private static SQLiteCommand m_sqlCmd;
    private static List<int> Mem = new List<int>();//Список 
    private int lastmem = -1;//последний номер операции когда число сохраняли в mem
    private char lastoperation = '+';//последняя операция
    public bool inputnumber = true;//true вводим число, false вводим операцию
    public List<int> GetMem()
    {
        return Mem;
    }
    public String GetMem(int id)//вычисленное значение по индексу
    {
        if (id == 0 && lastmem != -1 && Mem.Count > 0)
        {
            return Mem[lastmem].ToString();
        }
        else
        if (id >= 1 && id <= Mem.Count)
        {
            return Mem[id - 1].ToString();
        }
        else
        {
            return "";
        }
    }
    public String GetLastMem()//последнее вычисленное значение
    {
        return GetMem(0);
    }

    private static bool Create_SQLLite(ref String S)
    {
        if (!File.Exists("mem.sqlite"))
        {
            SQLiteConnection.CreateFile("mem.sqlite");
        }

        try
        {
            m_dbConn = new SQLiteConnection("Data Source=mem.sqlite;Version=3;");
            m_dbConn.Open();
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.Connection = m_dbConn;

            m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Memory (id INTEGER PRIMARY KEY AUTOINCREMENT, num INTEGER)";//создадим если не создана
            m_sqlCmd.ExecuteNonQuery();
            m_sqlCmd.CommandText = "DELETE FROM Memory";//Очистка
            m_sqlCmd.ExecuteNonQuery();
            return true;
        }
        catch (SQLiteException ex)
        {
            S = "Error: " + ex.Message;
            return false;
        }
    }
    private static bool Connect_SQLLite(ref String S)
    {
        if (!File.Exists("mem.sqlite"))
        {
            S = "No database mem.sqlite";
            return false;
        }
        try
        {
            m_dbConn = new SQLiteConnection("Data Source=mem.sqlite;Version=3;");
            m_dbConn.Open();
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.Connection = m_dbConn;
            return true;
        }
        catch (SQLiteException ex)
        {
            S = "Error: " + ex.Message;
            return false;
        }
    }
    private static bool Read_SQLLite(ref String S)
    {
        DataTable table = new DataTable();
        String sqlQuery;

        if (m_dbConn.State != ConnectionState.Open)
        {
            S = "Open connection with SQLLite database";
            return false;
        }
        Mem.Clear();
        try
        {
            sqlQuery = "SELECT * FROM Memory";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    Mem.Add(Convert.ToInt32(row["num"]));
                }
            }
            else
            {
                S = "Database is empty";
                return false;
            }
            return true;
        }
        catch (SQLiteException ex)
        {
            S = "Error: " + ex.Message;
            return false;
        }
    }

    private static bool Write_SQLlite(ref String S)
    {
        if (m_dbConn.State != ConnectionState.Open)
        {
            S = "Open connection with database";
            return false;
        }
        try
        {
            foreach (int num in Mem)
            {
                m_sqlCmd.CommandText = "INSERT INTO Memory ('num') values ('" + num.ToString() + "')";
                m_sqlCmd.ExecuteNonQuery();
            }
            return true;
        }
        catch (SQLiteException ex)
        {
            S = "Error: " + ex.Message;
            return false;
        }
    }
    public bool SaveSQLLite(ref String S)
    {
        if (Create_SQLLite(ref S))
        {
            return Write_SQLlite(ref S);
        }
        else
        {
            return false;
        }
    }
    public bool LoadSQLLite(ref String S)
    {
        if (Connect_SQLLite(ref S))
        {
            if (Read_SQLLite(ref S))
            {
                if (Mem.Count > 0)
                {
                    inputnumber = false;//вводим операцию
                    lastmem = Mem.Count - 1;
                    return true;
                }
                else
                {
                    inputnumber = true;//вводим число
                    lastmem = -1;
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        else
        {
            return false;
        }
    }
    public bool SaveXML()
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
            StreamWriter writer = new StreamWriter("mem.xml");
            serializer.Serialize(writer, Mem);
            writer.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public bool LoadXML()
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
            TextReader reader = new StreamReader("mem.xml");
            Mem = (List<int>)serializer.Deserialize(reader);
            reader.Close();
            if (Mem.Count > 0)
            {
                inputnumber = false;//вводим операцию
                lastmem = Mem.Count - 1;
                return true;
            }
            else
            {
                inputnumber = true;//вводим число
                lastmem = -1;
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
    public bool SaveJSON()
    {
        try
        {
            string json = JsonSerializer.Serialize(Mem);
            File.WriteAllText("mem.json", json);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public bool LoadJSON()
    {
        try
        {
            string data = File.ReadAllText("mem.json");
            Mem = JsonSerializer.Deserialize<List<int>>(data);
            if (Mem.Count > 0)
            {
                inputnumber = false;//вводим операцию
                lastmem = Mem.Count - 1;
                return true;
            }
            else
            {
                inputnumber = true;//вводим число
                lastmem = -1;
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public bool Calculate(String S)// S строка введённая пользователем
    {
        if (S == "ss")
        {
            if (SaveSQLLite(ref S))
            {
                Console.WriteLine("Saved to SQL Lite");
            }
            else
            {
                Console.WriteLine(S);
                return false;
            }
        }
        else if (S == "ls")
        {
            if (LoadSQLLite(ref S))
            {
                Console.WriteLine("Loaded " + Convert.ToString(Mem.Count) + " mem states!");
            }
            else
            {
                Console.WriteLine(S);
                return false;
            }

        }
        else if (S == "sj")
        {
            if (Mem.Count > 0)
            {
                if (SaveJSON())
                {
                    Console.WriteLine("Saved to JSON file");
                }
                else
                {
                    Console.WriteLine("Error! Saved to JSON file");
                    return false;
                }
            }
        }
        else if (S == "lj")
        {
            if (LoadJSON())
            {
                Console.WriteLine("Loaded " + Convert.ToString(Mem.Count) + " mem states!");
            }
            else
            {
                Console.WriteLine("Error! Loaded from JSON file");
                return false;
            }
        }
        else if (S == "sx")
        {
            if (Mem.Count > 0)
            {
                if (SaveXML())
                {
                    Console.WriteLine("Saved to XML file");
                }
                else
                {
                    Console.WriteLine("Error! Saved to XML file");
                    return false;
                }
            }
        }
        else if (S == "lx")
        {
            if (LoadXML())
            {
                Console.WriteLine("Loaded " + Convert.ToString(Mem.Count) + " mem states!");
            }
            else
            {
                Console.WriteLine("Error! Loaded from XML file");
                return false;
            }
        }
        else if (S == "+" || S == "-" || S == "/" || S == "*")
        {
            if (!inputnumber)
            {
                lastoperation = S[0];
                inputnumber = true;
            }
            else
            {
                Console.WriteLine("Error. You have to enter number!");
                return false;
            }
        }
        else if (S[0] == '#')
        {
            S = S.Substring(1);
            int num;
            if (int.TryParse(S, out num))
            {
                if (num >= 1 && num <= Mem.Count)
                {
                    lastmem = num - 1;
                    Console.WriteLine("[#" + (lastmem + 1).ToString() + "]=" + Mem[lastmem].ToString());
                    inputnumber = false;
                }
                else
                {
                    Console.WriteLine("Error. No mem state " + num.ToString());
                    return false;
                }
            }
        }
        else if (inputnumber)
        {
            int num;
            if (int.TryParse(S, out num))
            {
                if (lastmem == -1)
                {
                    lastmem++;
                    if (lastmem < Mem.Count)
                    {
                        Mem[lastmem] = num;
                    }
                    else
                    {
                        Mem.Add(num);
                    }
                    Console.WriteLine("[#" + (lastmem + 1).ToString() + "]=" + num.ToString());
                    inputnumber = false;
                }
                else
                {
                    switch (lastoperation)
                    {
                        case '+':
                            num = Mem[lastmem] + num;
                            lastmem++;
                            if (lastmem < Mem.Count)
                            {
                                Mem[lastmem] = num;
                            }
                            else
                            {
                                Mem.Add(num);
                            }
                            Console.WriteLine("[#" + (lastmem + 1).ToString() + "]=" + num.ToString());
                            inputnumber = false;
                            break;
                        case '-':
                            num = Mem[lastmem] - num;
                            lastmem++;
                            if (lastmem < Mem.Count)
                            {
                                Mem[lastmem] = num;
                            }
                            else
                            {
                                Mem.Add(num);
                            }
                            Console.WriteLine("[#" + (lastmem + 1).ToString() + "]=" + num.ToString());
                            inputnumber = false;
                            break;
                        case '*':
                            num = Mem[lastmem] * num;
                            lastmem++;
                            if (lastmem < Mem.Count)
                            {
                                Mem[lastmem] = num;
                            }
                            else
                            {
                                Mem.Add(num);
                            }
                            Console.WriteLine("[#" + (lastmem + 1).ToString() + "]=" + num.ToString());
                            inputnumber = false;
                            break;
                        case '/':
                            if (num == 0)
                            {
                                Console.WriteLine("Error. You have to enter not 0!");
                                return false;
                            }
                            else
                            {
                                num = Mem[lastmem] / num;
                                lastmem++;
                                if (lastmem < Mem.Count)
                                {
                                    Mem[lastmem] = num;
                                }
                                else
                                {
                                    Mem.Add(num);
                                }
                                Console.WriteLine("[#" + (lastmem + 1).ToString() + "]=" + num.ToString());
                                inputnumber = false;
                            }
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Error. You have to enter number!");
                return false;
            }
        }
        else
        {
            Console.WriteLine("Error. You have to enter operation!");
            return false;
        }
        return true;
    }

}
