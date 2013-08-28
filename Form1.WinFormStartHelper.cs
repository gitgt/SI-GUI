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
using System.Threading;

namespace SI_GUI
{
    // This file helps to start all the subwindows with the right l10n strings
    partial class Form1 : Form
    {
        string[] langAvailable = new string[] {
            "En",
            "De",
            "Fr",
            "Es",
            "Sl",
            "Da",
            "He",
            "Pt",
            "Nl",
            "Gl",
            "It",
            "Lo-LA",
            "Mt-MT",
            "Tr"
          };
        // Opens Help
        private void openHelp()
        {
            piwik.sendFeatreUseageStats(TDFPiwik.Features.OpenDialog_Help);
            string[] l10n = new string[3];
            l10n[0] = getstring("standarderror");
            l10n[1] = getstring("Error");
            l10n[2] = getstring("help");
            Form3 fm = new Form3(l10n, rtl_layout, Thread.CurrentThread.CurrentUICulture.Name.ToLower());
            fm.ShowDialog();
        }
        // Opens Manager
        private void openManager()
        {
            piwik.sendFeatreUseageStats(TDFPiwik.Features.OpenDialog_Manager);
            string[] l10n_manager = new string[6];
            l10n_manager[0] = getstring("man_title");
            l10n_manager[1] = getstring("man_exit");
            l10n_manager[2] = getstring("man_addinstall");
            l10n_manager[3] = getstring("man_del");
            l10n_manager[4] = getstring("standarderror");
            l10n_manager[5] = getstring("Error");
            string[] l10n_mai = new string[7];
            l10n_mai[0] = getstring("standarderror");
            l10n_mai[1] = getstring("Error");
            l10n_mai[2] = getstring("mai_path_soffice");
            l10n_mai[3] = getstring("mai_config_path");
            l10n_mai[4] = getstring("mai_text");
            l10n_mai[5] = getstring("okay");
            l10n_mai[6] = getstring("mai_abort");
            Manager window = new Manager(l10n_manager, l10n_mai, rtl_layout);
            window.ShowDialog();
        }

        // Opens About / Settings
        private void openSettings()
        {
            piwik.sendFeatreUseageStats(TDFPiwik.Features.OpenDialog_About);
            string[] l10n = new string[26];
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
            l10n[19] = getstring("ga_cb_allowed");
            l10n[20] = getstring("settings_select_dl_folder");
            l10n[21] = getstring("cb_advanced_file");
            l10n[22] = getstring("tt_advanced_file");
            l10n[23] = getstring("okay");
            l10n[24] = getstring("cb_autoedit");
            l10n[25] = getstring("help");
            Form2 fm = new Form2(l10n, rtl_layout, langAvailable);
            fm.ShowDialog();
            path_4_download = fm.get_download_location;
            AdvancedFilenames = fm.getAdvancedRenamingChecked;
        }

    }
}
