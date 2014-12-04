using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode;
using ICSharpCode.AvalonEdit;
using System.Xml;
using System.IO;

namespace pfPGv1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TextEditor newfile;
        Grammaire g;
        public MainWindow()
        {
            InitializeComponent();
             
            
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            newfile = new TextEditor();
            using (FileStream s = new FileStream(Environment.CurrentDirectory.Replace(@"\bin\Debug", @"\SyntaxHilighter\parser.xshd"),FileMode.Open))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    //Load default Syntax Highlighting
                    newfile.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, 
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);

       
                }
            }
            newfile.SetValue(Grid.RowProperty, 1);
            editeurGrid.Children.Add(newfile);
            newfile.ShowLineNumbers = true;
            newfile.FontSize = 16;
            newfile.Margin = new Thickness(20, 0, 20, 0);
            newfile.TextArea.BorderThickness = new Thickness(20);
            newfile.TextArea.BorderBrush = Brushes.Black;
            consoleText.Text = "Console:\n-------\n";
            information.Text = "Information:\n-----------\n";
            
           
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
              GrammaireYacc analyse = new GrammaireYacc(newfile.Text);
              if (analyse.VerifierGY())
              {
                  consoleText.Text += "grammaire correct!\n";
                  g = new Grammaire(newfile.Text);
                  g.TExtraction();
                  g.NTExtraction();
                  g.PExtraction();
                  g.EPCalcul();
                  string s="", t = "", p="";
                  int compteur = g.TERMINAUX.Count;
                  foreach (Terminal i in g.TERMINAUX)
                  {
                      compteur--;
                      if (compteur != 0)
                          s += i.ID + ", ";
                      else
                          s += i.ID + " }\n";
                      
                  }
                  compteur = g.NONTERMINAUX.Count;
                  foreach (NonTerminal i in g.NONTERMINAUX)
                  {
                      compteur--;
                      if (compteur != 0)
                          t += i.ID + ", ";
                      else
                          t += i.ID + " }\n";
                      
                  }
                  string premier =  "";
                  foreach (String z in g.PREMIER.Keys)
                  {
                      premier += z + "  = [ ";
                      foreach (Tuple mem in g.PREMIER[z])
                      {
                          premier += mem.ID + ", ";
                      }
                      premier += "]\n";                      
                  }
                 
                  g.EDebCalcul();
                  string debut = "";
                  foreach (String z in g.DEBUT.Keys)
                  {
                      debut += z + "  = [ ";
                      foreach (String mem in g.DEBUT[z])
                      {
                          debut += mem + ", ";
                      }
                      debut += "]\n";
                  }
                  

                  g.EDerCalcul();
                  string der = "";
                  foreach (String z in g.DERNIER.Keys)
                  {
                      der += z + "  = [ ";
                      foreach (Tuple mem in g.DERNIER[z])
                      {
                          der += mem.ID + ", ";
                      }
                      der += "]\n";
                  }
                  information.Text = "Terminaux = { " + s + "Non terminaux = { " + t +
                                     "Premiers :\n--------\n" + premier + "Derniers :\n-------\n" +
                                     der + "Debuts :\n------\n" + debut;

              }

              else
                  consoleText.Text += "grammaire incorrect!\n";
        }

        private void PF_Click(object sender, RoutedEventArgs e)
        {
            Production epsilonP;
            String iden = "";
            Boolean isPF = true;
            int dim;
            if ((epsilonP = g.EpsilonProduction()) != null)
            {
                consoleText.Text += "Grammaire n'est pas de précedence faible : " +
                                    epsilonP.MGP.ID + " -> " + " £ " + "\n";
                isPF = false;
            }
            if ((isPF) && ((iden = g.MDPIdentique()) != ""))
            {
                consoleText.Text += "Grammaire n'est pas de précedence faible : \n" +
                                     "éxistance des MDPS identiques : "+iden+"\n";
                isPF = false;
            }
            if (isPF)
            {
                g.TRCalcul();
                dim = g.INDEXES.Count;
                tdr.Text = "";
                for (int i = 0; i < (dim); i++)
                {
                    tdr.Text += "\t" + g.INDEXES[i];
                }
                tdr.Text += "\n";
                int cptr = 0;
                for (int i = 0; i < (dim); i++)
                {
                    tdr.Text += g.INDEXES[i];
                    for (int j = 0; j < (dim); j++)
                    {
                        if (g.TDR[g.INDEXES[i]][g.INDEXES[j]].Count > 0)
                        {
                            tdr.Text += "\t"; cptr = 0;
                            if (g.TDR[g.INDEXES[i]][g.INDEXES[j]].Count > 1)
                            {
                                isPF = false;
                                consoleText.Text += "Grammaire non PF  à cause de multidéfinition dan la table\n";
                                //tdr.IsReadOnly = false;
                                //tdr.Foreground.SetValue(TextBox.ForegroundProperty, Brushes.Red);
                                foreach (Relation r in g.TDR[g.INDEXES[i]][g.INDEXES[j]])
                                {
                                    if (cptr > 0)
                                        tdr.Text += ",";
                                    if (r == Relation.INF)
                                    {
                                        tdr.Text += "<";
                                    }
                                    else if (r == Relation.SUP)
                                    {
                                        tdr.Text += ">";
                                    }
                                    cptr++;
                                }
                               // tdr.Foreground.SetValue(TextBox.ForegroundProperty, Brushes.Black);
                                //tdr.IsReadOnly = true;
                            }
                            else
                            {
                                foreach (Relation r in g.TDR[g.INDEXES[i]][g.INDEXES[j]])
                                {
                                    if (r == Relation.INF)
                                    {
                                        tdr.Text += "<";
                                    }
                                    else if (r == Relation.SUP)
                                    {
                                        tdr.Text += ">";
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            tdr.Text += "\t" + "ER";
                        }
                    }
                    tdr.Text += "\n";
                }
            }
            if (isPF)
            {
                String relsep = "";
                if ((relsep = g.VerificationRS()) != "")
                {
                    consoleText.Text += "\nGrammaire non PF car : " + relsep;
                }
            }
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            if ((g.ISEVAL == true) && (g.ISINT))
            {
                if (g.EESource(Evaluation.Text + "#"))
                {
                    MessageBox.Show("Chaine accepter");
                }
                else
                {
                    MessageBox.Show("Domage pour toi!");
                }
            }
            else if ((g.ISEVAL == true) && (!g.ISINT))
            {
                if (g.ERSource(Evaluation.Text + "#"))
                {
                    MessageBox.Show("Chaine accepter");
                }
                else
                {
                    MessageBox.Show("Domage pour toi!");
                }
            }
            else
            {
                if (g.PSource(Evaluation.Text + "#"))
                {
                    MessageBox.Show("Chaine accepter");
                }
                else
                {
                    MessageBox.Show("Domage pour toi!");
                }
            }
        }

        private void GenererFichier_Click(object sender, RoutedEventArgs e)
        {
            if (Chemin.Text != "")
            {
               @g.GenerateCS(Chemin.Text);
            }
        }
    }
}
