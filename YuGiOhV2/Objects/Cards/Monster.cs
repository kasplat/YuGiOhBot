﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2.Objects.Cards
{
    public class Monster : Card
    {

        public string Attribute { get; set; }
        public string Types { get; set; }
        public string Atk { get; set; }
        public string Def { get; set; }
        public int PendulumScale { get; set; } //I blame xyz pendulums
        public string Materials { get; set; }

    }
}
