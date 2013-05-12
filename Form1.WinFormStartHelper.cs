﻿#region Licence
/*This file is part of the project "Reisisoft Server Install GUI",
 * which is licenced under LGPL v3+. You may find a copy in the source,
 * or obtain one at http://www.gnu.org/licenses/lgpl-3.0-standalone.html */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    // This file helps to start all the subwindows with the right l10n strings
    partial class Form1 : Form
    {
        // Opens the help
        private void openHelp()
        {
            string[] l10n = new string[3];
            l10n[0] = getstring("standarderror");
            l10n[1] = getstring("Error");
            l10n[2] = getstring("help");
            Form3 fm = new Form3(l10n);
            fm.ShowDialog();
        }
        // Opens manager
        private void openManager()
        {
            string[] l10n_manager = new string[11];
            string[] l10n_mai = new string[6];
        }

        // Opens About / change language
        private void openAbout()
        {
            string[] l10n = new string[19];
            l10n[0] = getstring("update_lang");
            l10n[1] = getstring("translations");
            l10n[2] = getstring("translator");
            l10n[3] = getstring("de");
            l10n[4] = getstring("en");
            l10n[5] = getstring("fr");
            l10n[6] = getstring("es");
            l10n[7] = getstring("he");
            l10n[8] = getstring("pt");
            l10n[9] = getstring("nl");
            l10n[10] = getstring("programmer");
            l10n[11] = getstring("sl");
            l10n[12] = getstring("da");
            l10n[13] = getstring("s_and");
            l10n[14] = getstring("about");
            l10n[15] = getstring("standarderror");
            l10n[16] = getstring("Error");
            l10n[17] = getstring("language_change_success");
            l10n[18] = getstring("success");
            Form2 fm = new Form2(l10n);
            fm.ShowDialog();
        }

        // Opens Mass DL. If true LibO LibreOffice archives will be opened, otherwise OpenOffice
        private string openMassDL(bool Libo, string[] versions, out bool goon)
        {
            string product;
            if (Libo)
                product = "LibreOffice";
            else
                product = "OpenOffice";
            string[] l10n = new string[4];
            l10n[0] = getstring("massdl_l10n_title").Replace("%product", product);
            l10n[3] = getstring("massdl_l10n_which");
            l10n[1] = getstring("okay");
            l10n[2] = getstring("abort");
            DialogResult dl;
            goon = true;
            MassDL mb = new MassDL(l10n, versions);
            dl = mb.ShowDialog();
            if (dl != System.Windows.Forms.DialogResult.OK)
                goon = false;
            return mb.getSelectedVersion;

        }
    }
}
