using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WSC_Session4.Class;
using WSC_Session4.Entity;

namespace WSC_Session4
{
    public partial class InventoryReportForm : Form
    {
        public InventoryReportForm()
        {
            InitializeComponent();
        }

        void loadData()
        {
            var source = Global.db.Warehouses.Select(x => new
            {
                x.ID,
                x.Name
            }).ToArray();

            cbWarehouse.DisplayMember = "Name";
            cbWarehouse.ValueMember = "ID";
            cbWarehouse.DataSource = source;

            loadParts();
        }
       
        void loadParts()
        {
            int warehouse = Convert.ToInt32(cbWarehouse.SelectedValue);


            var data = Global.db.Parts.Select(x => new
            {
                x.Name,
                current = (Decimal)(Global.db.OrderItems.Where(a => a.PartID == x.ID && a.Order.DestinationWarehouseID == warehouse).Sum(a => (Decimal?)a.Amount)) - (Global.db.OrderItems.Where(a => a.PartID == x.ID && a.Order.SourceWarehouseID == warehouse).Sum(a => (Decimal?)a.Amount) == null ? 0 : Global.db.OrderItems.Where(a => a.PartID == x.ID && a.Order.SourceWarehouseID == warehouse).Sum(a => (Decimal?)a.Amount)),
                received = (Decimal)Global.db.OrderItems.Where(a => a.PartID == x.ID && a.Order.DestinationWarehouseID == warehouse).Sum(a => (Decimal?)a.Amount)
            }).ToArray();

            List<Entity.ReportPart> reportParts = new List<Entity.ReportPart>();
            foreach(var item in data)
            {
                if (rbCurrent.Checked == true)
                {
                    if (item.current > 0)
                    {
                        ReportPart part = new ReportPart
                        {
                            Name = item.Name,
                            Current = item.current.Value,
                            Received = item.received
                        };
                        reportParts.Add(part);
                    }
                } else if (rbOut.Checked == true)
                {
                    if (item.current < 1)
                    {
                        ReportPart part = new ReportPart
                        {
                            Name = item.Name,
                            Current = item.current.Value,
                            Received = item.received
                        };
                        reportParts.Add(part);
                    }
                } else if (rbReceived.Checked == true)
                {
                    if (item.received > 0)
                    {
                        ReportPart part = new ReportPart
                        {
                            Name = item.Name,
                            Current = item.current.Value,
                            Received = item.received
                        };
                        reportParts.Add(part);
                    }
                }
            }
            dgv.DataSource = reportParts;
            
        }

        private void InventoryReportForm_Load(object sender, EventArgs e)
        {
            loadData();
        }

        private void cbWarehouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadParts();
        }

        private void rbCurrent_CheckedChanged(object sender, EventArgs e)
        {
            loadParts();
        }

        private void rbReceived_CheckedChanged(object sender, EventArgs e)
        {
            loadParts();
        }

        private void rbOut_CheckedChanged(object sender, EventArgs e)
        {
            loadParts();
        }
    }
}
