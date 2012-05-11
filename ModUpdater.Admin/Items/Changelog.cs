//    File:        Changelog.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater.Admin.Items
{
    class Changelog
    {
        private List<Item> Items;

        public void Add(Mod m, string n)
        {
            Items.Add(new Item { Mod = m, Action = Action.Added, Notes = n });
        }
        public void Remove(Mod m, string n)
        {
            Items.Add(new Item { Mod = m, Action = Action.Removed, Notes = n });
        }
        public void Change(Mod m, string n)
        {
            Items.Add(new Item { Mod = m, Action = Action.Changed, Notes = n });
        }
        public string GetChangelog()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Item i in Items)
            {
                sb.AppendLine(i.ToString());
            }
            return sb.ToString();
        }
        struct Item
        {
            public Mod Mod;
            public Action Action;
            public string Notes;

            public override string ToString()
            {
                return Action.ToString() + " " + Mod.Name + ".  Notes: " + Notes;
            }
        }
        enum Action
        {
            Added,
            Removed,
            Changed
        }
    }
}
