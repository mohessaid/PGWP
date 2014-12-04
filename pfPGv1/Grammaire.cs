using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace pfPGv1
{
    public enum TypeID
    {
        T = 0,
        NT = 1
    }
    public struct Tuple
    {
        public String  ID;
        public TypeID  Type;
    }
    public enum Relation
    {
        EGA = 0,
        SUP = 1,
        INF = 2,
        NON = 3
    }
    class Grammaire
    {
        private String Source;
        private String Axiome;
        private List<String> Indexes;
        private List<NonTerminal> NonTerminaux;
        private List<Terminal> Terminaux;
        private List<Production> Productions;
        private Dictionary<String, List<Tuple>> Dernier;
        private Dictionary<String, List<Tuple>> Premier;
        private Dictionary<String, List<String>> Debut;
        private Dictionary<String, Dictionary<String, List<Relation>>> TableDeRelations;
        private Boolean isEval;
        private Boolean isInt;

        public Grammaire(String text)
        {
            this.Source = @text;
            
            this.NonTerminaux = new List<NonTerminal>();
            this.Terminaux = new List<Terminal>();
            this.Productions = new List<Production>();
            this.Dernier = new Dictionary<String, List<Tuple>>();
            this.Premier = new Dictionary<String, List<Tuple>>();
            this.Debut = new Dictionary<String, List<String>>();
            this.TableDeRelations = new Dictionary<string, Dictionary<string, List<Relation>>>();
            this.Indexes = new List<string>();
            this.isEval = false;
            this.isInt = false;
        }

        public Boolean ISEVAL { get { return this.isEval; } }
        public Boolean ISINT { get { return this.isInt; } }
        public String AXIOME { get { return this.Axiome; }}
        public List<String> INDEXES { get { return this.Indexes; } }
        public List<NonTerminal> NONTERMINAUX { get { return this.NonTerminaux; } }
        public List<Terminal> TERMINAUX { get { return this.Terminaux; } }
        public List<Production> PRODUCTIONS { get { return this.Productions; } }
        public Dictionary<String, List<Tuple>> DERNIER { get { return this.Dernier; } }
        public Dictionary<String, List<Tuple>> PREMIER { get { return this.Premier; } }
        public Dictionary<String, List<String>> DEBUT { get { return this.Debut; } }
        public Dictionary<String, Dictionary<String, List<Relation>>> TDR{get {return this.TableDeRelations;}}

        // Fonction pour l'extracation des terminaux
        public void TExtraction ()
        {
            TimeSpan timeoutregx = new TimeSpan(5000);
            Regex terminalstype = new Regex(@"[a-z][a-z0-9]*(\n|\s|\t)*:(\n|\s|\t)*(caractere|chiffre|entier|chaine|reel|\[""[\w]*""\])(\n|\s|\t)*;",
                                 RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex terminalID = new Regex(@"([a-z][a-z0-9]*)|£", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex terminalType = new Regex(@"(caractere|chiffre|entier|chaine|reel|\[""[\w]*""\])", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex terminals = new Regex(@"('[^']*')", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            MatchCollection termtype;
            MatchCollection term;
            MatchCollection termi;
            Boolean exist = false;
            Terminal value;

            @termtype = terminalstype.Matches(this.Source);
            @term = terminals.Matches(this.Source);
            @termi = terminalID.Matches(this.Source); 

            if (termtype.Count != 0)
            {
                foreach (Match i in termtype)
                {
                    exist = false;
                    foreach (Terminal j in TERMINAUX)
                    {
                        if ((j.ID == i.Value))
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                    {
                        if (@terminalType.Match(i.Value).Value == "entier")
                            this.isInt = true;
                        TERMINAUX.Add(new Terminal
                                                   (
                                                       @terminalID.Match(i.Value).Value,
                                                       @terminalType.Match(i.Value).Value
                                                   )
                                     );
                    }
                    
                }
            }
            if (term.Count != 0)
            {
                foreach (Match i in term)
                {
                    exist = false;
                    value = new Terminal(i.Value);
                    foreach (Terminal j in TERMINAUX)
                    {
                        if ((j.ID == value.ID))
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                        TERMINAUX.Add(new Terminal(i.Value));
                            
                    
                }
            }
            if (termi.Count != 0)
            {
                foreach (Match i in termi)
                {
                    exist = false;
                    if ((i.Value != "dsyntax") && (i.Value != "fsyntax"))
                    {
                        if (i.Value == "£")
                        {

                            foreach (Terminal j in TERMINAUX)
                            {
                                if ((j.ID == i.Value))
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                                TERMINAUX.Add(new Terminal
                                                                (
                                                                    @terminalID.Match(i.Value).Value
                                                                )
                                                  );
                        }
                    }

                }
            }
        }

        // Fonction pour l'extracation des non terminaux
        public void NTExtraction()
        {
            TimeSpan timeoutregx = new TimeSpan(5000);
            Regex NTerminalID = new Regex(@"[A-Z][A-Z0-9]*", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            MatchCollection ntermtype = NTerminalID.Matches(this.Source);
            List<String> ent = new List<string>();
            Int16 k = 0;
            Boolean exist = false;
            if (ntermtype.Count != 0)
            {
                foreach (Match i in ntermtype)
                {
                    exist = false;
                    if (k == 0)
                    {
                        k++;
                        ent.Add(i.Value);
                        NONTERMINAUX.Add(new NonTerminal(i.Value));

                    }
                    else
                    {
                        foreach (String j in ent)
                        {
                            if (i.Value.Equals(j))
                                exist = true;
                        }
                        if (!exist)
                        {
                            ent.Add(i.Value);
                            NONTERMINAUX.Add(new NonTerminal(i.Value));
                        }
                    }
                }
            }
            this.Axiome = NONTERMINAUX[0].ID;
        }

        // Fonction pour l'extracation des productions
        public void PExtraction()
        {
            TimeSpan timeoutregx = new TimeSpan(5000);
            Regex EnsPro = new Regex(@"[A-Z][A-Z0-9]*(\n|\s|\t)*:(\n|\s|\t)*(((([A-Z][A-Z0-9]*)|('[^']*')|£|([a-z][a-z0-9]*))(\n|\s|\t)*)+(\n|\s|\t)*({.*})?){1}
                                        (\n|\s|\t)*
                                        ((\|){1}(\n|\s|\t)*((([A-Z][A-Z0-9]*)|('[^']*')|£|([a-z][a-z0-9]*))(\n|\s|\t)*)+(\n|\s|\t)*({.*})?(\n|\s|\t)*)*(\n|\s|\t)*;
                                         (\n|\s|\t)*", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex ProMgp = new Regex(@"^[A-Z][A-Z0-9]*(\n|\s|\t)*:", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex ProMdps = new Regex(@"((([A-Z][A-Z0-9]*)|('[^']*')|£|([a-z][a-z0-9]*))(\n|\s|\t)*)+(\n|\s|\t)*({.*})?(\n|\s|\t)*((\|)|(;))"
                                      , RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex NT = new Regex(@"^[A-Z][A-Z0-9]*", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex TNT = new Regex(@"([a-z][a-z0-9]*)|([A-Z][A-Z0-9]*)|('[^']*')|£", RegexOptions.IgnorePatternWhitespace, timeoutregx);
            Regex Eval = new Regex(@"({.*})", RegexOptions.IgnorePatternWhitespace, timeoutregx);

            MatchCollection ensPro;
            Match ensProMgp;
            MatchCollection ensMdps;
            MatchCollection ensTNT;
            Match ensEval;

            @ensPro = EnsPro.Matches(this.Source);
            Production production;
            if (ensPro.Count != 0)
            {
                foreach (Match i in ensPro)
                {
                    @ensProMgp = NT.Match(ProMgp.Match(i.Value).Value);
                    @ensMdps = ProMdps.Matches(i.Value);

                    foreach (Match l in ensMdps)
                    {
                        production = new Production(new NonTerminal(ensProMgp.Value));
                        production.MDP = new ArrayList();
                        
                        ensEval = Eval.Match(l.Value);
                        if (ensEval.Value != "")
                        {
                            production.EVALUATION = ensEval.Value;
                            this.isEval = true;
                        }
                        @ensTNT = TNT.Matches(l.Value);
                        foreach (Match m in ensTNT)
                        {
                            if (m.Value != "")
                            {
                                if (Char.IsLower(m.Value[0]) || (m.Value == "£") || (m.Value[0].Equals('\'')))
                                {
                                    production.MDP.Add(new Terminal(m.Value));
                                }
                                else
                                {
                                    production.MDP.Add(new NonTerminal(m.Value));
                                }
                            }
                        }
                        PRODUCTIONS.Add(production);
                    }


                }
            }
        }

        public Boolean ExistTuple(List<Tuple> list, Tuple t)
        {
            foreach (Tuple i in list)
            {
                if (i.ID == t.ID)
                    return true;
            }
            return false;
        }
        public List<Tuple> Derniers(NonTerminal nt)
        {
            List<Tuple> prem = new List<Tuple>();
            Tuple elem = new Tuple();
            Boolean exist = false;
            foreach (Production i in PRODUCTIONS)
            {
                if (i.MGP.ID == nt.ID)
                {
                    if (i.MDP[i.MDP.Count - 1].GetType() == typeof(Terminal))
                    {
                        elem.ID = ((Terminal)i.MDP[i.MDP.Count - 1]).ID;
                        elem.Type = TypeID.T;
                        exist = false;
                        foreach (Tuple j in prem)
                        {
                            if (j.ID == elem.ID)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        elem.ID = ((NonTerminal)i.MDP[i.MDP.Count - 1]).ID;
                        elem.Type = TypeID.NT;
                        exist = false;
                        foreach (Tuple j in prem)
                        {
                            if (j.ID == elem.ID)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    if (!exist)
                        prem.Add(elem);
                }
            }
            return prem;
        }

        // Fonction pour le calcul de l'ensemble Dernier
        public void EDerCalcul()
        {
            int cpt;
            Boolean terminer, exist;
            List<Tuple> ders, der;
            Tuple elem = new Tuple();
            NonTerminal ter;

            foreach (NonTerminal i in NONTERMINAUX)
            {
                ders = new List<Tuple>();
                terminer = false;
                ders = Derniers(i);

                for (int j = 0; j < ders.Count; j++)
                {
                    exist = false;
                    if (ders[j].Type == TypeID.NT)
                    {
                        foreach (String s in DERNIER.Keys)
                        {
                            if (ders[j].ID == s)
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (!exist)
                        {
                            ter = new NonTerminal(ders[j].ID);
                            der = new List<Tuple>(Derniers(ter));
                            foreach (Tuple n in der)
                            {
                                foreach (Tuple nt in ders)
                                {
                                    if (n.ID == nt.ID)
                                    {
                                        terminer = true;
                                        break;
                                    }
                                    else
                                    {
                                        terminer = false;
                                    }
                                }

                                if (!terminer)
                                {
                                    
                                    ders.Add(n);
                                }
                            }
                        }
                        else
                        {
                            foreach (Tuple n in DERNIER[ders[j].ID])
                            {
                                foreach (Tuple nt in ders)
                                {
                                    if (n.ID == nt.ID)
                                    {
                                        terminer = true;
                                        break;
                                    }
                                    else
                                    {
                                        terminer = false;
                                    }
                                }

                                if (!terminer)
                                {
                                    ders.Add(n);
                                }
                            }
                        }
                    }
                }
                DERNIER.Add(i.ID, ders);

            }
            
        }

        public List<Tuple> Premiers(NonTerminal nt)
        {
            List<Tuple> prem = new List<Tuple>();
            Tuple elem = new Tuple();
            Boolean exist = false;
            foreach (Production i in PRODUCTIONS)
            {
                if (i.MGP.ID == nt.ID)
                {
                    if (i.MDP[0].GetType() == typeof(Terminal))
                    {
                        elem.ID = ((Terminal)i.MDP[0]).ID;
                        elem.Type = TypeID.T;
                        exist = false;
                        foreach (Tuple j in prem)
                        {
                            if (j.ID == elem.ID)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        elem.ID = ((NonTerminal)i.MDP[0]).ID;
                        elem.Type = TypeID.NT;
                        exist = false;
                        foreach (Tuple j in prem)
                        {
                            if (j.ID == elem.ID)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    if (!exist)
                        prem.Add(elem);
                }
            }
            return prem;
        }

        // Fonction pour le calcul de l'ensemble Premier
        public void EPCalcul()
        {
            int cpt;
            Boolean terminer, exist;
            List<Tuple> prems, prem;
            Tuple elem = new Tuple();
            NonTerminal ter;
            
            foreach(NonTerminal i in NONTERMINAUX)
            {
                prems = new List<Tuple>();
                terminer = false;
                prems = Premiers(i);

                for (int j = 0; j < prems.Count; j++)
                {
                    exist = false;
                    if (prems[j].Type == TypeID.NT)
                    {
                        foreach (String s in PREMIER.Keys)
                        {
                            if (prems[j].ID == s)
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (!exist)
                        {
                            ter = new NonTerminal(prems[j].ID);
                            prem = new List<Tuple>(Premiers(ter));
                            foreach (Tuple n in prem)
                            {
                                foreach (Tuple nt in prems)
                                {
                                    if (n.ID == nt.ID)
                                    {
                                        terminer = true;
                                        break;
                                    }
                                    else
                                    {
                                        terminer = false;
                                    }
                                }

                                if (!terminer)
                                {
                                    prems.Add(n);
                                }
                            }
                        }
                        else
                        {
                            foreach (Tuple n in PREMIER[prems[j].ID])
                            {
                                foreach (Tuple nt in prems)
                                {
                                    if (n.ID == nt.ID)
                                    {
                                        terminer = true;
                                        break;
                                    }
                                    else
                                    {
                                        terminer = false;
                                    }
                                }

                                if (!terminer)
                                {
                                    prems.Add(n);
                                }
                            }
                        }
                    }
                }
                PREMIER.Add(i.ID, prems);
                
            }
        }

        // Fonction pour le calcul de l'ensemble Debut
        public void EDebCalcul()
        {
            if (PREMIER.Count != 0)
            {
                List<String> deb;                                  
                foreach(NonTerminal i in NONTERMINAUX)
                {
                    deb = new List<String>();
                    foreach (Tuple prem in PREMIER[i.ID])
                    {
                        if (prem.Type == TypeID.T)
                        {
                            deb.Add(prem.ID);
                        }
                    }
                    DEBUT.Add(i.ID, deb);
                }
            }
        }

        public Production EpsilonProduction()
        {
            foreach (Production i in PRODUCTIONS)
            {
                foreach (Object j in i.MDP)
                {
                    if (j.GetType() == typeof(Terminal))
                    {
                        if (((Terminal)j).ID == "£")
                        {
                            return i;
                        }
                    }
                }
            }
            return null;
        }

        public String MDPIdentique()
        {
            String iden = "";
            List<String> travail = new List<String>();
            String s = "";
            foreach (Production i in PRODUCTIONS)
            {
                s = "";
                foreach (Object j in i.MDP)
                {
                    if (j.GetType() == typeof(Terminal))
                    {
                        s += ((Terminal)j).ID;
                    }
                    else if (j.GetType() == typeof(NonTerminal))
                    {
                        s += ((NonTerminal)j).ID;
                    }
                }
                travail.Add(s);
            }
            int cpt = 0;

            for (int i = 0, t = travail.Count; i <t ; i++)
            {
                cpt = 0;
                for (int j = i; j < t; j++)
                {
                    if (i != j)
                    {
                        if (travail[i] == travail[j])
                        {
                            foreach (Production p in PRODUCTIONS)
                            {
                                if ((cpt) == i)
                                {
                                    iden += "\n\t"+ p.MGP.ID + " -> " + travail[i] + "\n\t";
                                }
                                if ((cpt) == j)
                                {
                                    iden += p.MGP.ID + " -> " + travail[j] + "\n";
                                    return iden;
                                }
                                cpt++;
                            }
                        }
                    }
                }
            }
            return iden;
        }

        public Boolean ExistRel(List<Relation> list, Relation r)
        {
            foreach (Relation i in list)
            {
                if (r == i)
                {
                    return true;
                }
            }
            return false;
        }

        public void TRCalcul()
        {
            int dim = NONTERMINAUX.Count + TERMINAUX.Count + 1;
            List<String> index = new List<String>();
            foreach (NonTerminal i in NONTERMINAUX)
            {
                index.Add(i.ID);
                INDEXES.Add(i.ID);
            }
            foreach (Terminal i in TERMINAUX)
            {
                index.Add(i.ID);
                INDEXES.Add(i.ID);
            }
            index.Add("#");
            INDEXES.Add("#");
            for (int i = 0; i < dim; i++)
            {
                TDR.Add(index[i], new Dictionary<string, List<Relation>>());
                for (int j = 0; j < dim; j++)
                {
                    TDR[index[i]].Add(index[j], new List<Relation>());
                }
            }
            Terminal ter1 = null, ter2 = null;
            NonTerminal nter1 = null, nter2 = null;

            // Relation entre # et S
            foreach (Tuple prem in PREMIER[index[0]])
            {
                if (!ExistRel(TDR["#"][prem.ID], Relation.INF))
                    TDR["#"][prem.ID].Add(Relation.INF);
            }
            foreach (Tuple der in DERNIER[index[0]])
            {
                if (!ExistRel(TDR[der.ID]["#"], Relation.SUP))
                    TDR[der.ID]["#"].Add(Relation.SUP);
            }
            // Les relation
            foreach (Production p in PRODUCTIONS)
            {
                if (p.MDP.Count > 1)
                {
                    for (int i = 0, t = (p.MDP.Count - 1); i < t; i++)
                    {
                        ter1 = null; ter2 = null; nter1 = null; nter2 = null;
                        if (p.MDP[i].GetType() == typeof(Terminal))
                        {
                            ter1 = (Terminal)p.MDP[i];
                        }
                        else
                        {
                            nter1 = (NonTerminal)p.MDP[i];
                        }
                        if (p.MDP[i + 1].GetType() == typeof(Terminal))
                        {
                            ter2 = (Terminal)p.MDP[i + 1];
                        }
                        else
                        {
                            nter2 = (NonTerminal)p.MDP[i + 1];
                        }

                        // Les de s1 et S2
                        if ((ter1 != null) && (ter2 != null)) // S1 et S2 son des terminaux
                        {
                            if (!ExistRel(TDR[ter1.ID][ter2.ID], Relation.INF))
                                TDR[ter1.ID][ter2.ID].Add(Relation.INF);
                        }
                        else if ((ter1 != null) && (nter2 != null))   // S1 est un Terminal et S2 NT
                        {
                            if (!ExistRel(TDR[ter1.ID][nter2.ID], (Relation.INF)))
                                TDR[ter1.ID][nter2.ID].Add(Relation.INF);
                            foreach (Tuple prem in PREMIER[nter2.ID])
                            {
                                if (!ExistRel(TDR[ter1.ID][prem.ID], (Relation.INF)))
                                    TDR[ter1.ID][prem.ID].Add(Relation.INF);
                            }
                        }
                        else if ((nter1 != null) && (ter2 != null))  // S1 est un non terminal et S2 terminal
                        {
                            if (!ExistRel(TDR[nter1.ID][ter2.ID], (Relation.INF)))
                                TDR[nter1.ID][ter2.ID].Add(Relation.INF);
                            foreach (Tuple der in DERNIER[nter1.ID])
                            {
                                if (!ExistRel(TDR[der.ID][ter2.ID], (Relation.SUP)))
                                    TDR[der.ID][ter2.ID].Add(Relation.SUP);
                            }
                        }
                        else if ((nter1 != null) && (nter2 != null))  // S1 et S2 sont des terminaux
                        {
                            if (!ExistRel(TDR[nter1.ID][nter2.ID], (Relation.INF)))
                                TDR[nter1.ID][nter2.ID].Add(Relation.INF);
                            foreach (Tuple prem in PREMIER[nter2.ID])
                            {
                                if (!ExistRel(TDR[nter1.ID][prem.ID], (Relation.INF)))
                                    TDR[nter1.ID][prem.ID].Add(Relation.INF);
                            }
                            foreach (Tuple der in DERNIER[nter1.ID])
                            {
                                foreach (String deb in DEBUT[nter2.ID])
                                {
                                    if (!ExistRel(TDR[der.ID][deb], (Relation.SUP)))
                                        TDR[der.ID][deb].Add(Relation.SUP);
                                }
                            }
                        }

                    }
                } // Fin de if 
            }
        }

        public String VerificationRS()
        {
            List<Production> travail3 = new List<Production>();
            List<Production> travail1 = new List<Production>();
            String retour = "";
            Terminal ter1 = null, ter2 = null, ter3 = null, ter4 = null;
            NonTerminal nter1 = null, nter2 = null, nter3 = null, nter4 = null;
            int taille = 0;

            foreach (Production i in PRODUCTIONS)
            {
                if (i.MDP.Count > 1)
                {
                    travail3.Add(i);
                }
                if (i.MDP.Count == 1)
                {
                    travail1.Add(i);
                }
            }

            foreach (Production i in travail3)
            {
                ter1 = null; nter1 = null;
                taille = i.MDP.Count - 1;
                if (i.MDP[taille].GetType() == typeof(Terminal))
                {
                    ter1 = (Terminal)i.MDP[taille];
                }
                else
                {
                    nter1 = (NonTerminal)i.MDP[taille];
                }
                
                foreach (Production j in travail1)
                {
                    ter2 = null; nter2 = null;
                    if (j.MDP[0].GetType() == typeof(Terminal))
                    {
                        ter2 = (Terminal)j.MDP[0];
                    }
                    else
                    {
                        nter2 = (NonTerminal)j.MDP[0];
                    }

                    if ((ter1 != null)&&(ter2 != null))
                    {
                        if (ter1.ID == ter2.ID)
                        {
                            if (i.MDP[taille - 1].GetType() == typeof(Terminal))
                            {
                                ter3 = (Terminal)i.MDP[taille - 1];
                                if (TDR[ter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      ter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + ter1.ID;
                                }
                            }
                            else
                            {
                                nter3 = (NonTerminal)i.MDP[taille - 1];
                                if (TDR[nter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      nter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + ter1.ID;
                                }
                            }
                        }

                    }
                    else if ((ter1 != null) && (nter2 != null))
                    {
                        if (ter1.ID == nter2.ID)
                        {
                            if (i.MDP[taille - 1].GetType() == typeof(Terminal))
                            {
                                ter3 = (Terminal)i.MDP[taille - 1];
                                if (TDR[ter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      ter1.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + ter1.ID;
                                }
                            }
                            else
                            {
                                nter3 = (NonTerminal)i.MDP[taille - 1];
                                if (TDR[nter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      ter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + ter1.ID;
                                }
                            }
                        }
                    }
                    else if ((nter1 != null) && (ter2 != null))
                    {
                        if (nter1.ID == ter2.ID)
                        {
                            if (i.MDP[taille - 1].GetType() == typeof(Terminal))
                            {
                                ter3 = (Terminal)i.MDP[taille - 1];
                                if (TDR[ter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      nter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + nter1.ID;
                                }
                            }
                            else
                            {
                                nter3 = (NonTerminal)i.MDP[taille - 1];
                                if (TDR[nter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      nter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + nter1.ID;
                                }
                            }
                        }
                    }
                    else if ((nter1 != null) && (nter2 != null))
                    {
                        if (nter1.ID == nter2.ID)
                        {
                            if (i.MDP[taille - 1].GetType() == typeof(Terminal))
                            {
                                ter3 = (Terminal)i.MDP[taille - 1];
                                if (TDR[ter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      nter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + nter1.ID;
                                }
                            }
                            else
                            {
                                nter3 = (NonTerminal)i.MDP[taille - 1];
                                if (TDR[nter3.ID][j.MGP.ID].Count != 0)
                                {
                                    retour += "\n\t" + i.MGP.ID + " ->";
                                    for (int k = 0; k <= taille; k++)
                                    {
                                        if (i.MDP[k].GetType() == typeof(Terminal))
                                        {
                                            retour += " " + ((Terminal)i.MDP[k]).ID;
                                        }
                                        else
                                        {
                                            retour += " " + ((NonTerminal)i.MDP[k]).ID;
                                        }
                                    }
                                    retour += " ( Relation entre " +
                                                      nter3.ID + " et " + j.MGP.ID + " exist! )" + "\n\t" +
                                                      j.MGP.ID + " -> " + nter1.ID;
                                }
                            }
                        }
                    }
                }
            }

            return retour;

        }

        public Boolean PSource(String chaine)
        {
            List<Production> M = new List<Production>();
            List<Production> Travail = new List<Production>();
            Production p;
            Terminal ter =null;
            NonTerminal nter = null;
            Pile pile = new Pile("#", false);
            int sp = 0, tc = 0, etat = 0;
            Boolean erreur = false, accepter = false, finale = false, transite = true,
                    efinale = false;
            String mgp = "", mgpm = "" , mdp = "", mdpm = "";
            
            // Construction de AFD miroir des MDPs
            foreach (Production i in PRODUCTIONS)
            {
                p = new Production(i.MGP);
                p.MDP = new ArrayList();
                for (int j = (i.MDP.Count - 1); j >= 0; j--)
                {
                    p.MDP.Add(i.MDP[j]);
                }
                M.Add(p);
            } // Find de Construction de AFD miroir.
            #region ETIQ
            
                while ((!erreur) && (!accepter))
                {

                    if (TDR[pile.Ps[sp]][Convert.ToString(chaine[tc])].Count != 0)
                    {
                        // sp < tc
                        if (TDR[pile.Ps[sp]][Convert.ToString(chaine[tc])][0] == Relation.INF)
                        {
                            pile.Ps.Add(Convert.ToString(chaine[tc])); tc++; sp++;
                        }
                        else
                        {
                            // sp > tc
                            if (TDR[pile.Ps[sp]][Convert.ToString(chaine[tc])][0] == Relation.SUP)
                            {
                                etat = 0; finale = false; transite = true; efinale = false;
                                mgp = ""; mdp = ""; mdpm = "";
                                while ((transite) && (!efinale))
                                {
                                    mgpm = "";
                                    foreach (Production i in M)
                                    {
                                        efinale = true;
                                        if (etat > 0)
                                        {
                                            if ((etat < i.MDP.Count) &&
                                                ((i.MDP[etat - 1].GetType() == typeof(Terminal)) ?
                                                ((Terminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1]) :
                                                ((NonTerminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1])))
                                            {
                                                efinale = false;
                                                if (i.MDP[etat].GetType() == typeof(Terminal))
                                                {
                                                    ter = (Terminal)i.MDP[etat];
                                                    if (ter.ID == pile.Ps[sp])
                                                    {
                                                        transite = true;
                                                        mgpm = i.MGP.ID;
                                                        etat++;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    nter = (NonTerminal)i.MDP[etat];
                                                    if (nter.ID == pile.Ps[sp])
                                                    {
                                                        transite = true;
                                                        mgpm = i.MGP.ID;
                                                        etat++;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            efinale = false;
                                            if (i.MDP[etat].GetType() == typeof(Terminal))
                                            {
                                                ter = (Terminal)i.MDP[etat];
                                                if (ter.ID == pile.Ps[sp])
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                nter = (NonTerminal)i.MDP[etat];
                                                if (nter.ID == pile.Ps[sp])
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (transite && !efinale)
                                    {
                                        mdp += pile.Ps[sp];
                                        pile.Ps.RemoveAt(sp);
                                        sp--;
                                    }
                                }
                                finale = false;
                                foreach (Production i in M)
                                {
                                    if (i.MDP.Count == mdp.Length)
                                    {
                                        for (int j = 0; j < i.MDP.Count; j++)
                                        {
                                            if (i.MDP[j].GetType() == typeof(Terminal))
                                            {
                                                ter = (Terminal)i.MDP[j];
                                                mdpm += ter.ID;
                                            }
                                            else
                                            {
                                                nter = (NonTerminal)i.MDP[j];
                                                mdpm += nter.ID;
                                            }
                                        }
                                        if (mdp == mdpm)
                                        {
                                            finale = true;
                                            mgp = i.MGP.ID;
                                            break;
                                        }
                                        else
                                        {
                                            mgp = "";
                                            mdpm = "";
                                        }
                                    }
                                }
                                if (finale && (AXIOME == mgp) && (chaine[tc] == '#') && (pile.Ps[sp] == "#"))
                                {
                                    accepter = true;
                                }
                                else if (finale)
                                {
                                    pile.Ps.Add(mgp); sp++;
                                }


                            }
                            else
                            {
                                erreur = true;
                            }
                        }
                    }
                    else
                    {
                        erreur = true;
                    }
                
                }
            #endregion ETIQ

                if (accepter)
                {
                    return true;
                }
                else
                {
                    return false;
                }

        }


        public decimal[] decRemover(ref string input)
        {
            int n = 0;
            MatchCollection matches = Regex.Matches(input, @"\d+(\.\d+)?");
            decimal[] decimalarray = new decimal[matches.Count];
            int st = 0;
            foreach (Match m in matches)
            {
                decimalarray[n] = decimal.Parse(m.Value);
                st = input.IndexOf(m.Value);
                input = input.Remove(st, m.Value.Length);
                input = input.Insert(st, "i");
                n++;
            }

            return decimalarray;
        }
        public int[] intRemover(ref string input)
        {
            int n = 0;
            MatchCollection matches = Regex.Matches(input, @"\d+");
            int[] decimalarray = new int[matches.Count];
            int st = 0;
            foreach (Match m in matches)
            {
                decimalarray[n] = int.Parse(m.Value);
                st = input.IndexOf(m.Value);
                input = input.Remove(st, m.Value.Length);
                input = input.Insert(st, "i");
                n++;
            }

            return decimalarray;
        }
        // Evaluation avec des entier
        public Boolean EESource(String chaine)
        {
            int ind = 0;
            int[] valeurs = intRemover(ref chaine);
            List<Production> M = new List<Production>();
            List<Production> Travail = new List<Production>();
            Production p;
            Terminal ter = null;
            NonTerminal nter = null;
            Pile pile = new Pile("#", true);
            int sp = 0, tc = 0, etat = 0;
            Boolean erreur = false, accepter = false, finale = false, transite = true,
                    efinale = false;
            String mgp = "", mgpm = "", mdp = "", mdpm = "";
            Case elem;
            int val = 0, val1 = 0, val2 = 0, resultat = 0;

            // Construction de AFD miroir des MDPs
            foreach (Production i in PRODUCTIONS)
            {
                p = new Production(i.MGP);
                p.MDP = new ArrayList();
                for (int j = (i.MDP.Count - 1); j >= 0; j--)
                {
                    p.MDP.Add(i.MDP[j]);
                }
                M.Add(p);
            } // Find de Construction de AFD miroir.
            #region ETIQ

            while ((!erreur) && (!accepter))
            {

                if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])].Count != 0)
                {
                    // sp < tc
                    if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.INF)
                    {
                        if (chaine[tc] == 'i')
                        {
                            elem = new Case("i", valeurs[ind], 0);
                            //elem.VALE = valeurs[ind];
                            pile.Pc.Add(elem); tc++; sp++; ind++;
                        }
                        else
                        {
                            elem = new Case(Convert.ToString(chaine[tc]), 0, 0);
                            pile.Pc.Add(elem); tc++; sp++;
                        }
                    }
                    else
                    {
                        // sp > tc
                        if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.SUP)
                        {
                            etat = 0; finale = false; transite = true; efinale = false;
                            mgp = ""; mdp = ""; mdpm = "";
                            while ((transite) && (!efinale))
                            {
                                mgpm = "";
                                foreach (Production i in M)
                                {
                                    efinale = true;
                                    if (etat > 0)
                                    {
                                        if ((etat < i.MDP.Count) &&
                                            ((i.MDP[etat - 1].GetType() == typeof(Terminal)) ?
                                            ((Terminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1]) :
                                            ((NonTerminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1])))
                                        {
                                            efinale = false;
                                            if (i.MDP[etat].GetType() == typeof(Terminal))
                                            {
                                                ter = (Terminal)i.MDP[etat];
                                                if (ter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                nter = (NonTerminal)i.MDP[etat];
                                                if (nter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        efinale = false;
                                        if (i.MDP[etat].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[etat];
                                            if (ter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[etat];
                                            if (nter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (transite && !efinale)
                                {
                                    mdp += pile.Pc[sp].ID;
                                    if (pile.Pc[sp].ID == "i")
                                    {
                                        val = pile.Pc[sp].VALE;
                                    }
                                    else if (pile.Pc[sp].ID == "F")
                                    {
                                        val = pile.Pc[sp].VALE;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "T")
                                    {
                                        val = pile.Pc[sp].VALE;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "E")
                                    {
                                        val = pile.Pc[sp].VALE;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "+")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "-")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "*")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "/")
                                    {
                                        val2 = val;
                                    }
                                    pile.Pc.RemoveAt(sp);
                                    sp--;
                                }
                            }
                            finale = false;
                            foreach (Production i in M)
                            {
                                if (i.MDP.Count == mdp.Length)
                                {
                                    for (int j = 0; j < i.MDP.Count; j++)
                                    {
                                        if (i.MDP[j].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[j];
                                            mdpm += ter.ID;
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[j];
                                            mdpm += nter.ID;
                                        }
                                    }
                                    if (mdp == mdpm)
                                    {
                                        finale = true;
                                        mgp = i.MGP.ID;
                                        break;
                                    }
                                    else
                                    {
                                        mgp = "";
                                        mdpm = "";
                                    }
                                }
                            }
                            if (finale && (AXIOME == mgp) && (chaine[tc] == '#') && (pile.Pc[sp].ID == "#"))
                            {
                                accepter = true;
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                        resultat = val1 + val2;
                                    else if (mdp[1] == '-')
                                        resultat = val1 - val2;
                                }
                                System.Windows.MessageBox.Show(Convert.ToString(resultat), "resulat");

                            }
                            else if (finale)
                            {
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                    {
                                        resultat = val1 + val2;
                                    }
                                    else if (mdp[1] == '-')
                                    {
                                        resultat = val1 - val2;
                                    }
                                    else if (mdp[1] == '*')
                                    {
                                        resultat = val1 * val2;
                                    }
                                    else if (mdp[1] == '/')
                                    {
                                        resultat = val1 / val2;
                                    }
                                    elem = new Case(mgp, resultat, 0);
                                    //elem.VALE = resultat;
                                }
                                else
                                {
                                    elem = new Case(mgp, val, 0);
                                    //elem.VALE = val;
                                }
                                
                                pile.Pc.Add(elem); sp++;
                            }


                        }
                        else
                        {
                            erreur = true;
                        }
                    }
                }
                else
                {
                    erreur = true;
                }

            }
            #endregion ETIQ

            if (accepter)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public Boolean EESourceC(String chaine)
        {
            int ind = 0;
            int[] valeurs = intRemover(ref chaine);
            List<Production> M = new List<Production>();
            List<Production> Travail = new List<Production>();
            Production p;
            Terminal ter = null;
            NonTerminal nter = null;
            Pile pile = new Pile("#", true);
            int sp = 0, tc = 0, etat = 0;
            Boolean erreur = false, accepter = false, finale = false, transite = true,
                    efinale = false;
            String mgp = "", mgpm = "", mdp = "", mdpm = "";
            Case elem;
            int val = 0, val1 = 0, val2 = 0, resultat = 0;

            // Construction de AFD miroir des MDPs
            foreach (Production i in PRODUCTIONS)
            {
                p = new Production(i.MGP);
                p.MDP = new ArrayList();
                for (int j = (i.MDP.Count - 1); j >= 0; j--)
                {
                    p.MDP.Add(i.MDP[j]);
                }
                M.Add(p);
            } // Find de Construction de AFD miroir.
            #region ETIQ

            while ((!erreur) && (!accepter))
            {

                if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])].Count != 0)
                {
                    // sp < tc
                    if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.INF)
                    {
                        if (chaine[tc] == 'i')
                        {
                            elem = new Case("i", valeurs[ind], 0);
                            //elem.VALE = valeurs[ind];
                            pile.Pc.Add(elem); tc++; sp++; ind++;
                        }
                        else
                        {
                            elem = new Case(Convert.ToString(chaine[tc]), 0, 0);
                            pile.Pc.Add(elem); tc++; sp++;
                        }
                        Console.WriteLine();
                        foreach (Case caso in pile.Pc)
                        {
                            if (caso.ID != "#")
                            {
                                Console.Write(caso.ID + " [ " + Convert.ToString(caso.VALE) + " ] ");
                            }
                            else
                            {
                                Console.Write("#");
                            }
                        }
                    }
                    else
                    {
                        // sp > tc
                        if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.SUP)
                        {
                            etat = 0; finale = false; transite = true; efinale = false;
                            mgp = ""; mdp = ""; mdpm = "";
                            while ((transite) && (!efinale))
                            {
                                mgpm = "";
                                foreach (Production i in M)
                                {
                                    efinale = true;
                                    if (etat > 0)
                                    {
                                        if ((etat < i.MDP.Count) &&
                                            ((i.MDP[etat - 1].GetType() == typeof(Terminal)) ?
                                            ((Terminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1]) :
                                            ((NonTerminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1])))
                                        {
                                            efinale = false;
                                            if (i.MDP[etat].GetType() == typeof(Terminal))
                                            {
                                                ter = (Terminal)i.MDP[etat];
                                                if (ter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                nter = (NonTerminal)i.MDP[etat];
                                                if (nter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        efinale = false;
                                        if (i.MDP[etat].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[etat];
                                            if (ter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[etat];
                                            if (nter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (transite && !efinale)
                                {
                                    mdp += pile.Pc[sp].ID;
                                    if (pile.Pc[sp].ID == "i")
                                    {
                                        val = pile.Pc[sp].VALE;
                                    }
                                    else if (pile.Pc[sp].ID == "F")
                                    {
                                        val = pile.Pc[sp].VALE;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "T")
                                    {
                                        val = pile.Pc[sp].VALE;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "E")
                                    {
                                        val = pile.Pc[sp].VALE;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "+")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "-")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "*")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "/")
                                    {
                                        val2 = val;
                                    }
                                    pile.Pc.RemoveAt(sp);
                                    Console.WriteLine();
                                    foreach (Case caso in pile.Pc)
                                    {
                                        if (caso.ID != "#")
                                        {
                                            Console.Write(caso.ID + " [ " + Convert.ToString(caso.VALE) + " ] ");
                                        }
                                        else
                                        {
                                            Console.Write("#");
                                        }
                                    }
                                    sp--;
                                }
                            }
                            finale = false;
                            foreach (Production i in M)
                            {
                                if (i.MDP.Count == mdp.Length)
                                {
                                    for (int j = 0; j < i.MDP.Count; j++)
                                    {
                                        if (i.MDP[j].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[j];
                                            mdpm += ter.ID;
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[j];
                                            mdpm += nter.ID;
                                        }
                                    }
                                    if (mdp == mdpm)
                                    {
                                        finale = true;
                                        mgp = i.MGP.ID;
                                        break;
                                    }
                                    else
                                    {
                                        mgp = "";
                                        mdpm = "";
                                    }
                                }
                            }
                            if (finale && (AXIOME == mgp) && (chaine[tc] == '#') && (pile.Pc[sp].ID == "#"))
                            {
                                accepter = true;
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                        resultat = val1 + val2;
                                    else if (mdp[1] == '-')
                                        resultat = val1 - val2;
                                }
                                //System.Windows.MessageBox.Show(Convert.ToString(resultat), "resulat");

                            }
                            else if (finale)
                            {
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                    {
                                        resultat = val1 + val2;
                                    }
                                    else if (mdp[1] == '-')
                                    {
                                        resultat = val1 - val2;
                                    }
                                    else if (mdp[1] == '*')
                                    {
                                        resultat = val1 * val2;
                                    }
                                    else if (mdp[1] == '/')
                                    {
                                        resultat = val1 / val2;
                                    }
                                    elem = new Case(mgp, resultat, 0);
                                    //elem.VALE = resultat;
                                }
                                else
                                {
                                    elem = new Case(mgp, val, 0);
                                    //elem.VALE = val;
                                }

                                pile.Pc.Add(elem); sp++;
                                Console.WriteLine();
                                foreach (Case caso in pile.Pc)
                                {
                                    if (caso.ID != "#")
                                    {
                                        Console.Write(caso.ID + " [ " + Convert.ToString(caso.VALE) + " ] ");
                                    }
                                    else
                                    {
                                        Console.Write("#");
                                    }
                                }
                            }


                        }
                        else
                        {
                            erreur = true;
                        }
                    }
                }
                else
                {
                    erreur = true;
                }

            }
            #endregion ETIQ

            if (accepter)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public Boolean ERSource(String chaine)
        {
            int ind = 0;
            decimal[] valeurs = decRemover(ref chaine);
            List<Production> M = new List<Production>();
            List<Production> Travail = new List<Production>();
            Production p;
            Terminal ter = null;
            NonTerminal nter = null;
            Pile pile = new Pile("#", true);
            int sp = 0, tc = 0, etat = 0;
            Boolean erreur = false, accepter = false, finale = false, transite = true,
                    efinale = false;
            String mgp = "", mgpm = "", mdp = "", mdpm = "";
            Case elem;
            decimal val = 0, val1 = 0, val2 = 0, resultat = 0;

            // Construction de AFD miroir des MDPs
            foreach (Production i in PRODUCTIONS)
            {
                p = new Production(i.MGP);
                p.MDP = new ArrayList();
                for (int j = (i.MDP.Count - 1); j >= 0; j--)
                {
                    p.MDP.Add(i.MDP[j]);
                }
                M.Add(p);
            } // Find de Construction de AFD miroir.
            #region ETIQ

            while ((!erreur) && (!accepter))
            {

                if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])].Count != 0)
                {
                    // sp < tc
                    if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.INF)
                    {
                        if (chaine[tc] == 'i')
                        {
                            elem = new Case("i", 0, valeurs[ind]);
                            //elem.VALE = valeurs[ind];
                            pile.Pc.Add(elem); tc++; sp++; ind++;
                        }
                        else
                        {
                            elem = new Case(Convert.ToString(chaine[tc]), 0, 0);
                            pile.Pc.Add(elem); tc++; sp++;
                        }
                    }
                    else
                    {
                        // sp > tc
                        if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.SUP)
                        {
                            etat = 0; finale = false; transite = true; efinale = false;
                            mgp = ""; mdp = ""; mdpm = "";
                            while ((transite) && (!efinale))
                            {
                                mgpm = "";
                                foreach (Production i in M)
                                {
                                    efinale = true;
                                    if (etat > 0)
                                    {
                                        if ((etat < i.MDP.Count) &&
                                            ((i.MDP[etat - 1].GetType() == typeof(Terminal)) ?
                                            ((Terminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1]) :
                                            ((NonTerminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1])))
                                        {
                                            efinale = false;
                                            if (i.MDP[etat].GetType() == typeof(Terminal))
                                            {
                                                ter = (Terminal)i.MDP[etat];
                                                if (ter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                nter = (NonTerminal)i.MDP[etat];
                                                if (nter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        efinale = false;
                                        if (i.MDP[etat].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[etat];
                                            if (ter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[etat];
                                            if (nter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (transite && !efinale)
                                {
                                    mdp += pile.Pc[sp].ID;
                                    if (pile.Pc[sp].ID == "i")
                                    {
                                        val = pile.Pc[sp].VALER;
                                    }
                                    else if (pile.Pc[sp].ID == "F")
                                    {
                                        val = pile.Pc[sp].VALER;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "T")
                                    {
                                        val = pile.Pc[sp].VALER;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "E")
                                    {
                                        val = pile.Pc[sp].VALER;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "+")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "-")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "*")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "/")
                                    {
                                        val2 = val;
                                    }
                                    pile.Pc.RemoveAt(sp);
                                    sp--;
                                }
                            }
                            finale = false;
                            foreach (Production i in M)
                            {
                                if (i.MDP.Count == mdp.Length)
                                {
                                    for (int j = 0; j < i.MDP.Count; j++)
                                    {
                                        if (i.MDP[j].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[j];
                                            mdpm += ter.ID;
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[j];
                                            mdpm += nter.ID;
                                        }
                                    }
                                    if (mdp == mdpm)
                                    {
                                        finale = true;
                                        mgp = i.MGP.ID;
                                        break;
                                    }
                                    else
                                    {
                                        mgp = "";
                                        mdpm = "";
                                    }
                                }
                            }
                            if (finale && (AXIOME == mgp) && (chaine[tc] == '#') && (pile.Pc[sp].ID == "#"))
                            {
                                accepter = true;
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                        resultat = val1 + val2;
                                    else if (mdp[1] == '-')
                                        resultat = val1 - val2;
                                }
                                System.Windows.MessageBox.Show(Convert.ToString(resultat), "resulat");

                            }
                            else if (finale)
                            {
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                    {
                                        resultat = val1 + val2;
                                    }
                                    else if (mdp[1] == '-')
                                    {
                                        resultat = val1 - val2;
                                    }
                                    else if (mdp[1] == '*')
                                    {
                                        resultat = val1 * val2;
                                    }
                                    else if (mdp[1] == '/')
                                    {
                                        resultat = val1 / val2;
                                    }
                                    elem = new Case(mgp, 0, resultat);
                                    //elem.VALE = resultat;
                                }
                                else
                                {
                                    elem = new Case(mgp, 0, val);
                                    //elem.VALE = val;
                                }

                                pile.Pc.Add(elem); sp++;
                                
                            }


                        }
                        else
                        {
                            erreur = true;
                        }
                    }
                }
                else
                {
                    erreur = true;
                }

            }
            #endregion ETIQ

            if (accepter)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public Boolean ERSourceC(String chaine)
        {
            int ind = 0;
            decimal[] valeurs = decRemover(ref chaine);
            List<Production> M = new List<Production>();
            List<Production> Travail = new List<Production>();
            Production p;
            Terminal ter = null;
            NonTerminal nter = null;
            Pile pile = new Pile("#", true);
            int sp = 0, tc = 0, etat = 0;
            Boolean erreur = false, accepter = false, finale = false, transite = true,
                    efinale = false;
            String mgp = "", mgpm = "", mdp = "", mdpm = "";
            Case elem;
            decimal val = 0, val1 = 0, val2 = 0, resultat = 0;

            // Construction de AFD miroir des MDPs
            foreach (Production i in PRODUCTIONS)
            {
                p = new Production(i.MGP);
                p.MDP = new ArrayList();
                for (int j = (i.MDP.Count - 1); j >= 0; j--)
                {
                    p.MDP.Add(i.MDP[j]);
                }
                M.Add(p);
            } // Find de Construction de AFD miroir.
            #region ETIQ

            while ((!erreur) && (!accepter))
            {

                if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])].Count != 0)
                {
                    // sp < tc
                    if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.INF)
                    {
                        if (chaine[tc] == 'i')
                        {
                            elem = new Case("i", 0,valeurs[ind]);
                            //elem.VALE = valeurs[ind];
                            pile.Pc.Add(elem); tc++; sp++; ind++;
                        }
                        else
                        {
                            elem = new Case(Convert.ToString(chaine[tc]), 0, 0);
                            pile.Pc.Add(elem); tc++; sp++;
                        }
                        Console.WriteLine();
                        foreach (Case caso in pile.Pc)
                        {
                            if ((caso.ID != "#") && (caso.ID != "+") && (caso.ID != "-") && (caso.ID != "*") && (caso.ID != "/"))
                            {
                                Console.Write(caso.ID + " [ " + Convert.ToString(caso.VALER) + " ] ");
                            }
                            else
                            {
                                Console.Write("#");
                            }
                        }
                    }
                    else
                    {
                        // sp > tc
                        if (TDR[pile.Pc[sp].ID][Convert.ToString(chaine[tc])][0] == Relation.SUP)
                        {
                            etat = 0; finale = false; transite = true; efinale = false;
                            mgp = ""; mdp = ""; mdpm = "";
                            while ((transite) && (!efinale))
                            {
                                mgpm = "";
                                foreach (Production i in M)
                                {
                                    efinale = true;
                                    if (etat > 0)
                                    {
                                        if ((etat < i.MDP.Count) &&
                                            ((i.MDP[etat - 1].GetType() == typeof(Terminal)) ?
                                            ((Terminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1]) :
                                            ((NonTerminal)i.MDP[etat - 1]).ID == Convert.ToString(mdp[etat - 1])))
                                        {
                                            efinale = false;
                                            if (i.MDP[etat].GetType() == typeof(Terminal))
                                            {
                                                ter = (Terminal)i.MDP[etat];
                                                if (ter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                nter = (NonTerminal)i.MDP[etat];
                                                if (nter.ID == pile.Pc[sp].ID)
                                                {
                                                    transite = true;
                                                    mgpm = i.MGP.ID;
                                                    etat++;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        efinale = false;
                                        if (i.MDP[etat].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[etat];
                                            if (ter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[etat];
                                            if (nter.ID == pile.Pc[sp].ID)
                                            {
                                                transite = true;
                                                mgpm = i.MGP.ID;
                                                etat++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (transite && !efinale)
                                {
                                    mdp += pile.Pc[sp].ID;
                                    if (pile.Pc[sp].ID == "i")
                                    {
                                        val = pile.Pc[sp].VALER;
                                    }
                                    else if (pile.Pc[sp].ID == "F")
                                    {
                                        val = pile.Pc[sp].VALER;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "T")
                                    {
                                        val = pile.Pc[sp].VALER;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "E")
                                    {
                                        val = pile.Pc[sp].VALER;
                                        val1 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "+")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "-")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "*")
                                    {
                                        val2 = val;
                                    }
                                    else if (pile.Pc[sp].ID == "/")
                                    {
                                        val2 = val;
                                    }
                                    pile.Pc.RemoveAt(sp);
                                    Console.WriteLine();
                                    foreach (Case caso in pile.Pc)
                                    {
                                        if (caso.ID != "#")
                                        {
                                            Console.Write(caso.ID + " [ " + Convert.ToString(caso.VALER) + " ] ");
                                        }
                                        else
                                        {
                                            Console.Write("#");
                                        }
                                    }
                                    sp--;
                                }
                            }
                            finale = false;
                            foreach (Production i in M)
                            {
                                if (i.MDP.Count == mdp.Length)
                                {
                                    for (int j = 0; j < i.MDP.Count; j++)
                                    {
                                        if (i.MDP[j].GetType() == typeof(Terminal))
                                        {
                                            ter = (Terminal)i.MDP[j];
                                            mdpm += ter.ID;
                                        }
                                        else
                                        {
                                            nter = (NonTerminal)i.MDP[j];
                                            mdpm += nter.ID;
                                        }
                                    }
                                    if (mdp == mdpm)
                                    {
                                        finale = true;
                                        mgp = i.MGP.ID;
                                        break;
                                    }
                                    else
                                    {
                                        mgp = "";
                                        mdpm = "";
                                    }
                                }
                            }
                            if (finale && (AXIOME == mgp) && (chaine[tc] == '#') && (pile.Pc[sp].ID == "#"))
                            {
                                accepter = true;
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                        resultat = val1 + val2;
                                    else if (mdp[1] == '-')
                                        resultat = val1 - val2;
                                }
                                //System.Windows.MessageBox.Show(Convert.ToString(resultat), "resulat");

                            }
                            else if (finale)
                            {
                                if (mdp.Length > 1)
                                {
                                    if (mdp[1] == '+')
                                    {
                                        resultat = val1 + val2;
                                    }
                                    else if (mdp[1] == '-')
                                    {
                                        resultat = val1 - val2;
                                    }
                                    else if (mdp[1] == '*')
                                    {
                                        resultat = val1 * val2;
                                    }
                                    else if (mdp[1] == '/')
                                    {
                                        resultat = val1 / val2;
                                    }
                                    elem = new Case(mgp, 0, resultat);
                                    //elem.VALE = resultat;
                                }
                                else
                                {
                                    elem = new Case(mgp, 0, val);
                                    //elem.VALE = val;
                                }

                                pile.Pc.Add(elem); sp++;
                                Console.WriteLine();
                                foreach (Case caso in pile.Pc)
                                {
                                    if ((caso.ID != "#") && (caso.ID != "+") && (caso.ID != "-") && (caso.ID != "*") && (caso.ID != "/"))
                                    {
                                        Console.Write(caso.ID + " [ " + Convert.ToString(caso.VALER) + " ] ");
                                    }
                                    else
                                    {
                                        Console.Write("#");
                                    }
                                }
                            }


                        }
                        else
                        {
                            erreur = true;
                        }
                    }
                }
                else
                {
                    erreur = true;
                }

            }
            #endregion ETIQ

            if (accepter)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public void GenerateCS(String chemin)
        {
            String sourceC = "";
            @sourceC = @"using System;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Text;
                        using System.Threading.Tasks;

                        namespace CodeGenerer
                        {
                            class Program
                            {
                                static void Main(string[] args)
                                {" +

                        "Grammaire g;String Source;Source =@" + @""""+ Source + @""""+
                        @";g = new Grammaire( Source);
                          g.TExtraction();
                          g.NTExtraction();
                          g.PExtraction();
                          g.EPCalcul();
                          g.EDebCalcul();
                          g.EDerCalcul();
                          g.TRCalcul();
                          String Chaine = """";
                          if ((g.ISEVAL == true) && (g.ISINT))
                          {
                              Console.WriteLine(""Voiez entrez votre chaine:"");
                              Chaine = Console.ReadLine();
                              if (g.EESourceC(Chaine + ""#""))
                              {
                                  Console.WriteLine(""Chaine accepter!"");
                              }
                              else
                              {
                                  Console.WriteLine(""Chaine non accepter!"");
                              }
                          }
                          else if((g.ISEVAL == true) && (!g.ISINT))
                          {
                              Console.WriteLine(""Voiez entrez votre chaine:"");
                              Chaine = Console.ReadLine();
                              if (g.ERSourceC(Chaine + ""#""))
                              {
                                  Console.WriteLine(""Chaine accepter!"");
                              }
                              else
                              {
                                  Console.WriteLine(""Chaine non accepter!"");
                              }     
                          }
                          else
                          {
                              Console.WriteLine(""Voiez entrez votre chaine:"");
                              Chaine = Console.ReadLine();
                              if (g.PSource(Chaine + ""#""))
                              {
                                  Console.WriteLine(""Chaine accepter!"");
                              }
                              else
                              {
                                  Console.WriteLine(""Chaine non accepter!"");
                              }
                          }Console.ReadKey();}}}"

                                    ;
            FileStream fs = new FileStream(@chemin, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(sourceC);
            sw.Close(); 
            fs.Close();



        }
    }                                                     
}
