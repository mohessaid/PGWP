using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pfPGv1
{
    class Production
    {
        private NonTerminal Mgp;
        private ArrayList Mdp;
        private String Evaluation;

        public Production(NonTerminal mgp)
        {
            MGP = mgp;
        }

        public Production(NonTerminal mgp, ArrayList mdp)
        {
            MGP = mgp;
            MDP = mdp;
        }

        public NonTerminal MGP
        {
            get
            {
                return this.Mgp;
            }
            set 
            {
                this.Mgp = value;
            }
        }

        public ArrayList MDP
        {
            get
            {
                return this.Mdp;
            }
            set
            {
                this.Mdp = value;
            }
        }

        public String EVALUATION
        {
            get
            {
                return this.Evaluation;
            }
            set
            {
                this.Evaluation = value;
            }
        }
    }
}
