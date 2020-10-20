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
    public partial class PurchaseOrderForm : Form
    {
        OrderItem orderItem;
        List<OrderItem> selectedParts = new List<OrderItem>();
        List<OrderItem> removedParts = new List<OrderItem>();

        public PurchaseOrderForm(OrderItem orderItem)
        {
            InitializeComponent();
            this.orderItem = orderItem;
        }

        void initData()
        {
            var suppliers = Global.db.Suppliers.Select(x => new
            {
                x.ID,
                x.Name
            }).ToArray();

            var warehouse = Global.db.Warehouses.Select(x => new
            {
                x.ID,
                x.Name
            }).ToArray();

            var parts = Global.db.Parts.Select(x => new
            {
                x.ID,
                x.Name
            }).ToArray();

            cbSuppliers.ValueMember = "ID";
            cbSuppliers.DisplayMember = "Name";

            cbWarehouse.ValueMember = "ID";
            cbWarehouse.DisplayMember = "Name";

            cbPart.ValueMember = "ID";
            cbPart.DisplayMember = "Name";

            cbSuppliers.DataSource = suppliers;
            cbWarehouse.DataSource = warehouse;
            cbPart.DataSource = parts;
        }

        void loadParts()
        {
            dgv.DataSource = null;
            dgv.Columns.Clear();
            var source = selectedParts.Select(x => new
            {
                x.ID,
                x.Part.Name,
                x.BatchNumber,
                x.Amount
            }).ToArray();

            dgv.DataSource = source;
            dgv.Columns[0].Visible = false;
            dgv.Columns[1].HeaderText = "Part Name";
            dgv.Columns[2].HeaderText = "Batch Number";
            dgv.Columns[3].HeaderText = "Amount";

            var btn = new DataGridViewButtonColumn
            {
                Name = "Remove",
                Text = "Remove",
                HeaderText = "Action",
                UseColumnTextForButtonValue = true
            };

            dgv.Columns.AddRange(btn);
        }
        private void PurchaseOrderForm_Load(object sender, EventArgs e)
        {
            initData();

            if (orderItem == null)
            {

            } else
            {
                var existingParts = Global.db.OrderItems.Where(x => x.OrderID == orderItem.OrderID).Select(x => new
                {
                    x.ID,
                    x.OrderID,
                    x.PartID,
                    x.BatchNumber,
                    x.Amount
                }).ToArray();

                foreach(var item in existingParts)
                {
                    var part = Global.db.OrderItems.Find(item.ID);
                    selectedParts.Add(part);
                }

                cbSuppliers.SelectedValue = orderItem.Order.SupplierID;
                cbWarehouse.SelectedValue = orderItem.Order.DestinationWarehouseID;
                datePicker.Value = orderItem.Order.Date;
            }

            loadParts();
        }

        private void cbPart_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(cbPart.SelectedValue);
            var item = Global.db.Parts.Find(id);

            if (item != null)
            {
                if (item.BatchNumberHasRequired == true)
                {
                    txtBatchNumber.Enabled = true;
                    txtBatchNumber.Text = "";
                } else
                {
                    txtBatchNumber.Enabled = false;
                    txtBatchNumber.Text = "";
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!txtAmount.Text.isNumber())
            {
                Global.WarningAlert("Please input a valid amount");
                return;
            }

            if (txtBatchNumber.Enabled == true && txtBatchNumber.Text == "")
            {
                Global.WarningAlert("Batch Number required for this part");
                return;
            }

            var part = Global.db.Parts.Find(Convert.ToInt32(cbPart.SelectedValue));

            int destination = Convert.ToInt32(cbWarehouse.SelectedValue);
            if (part != null)
            {
                
                if (Convert.ToInt32(txtAmount.Text) < part.MinimumAmount)
                {
                    Global.ErrorAlert("Minimum amount is " + part.MinimumAmount);
                    return;
                }
                var exist = selectedParts.FirstOrDefault(x => x.PartID == part.ID && x.BatchNumber == txtBatchNumber.Text);
                if (exist == null)
                {
                    exist = new OrderItem();
                    exist.PartID = part.ID;
                    exist.Part = part;
                    exist.BatchNumber = txtBatchNumber.Text;
                    exist.Amount = Convert.ToInt32(txtAmount.Text);
                    selectedParts.Add(exist);
                    loadParts();
                } else
                {
                    int index = selectedParts.IndexOf(exist);
                    selectedParts[index].Amount = selectedParts[index].Amount + Convert.ToInt32(txtAmount.Text);
                    loadParts();
                }
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (selectedParts.Count < 1)
            {
                Global.WarningAlert("There is no selected parts");
                return;
            }

            int supplier = Convert.ToInt32(cbSuppliers.SelectedValue);
            int warehouse = Convert.ToInt32(cbWarehouse.SelectedValue);

            if (orderItem == null)
            {
                var order = new Order
                {
                    TransactionTypeID = 1,
                    SupplierID = supplier,
                    DestinationWarehouseID = warehouse,
                    Date = datePicker.Value.Date
                };
                Global.db.Orders.Add(order);
                Global.db.SaveChanges();     

                foreach(var part in selectedParts)
                {

                    var exist = new OrderItem();
                    exist.OrderID = order.ID;
                    exist.PartID = part.PartID;
                    exist.BatchNumber = part.BatchNumber;
                    exist.Amount = part.Amount;
                    Global.db.OrderItems.Add(exist);
                    Global.db.SaveChanges();

                }

                this.Close();
            } else
            {
                var order = Global.db.Orders.FirstOrDefault(x => x.ID == orderItem.OrderID);
                if (order != null)
                {
                    order.TransactionTypeID = 1;
                    order.SupplierID = supplier;
                    order.DestinationWarehouseID = warehouse;
                    order.Date = datePicker.Value.Date;
                    Global.db.SaveChanges();

                    foreach(var part in removedParts)
                    {
                        var exist = Global.db.OrderItems.Find(part.ID);
                        if (exist != null)
                        {
                            Global.db.OrderItems.Remove(exist);
                            Global.db.SaveChanges();
                        }
                    }

                    foreach(var part in selectedParts)
                    {
                        var exist = Global.db.OrderItems.Find(part.ID);
                        if (exist == null)
                        {
                            exist = new OrderItem();
                            exist.OrderID = order.ID;
                            exist.PartID = part.PartID;
                            exist.BatchNumber = part.BatchNumber;
                            exist.Amount = part.Amount;

                            Global.db.OrderItems.Add(exist);
                            Global.db.SaveChanges();
                        }
                    }

                    this.Close();
                }
            }
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name == "Remove")
            {
                int id = Convert.ToInt32(dgv[0, e.RowIndex].Value.ToString());
                String batch = dgv[2, e.RowIndex].Value.ToString();
                var item = selectedParts.FirstOrDefault(x => x.ID == id && x.BatchNumber == batch);
                if (item != null)
                {
                    if (Global.ChoiceAlert("Are you sure want to remove selected data?") == true)
                    {
                        selectedParts.Remove(item);
                        removedParts.Add(item);
                        loadParts();
                    }
                }
            }
        }
    }
}
