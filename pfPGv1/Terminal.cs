using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pfPGv1
{
    class Terminal
    {
        private String Id;
        private String Type;
        private Int32 Valeur;
        public Terminal(String identificatuer)
        {
            ID = identificatuer;
            TYPE = "none";
        }
        public Terminal(String identificatuer, String type)
        {
            ID = identificatuer;
            TYPE = type;
        }

        public String ID
        {
            get
            {
                return this.Id;
            }
            set 
            {
                if (value[0] == '\'')
                {
                    this.Id = value.Replace('\'', ' ');
                    this.Id = this.Id.Trim();
                }
                else
                {
                    this.Id = value;
                }
            }

        }

        public String TYPE
        {
            get
            {
                return this.Type;
            }
            set
            {
                this.Type = value;
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
