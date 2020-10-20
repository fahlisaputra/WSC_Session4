using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSC_Session4.Class
{
    class Global
    {
        public static Session4Entities db = new Session4Entities();
        public static void InfoAlert(String message)
        {
            MessageBox.Show(message, "Kazan Neft", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void WarningAlert(String message)
        {
            MessageBox.Show(message, "Kazan Neft", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void ErrorAlert(String message)
        {
            MessageBox.Show(message, "Kazan Neft", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static bool ChoiceAlert(String message)
        {
            if (MessageBox.Show(message, "Kazan Neft", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                return true;
            }
            return false;
        }
    }

    public static class Extension
    {
        public static bool isNumber(this String value)
        {
            try
            {
                int trying = int.Parse(value);
                return true;
            } catch(Exception e)
            {
                return false;
            }
        }
    }
}
