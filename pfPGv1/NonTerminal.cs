using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pfPGv1
{
    public class NonTerminal
    {
        private String Id;
        private Int32 Valeur;
        public NonTerminal(String identificatuer)
        {
            ID = identificatuer;
        }

        public String ID
        {
            get
            {
                return this.Id;
            }
            set 
            {
                this.Id = value;
            }

        }

        public Int32 VALEUR
        {
            get
            {
                return this.Valeur;
            }
            set
            {
                this.Valeur = value;
            }

        }
    }
}
