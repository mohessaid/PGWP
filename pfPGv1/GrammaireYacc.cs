using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace pfPGv1
{
    class GrammaireYacc
    {
        private String Text;
        private TimeSpan timeoutregx;
        private Regex reg; 
        public GrammaireYacc(String text)
        {
            this.Text = @text;
            timeoutregx = new TimeSpan(5000);
            reg = new Regex(@"\A((\n|\s|\t)*dlexic
                                     (\n|\s|\t)*
                                     (([a-z][a-z0-9]*(\n|\s|\t)*:(\n|\s|\t)*(caractere|chiffre|entier|chaine|reel|\[""[\w]*""\])(\n|\s|\t)*;(\n|\s|\t)*)*)
                                     (\n|\s|\t)*
                                     flexic)?
                                     (\n|\s|\t)*
                                     dsyntax
                                     ((\n|\s|\t)*
                                       [A-Z][A-Z0-9]*(\n|\s|\t)*:(\n|\s|\t)*(((([A-Z][A-Z0-9]*)|('[^']*')|£|([a-z][a-z0-9]*))(\n|\s|\t)*)+(\n|\s|\t)*({.*})?){1}
                                        (\n|\s|\t)*
                                        ((\|){1}(\n|\s|\t)*((([A-Z][A-Z0-9]*)|('[^']*')|£|([a-z][a-z0-9]*))(\n|\s|\t)*)+(\n|\s|\t)*({.*})?(\n|\s|\t)*)*(\n|\s|\t)*;
                                         (\n|\s|\t)*
                                       )*
                                     fsyntax
                                     \z"

                                 , RegexOptions.IgnorePatternWhitespace, timeoutregx);
        }

        public bool VerifierGY()
        {
            try
            {
                Match match;
                @match = this.reg.Match(this.Text);
                if (match.Success)
                    return true;
                else
                    return false;
            }
            catch (RegexMatchTimeoutException exception)
            {
                return false;
            }
        }



    }
}
