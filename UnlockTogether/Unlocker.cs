using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Win32;

namespace UnlockTogether
{
    internal class Unlocker
    {
        List<EventRewardsContainer> EventRewards = new List<EventRewardsContainer>();

        internal Unlocker()
        {
            if (File.Exists("events.conf"))
            {
                var lines = File.ReadAllLines("events.conf");
                foreach(var line in lines)
                {
                    var split = line.Split('\t');
                    var name = split[0];
                    var items = split[1].Split(',');

                    EventRewards.Add(new EventRewardsContainer
                    {
                        Name = name,
                        Rewards = items
                    });
                }          
            } else
            {
                Console.WriteLine("[!!] events.conf is missing. Exiting...");
                Environment.Exit(0);
            }
        }

        internal void Init()
        {
            if(Process.GetProcessesByName("Steam").Length == 0)
            {
                Console.WriteLine("[!!] Please run Steam and log in to your account before using this program.");
                return;
            }
            if(Process.GetProcessesByName("FarmTogether").Length > 0)
            {
                Console.WriteLine("[!!] Please exit the game before using this program.");
                return;
            }
            Console.WriteLine("Make sure you don't run Farm Together before you're done with this program.");

            int steamId32 = GetSteamId32FromRegistry();
            string steamPath = GetSteamPath();

#if DEBUG
            Console.WriteLine("SteamID32: " + steamId32.ToString());
            Console.WriteLine("Steam path: " + steamPath);
#endif
            if(steamId32 != -1 && steamPath != null)
            {
                var steamIdStr = steamId32.ToString();
                var farmsPath = Path.Combine(steamPath, "userdata", steamIdStr, "673950", "remote", "farms.xml");
#if DEBUG
                Console.WriteLine(farmsPath);
#endif

                Console.WriteLine("Found SteamID32 and save file path.");
                if(!File.Exists(farmsPath))
                {
                    Console.WriteLine("[!!] Cannot find farms.xml save file at " + farmsPath + ".");
                    return;
                }
                Console.WriteLine("Found save file.");

                int selection = -1;
                int count = 0;

                Console.WriteLine("Please select the event to unlock your rewards. Enter 0 to exit.");
                foreach (var e in EventRewards)
                {
                    Console.WriteLine("\t[" + ++count + "] " + e.Name);
                }

                do
                {
                    string input = Console.ReadLine();
                    if(!int.TryParse(input, out selection))
                    {
                        Console.WriteLine("Please enter a proper number...");
                        selection = -1;
                        continue;
                    }
                    if(selection <= 0 || selection > EventRewards.Count)
                    {
                        Console.WriteLine("That does not exist.");
                        selection = -1;
                        continue;
                    }

                    var _event = EventRewards[selection - 1];

                    var farmsXml = new XmlDocument();
                    farmsXml.Load(farmsPath);
                    foreach(var item in _event.Rewards)
                    {
                        var ele = farmsXml.CreateElement("Reward");
                        ele.SetAttribute("id", item);
                        farmsXml.DocumentElement["Rewards"].AppendChild(ele.Clone());
                    }
                    farmsXml.Save(farmsPath);

                    Console.WriteLine("Unlocked rewards from " + _event.Name + " event for your character.");
                } while (selection != 0);
            } else
            {
                if(steamId32 == -1)
                {
                    Console.WriteLine("[!!] Cannot retrieve SteamID32 from the registry.");
                }
                if(steamPath == null)
                {
                    Console.WriteLine("[!!] Cannot retrieve Steam's path from the registry.");
                }
            }
        }

        internal static object GetHkcuValue(string Key, string Value)
        {
            var _key = Registry.CurrentUser.OpenSubKey(Key);
            if (_key != null)
            {
                var entry = _key.GetValue(Value);
                if (entry != null)
                {
                    return entry;
                }
            }

            return null;
        }

        public static int GetSteamId32FromRegistry()
        {
            var entry = GetHkcuValue(@"Software\Valve\Steam\ActiveProcess", "ActiveUser");

            if (entry != null)
            {
                return Convert.ToInt32(entry);
            }

            return -1;
        }

        public static string GetSteamPath()
        {
            var entry = GetHkcuValue(@"Software\Valve\Steam", "SteamPath");

            if (entry != null)
            {
                return (string)entry;
            }

            return null;
        }
    }
}
