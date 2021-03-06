﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public abstract class Card
    {

        public string Name { get; set; }
        public string RealName { get; set; }
        public string CardType { get; set; }
        public string Lore { get; set; }

        public string Archetype { get; set; }
        public string Supports { get; set; }
        public string AntiSupports { get; set; }

        public bool OcgExists { get; set; }
        public bool TcgExists { get; set; }

        public string Img { get; set; }
        public string Url { get; set; }
        public string Passcode { get; set; }

        public string OcgStatus { get; set; }
        public string TcgAdvStatus { get; set; }
        public string TcgTrnStatus { get; set; }

        public bool HasEffect
        {
            get
            {

                if (this is SpellTrap)
                    return true;
                else
                {

                    var monster = this as Monster;

                    if (monster.Types.Contains("Effect"))
                        return true;
                    else
                        return false;

                }

            }
        }

    }
}
