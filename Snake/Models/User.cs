using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Models
{
    internal class User
    {
        int id;
        string nev;
        int pontszam;

        public User(int id, string nev, int pontszam)
        {
            this.Id = id;
            this.Nev = nev;
            this.Pontszam = pontszam;
        }

        public int Id { get => id; set => id = value; }
        public string Nev { get => nev; set => nev = value; }
        public int Pontszam { get => pontszam; set => pontszam = value; }
    }
}
