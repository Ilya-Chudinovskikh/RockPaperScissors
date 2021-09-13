using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ConsoleTableExt;

namespace Zadanie_3
{
    class Program
    {
        static void Main(string[] moves)
        {
            if (CheckErrors(moves) != null)
                Console.WriteLine(CheckErrors(moves));
            else
            {
            start:
                string ai_turn = moves[new Random().Next(0, moves.Length - 1)];
                string key = Encryption.KeyHmacGeneration(moves, ai_turn);
                ShowMenu(moves);
                string input = Console.ReadLine();
                List<string> commands = new List<string>(GetCommands(moves));
                if (commands.Any(x => x == input))
                {
                    GameInfo(input, ai_turn, key, moves);
                    goto start;
                }
                else if (input == "0")
                    Console.WriteLine("You leaved the game.");
                else if (input == "?")
                {
                    ConsoleTableBuilder.From(Table.PrintTable(moves)).ExportAndWriteLine();
                    goto start;
                }

                else
                {
                    Console.WriteLine($"Error: please, enter a valid command, for example:\n1 or ?");
                    goto start;
                }
            }
        }
        public static string CheckErrors(string[] moves)
        {
            int len = moves.Length;
            string error = null;
            if (len < 2)
                error = $"Error: insufficient number of moves ({len}).\nThe number of moves has to be more than 2, for example:\n1 2 3 or Rock Paper Scissors";
            else if (len % 2 == 0)
                error = $"Error: the number of moves is even ({len}).\nThe number of moves has to be odd, for example:\n1 2 3 or Rock Paper Scissors";
            else if (moves.Distinct().Count() != len)
            {
                string rep = null;
                foreach (string s in moves.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList())
                    rep += s + ", ";
                error = $"Error: the names of moves are repeated ({rep.Substring(0, len - 2)}).\nMove names has to be unique, for example:\n1 2 3 or Rock Paper Scissors";
            }
            return error;
        }
        public static void ShowMenu(string[] moves)
        {
            Console.WriteLine("Available moves:");
            List<string> commands = new();
            for (int i = 1; i < moves.Length + 1; i++)
            {
                Console.WriteLine($"{i} - {moves[i - 1]}");
                commands.Add(i.ToString());
            }
            Console.WriteLine("0 - exit");
            Console.WriteLine("? - help");
            Console.WriteLine("Enter your move:");
        }
        public static void GameInfo(string input, string ai_turn, string key, string[] moves)
        {
            input = moves[Int32.Parse(input) - 1];
            Console.WriteLine($"Your move: {input}");
            Console.WriteLine($"Computer move: {ai_turn}");
            Console.WriteLine(Winner.Win(new Winner(ai_turn, input), moves));
            Console.WriteLine($"HMAC key: {key}");
        }
        public static List<string> GetCommands(string[] moves)
        {
            List<string> commands = new();
            for (int i = 1; i < moves.Length + 1; i++)
                commands.Add(i.ToString());
            return commands;
        }
    }
    public class Table
    {
        public static List<List<object>> PrintTable(string[] moves)
        {
            var tableData = new List<List<object>> { AddFirstRow(moves) };
            foreach (string move in moves)
            {
                List<object> row = new List<object>();
                row.Add(move);
                foreach (string cell in moves)
                {
                    row.Add(CheckWinner(move, cell, moves));
                }
                tableData.Add(new List<object>(row));
            }
            return tableData;
        }
        public static List<object> AddFirstRow(string[] moves)
        {
            List<object> first_row = new List<object> { @"PC \ User" };
            foreach (string move in moves)
                first_row.Add(move);
            return first_row;
        }
        public static string CheckWinner(string m, string c, string[] moves)
        {
            string who_won = Winner.Win(new Winner(m, c), moves);
            return who_won == "You win!" ? "WIN" : who_won == "You lose :(" ? "LOSE" : "DRAW";
        }
    }
    class Winner
    {
        public string Ai_t { get; set; }
        public string H_t { get; set; }
        public Winner(string a, string h) { Ai_t = a; H_t = h; }
        public static string Win(Winner w, string[] moves)
        {
            int am = Array.IndexOf(moves, w.Ai_t);
            int hm = Array.IndexOf(moves, w.H_t);
            int l = (moves.Length - 1) / 2;
            string result = am != hm ? (am > l ? (hm > am || am - hm > l ? "You win!" : "You lose :(") : hm > am && hm - am <= l ? "You win!" : "You lose :(") : "Draw! The friendship wins ;)";
            return result;
        }
    }
    public class Encryption
    {
        public static string GetKey()
        {
            byte[] key_bytes = new byte[16];
            RandomNumberGenerator key = RandomNumberGenerator.Create();
            key.GetBytes(key_bytes);
            return BitConverter.ToString(key_bytes).Replace("-", string.Empty).ToLower();
        }
        public static string GetHmac(string key_string, string move)
        {
            byte[] hmac_hash = new HMACSHA256(Encoding.UTF8.GetBytes(key_string)).ComputeHash(Encoding.UTF8.GetBytes(move));
            return BitConverter.ToString(hmac_hash).Replace("-", string.Empty).ToLower();
        }
        public static string KeyHmacGeneration(string[] moves, string ai_turn)
        {
            string key = Encryption.GetKey();
            Console.WriteLine($"HMAC:\n{Encryption.GetHmac(key, ai_turn)}");
            return key;
        }
    }
}