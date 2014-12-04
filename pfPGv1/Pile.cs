using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pfPGv1
{
    public class Case
    {
        private String Id;
        private int Vale;
        private decimal Valer;

        public Case(String id)
        {
            this.Id = id;
            this.Vale = 0;
            this.Valer = 0;
        }
        public Case(String id, int vale, decimal valer)
        {
            this.Id = id;
            this.Vale = vale;
            this.Valer = valer;
        }

        public String ID { get { return this.Id; }  }
        public int VALE { get { return this.Vale; } set { this.VALE = value; } }
        public decimal VALER { get { return this.Valer; } set { this.VALER = value; } }
    }
    public class Pile
    {
        public List<String> Ps;
        public List<Case> Pc;

        public Pile( String daise, Boolean evaluation)
        {
            if (!evaluation)
            {
                this.Ps = new List<string>(0);
                this.Ps.Add("#");
            }
            else
            {
                this.Pc = new List<Case>(0);
                Case Daise = new Case("#");
                this.Pc.Add(Daise);
            }
        }
        
    }
}
