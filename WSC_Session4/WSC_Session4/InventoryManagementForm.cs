using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WSC_Session4.Class;

namespace WSC_Session4
{
    public partial class InventoryManagementForm : Form
    {
        DataGridViewColumn SORTED_COLUMN;
        int SORT_DIRECTION;
        int SORT_COL_INDEX;
        public InventoryManagementForm()
        {
            InitializeComponent();
        }
        void loadData()
        {
            dgv.Columns.Clear();
            dgv.DataSource = null;
            var source = Global.db.OrderItems
                .ToArray()
                .Select(x => new
                {
                    x.ID,
                    x.Part.Name,
                    type = x.Order.TransactionType.Name,
                    x.Order.Date,
                    x.Amount,
                    source = x.Order.SourceWarehouseID == null ? "-" : x.Order.Warehouse.Name,
                    destination = x.Order.DestinationWarehouseID == null ? "-" : x.Order.Warehouse1.Name
                }).ToArray();
            source = source.OrderByDescending(a => a.Date).ThenBy(a => a.type).ToArray();

            if (SORTED_COLUMN != null)
            {
                switch (SORT_COL_INDEX)
                {
                    case 1:
                        if (SORT_DIRECTION == 0)
                        {
                            source = source.OrderBy(a => a.Name).ToArray();
                        }
                        else
                        {
                            source = source.OrderByDescending(a => a.Name).ToArray();
                        }
                        break;

                    case 2:
                        if (SORT_DIRECTION == 0)
                        {
                            source = source.OrderBy(a => a.type).ToArray();
                        }
                        else
                        {
                            source = source.OrderByDescending(a => a.type).ToArray();
                        }
                        break;

                    case 3:
                        if (SORT_DIRECTION == 0)
                        {
                            source = source.OrderBy(a => a.Date).ToArray();
                        }
                        else
                        {
                            source = source.OrderByDescending(a => a.Date).ToArray();
                        }
                        break;

                    case 4:
                        if (SORT_DIRECTION == 0)
                        {
                            source = source.OrderBy(a => a.Amount).ToArray();
                        }
                        else
                        {
                            source = source.OrderByDescending(a => a.Amount).ToArray();
                        }
                        break;

                    case 5:
                        if (SORT_DIRECTION == 0)
                        {
                            source = source.OrderBy(a => a.source).ToArray();
                        }
                        else
                        {
                            source = source.OrderByDescending(a => a.source).ToArray();
                        }
                        break;

                    case 6:
                        if (SORT_DIRECTION == 0)
                        {
                            source = source.OrderBy(a => a.destination).ToArray();
                        }
                        else
                        {
                            source = source.OrderByDescending(a => a.destination).ToArray();
                        }
                        break;

                    default:
                        break;
                }
            }

            BindingSource binding = new BindingSource();
            binding.DataSource = source;
            dgv.DataSource = binding;

            dgv.Columns[0].Visible = false;
            dgv.Columns[1].HeaderText = "Part Name";
            dgv.Columns[2].HeaderText = "Transaction Type";
            dgv.Columns[3].HeaderText = "Date";
            dgv.Columns[4].HeaderText = "Amount";
            dgv.Columns[5].HeaderText = "Source";
            dgv.Columns[6].HeaderText = "Destination";
            dgv.Columns[3].Width = 70;
            dgv.Columns[4].Width = 70;
            dgv.Columns[5].Width = 100;
            dgv.Columns[6].Width = 100;
            

            foreach(DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells[2].Value.ToString() == "Purchase Order")
                {
                    row.Cells[4].Style.BackColor = Color.SpringGreen;
                }
            }

            var btnEdit = new DataGridViewLinkColumn
            {
                Name = "Edit",
                Text = "Edit",
                UseColumnTextForLinkValue = true,
                HeaderText = "Action"
            };

            var btnRemove = new DataGridViewLinkColumn
            {
                Name = "Remove",
                Text = "Remove",
                UseColumnTextForLinkValue = true,
                HeaderText = "Action"
            };

            dgv.Columns.AddRange(btnEdit);
            dgv.Columns.AddRange(btnRemove);

            dgv.Columns[7].Width = 40;
            dgv.Columns[8].Width = 60;

            SORTED_COLUMN = dgv.Columns[SORT_COL_INDEX];
            if (SORTED_COLUMN != null)
            {
                SORTED_COLUMN.HeaderCell.SortGlyphDirection = SORT_DIRECTION == 0 ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.Descending;
            }
        }
     
        private void InventoryManagementForm_Load(object sender, EventArgs e)
        {
            loadData();
        }

        private void dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn newColumn = dgv.Columns[e.ColumnIndex];
            DataGridViewColumn OLD_SORT = SORTED_COLUMN;
            SORT_COL_INDEX = e.ColumnIndex;
            if (SORT_COL_INDEX == 7 || SORT_COL_INDEX == 8)
            {

            } else
            {
                if (SORTED_COLUMN != null)
                {
                    if (newColumn == SORTED_COLUMN)
                    {
                        if (SORT_DIRECTION == 0)
                        {
                            SORT_DIRECTION = 1;
                        }
                        else
                        {
                            SORT_DIRECTION = 0;
                        }
                    }
                    else
                    {
                        SORT_DIRECTION = 0;
                        SORTED_COLUMN = newColumn;
                    }
                }
                else
                {
                    SORT_DIRECTION = 0;
                    SORTED_COLUMN = newColumn;
                }
                loadData();
            }
            
        }

        private void dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name == "Remove")
            {
                if (Global.ChoiceAlert("Are you sure want to delete this data? This action can't be undone.") == true)
                {
                    int id = int.Parse(dgv[0, e.RowIndex].Value.ToString());
                    var item = Global.db.OrderItems.Find(id);
                    Global.db.OrderItems.Remove(item);
                    Global.db.SaveChanges();
                    loadData();
                }
            }

            if (dgv.Columns[e.ColumnIndex].Name == "Edit")
            {
                int id = int.Parse(dgv[0, e.RowIndex].Value.ToString());
                var item = Global.db.OrderItems.Find(id);

                if (item.Order.TransactionTypeID == 1)
                {
                    PurchaseOrderForm purchase = new PurchaseOrderForm(item);
                    purchase.ShowDialog();
                    loadData();
                } else if (item.Order.TransactionTypeID == 2)
                {
                    WarehouseManagementForm form = new WarehouseManagementForm(item);
                    form.ShowDialog();
                    loadData();
                }              
            }
        }

        private void purchaseOrderManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PurchaseOrderForm purchase = new PurchaseOrderForm(null);
            purchase.ShowDialog();
            loadData();
        }

        private void warehouseManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WarehouseManagementForm form = new WarehouseManagementForm(null);
            form.ShowDialog();
            loadData();
        }

        private void inventoryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InventoryReportForm inventory = new InventoryReportForm();
            inventory.ShowDialog();
        }
    }
}
